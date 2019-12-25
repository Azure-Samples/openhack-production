using LinkyLink.Helpers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinkyLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinksController : ControllerBase
    {
        private readonly ILinksService _linksService;

        public LinksController(ILinksService linksService)
        {
            _linksService = linksService;
        }

        // GET: api/Links
        [HttpGet]
        public async Task<IEnumerable<LinkBundle>> GetLinkBundles()
        {
            return await _linksService.AllLinkBundles();
        }

        // GET: api/Links/{vanityUrl}
        [HttpGet("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> GetLinkBundle(string vanityUrl)
        {
            // Get links for specified vanityUrl
            var linkBundle = await _linksService.FindLinkBundle(vanityUrl);

            if (linkBundle == null)
            {
                return NotFound();
            }

            return linkBundle;
        }

        // GET: api/Links/User/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<LinkBundle>> GetLinkBundlesForUser(string userId)
        {
            string userHandle = _linksService.GetUserAccountHash();

            if (string.IsNullOrEmpty(userHandle) || userHandle != userId)
            {
                return Unauthorized();
            }

            var linkBundlesForUser = await _linksService.FindLinkBundlesForUser(userId);

            if (!linkBundlesForUser.Any())
            {
                return NotFound();
            }

            var results = linkBundlesForUser.Select(d => new
            {
                userId = d.UserId,
                vanityUrl = d.VanityUrl,
                description = d.Description,
                linkCount = d.Links.Count.ToString()
            });
            return new OkObjectResult(results);
        }

        // POST: api/Links
        [HttpPost]
        public async Task<ActionResult<LinkBundle>> PostLinkBundle(LinkBundle linkBundle)
        {
            // Todo - Check if this condition is really needed
            if (!ValidatePayLoad(linkBundle, Request, out ProblemDetails problems))
            {
                return new BadRequestObjectResult(problems);
            }

            string userHandle = _linksService.GetUserAccountHash();
            linkBundle.UserId = userHandle;
            
            ValidateVanityUrl(linkBundle);

            // Todo - Move these checks to ValidateVanityUrl
            string vanity_regex = @"^([\w\d-])+(/([\w\d-])+)*$";
            Match match = Regex.Match(linkBundle.VanityUrl, vanity_regex, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return new BadRequestResult();
            }

            try
            {
                await _linksService.CreateLinkBundle(linkBundle);
            }
            catch (DbUpdateException)
            {
                if (await _linksService.LinkBundleExists(linkBundle.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetLinkBundle", new { vanityUrl = linkBundle.VanityUrl }, linkBundle);
        }

        // DELETE: api/Links/{vanityUrl}
        [HttpDelete("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> DeleteLinkBundle(string vanityUrl)
        {
            string userHandle = _linksService.GetUserAccountHash();

            if (string.IsNullOrEmpty(userHandle))
            {
                return Unauthorized();
            }

            var linkBundle = await _linksService.FindLinkBundle(vanityUrl);

            if (linkBundle == null)
            {
                return NotFound();
            }

            if (!userHandle.Equals(linkBundle.UserId, StringComparison.InvariantCultureIgnoreCase))
            {
                return Forbid();
            }

            try
            {
                await _linksService.RemoveLinkBundle(linkBundle);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        // PATCH: api/Links/{vanityUrl}
        [HttpPatch("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> PatchLinkBundle(string vanityUrl, JsonPatchDocument<LinkBundle> linkBundle)
        {
            string userHandle = _linksService.GetUserAccountHash();
            if (string.IsNullOrEmpty(userHandle))
            {
                return Unauthorized();
            }

            var linkBundleEntry = await _linksService.FindLinkBundle(vanityUrl);

            if (linkBundleEntry == null)
            {
                return NotFound();
            }

            if (!userHandle.Equals(linkBundleEntry.UserId, StringComparison.InvariantCultureIgnoreCase))
            {
                return Forbid();
            }

            try
            {
                linkBundle.ApplyTo(linkBundleEntry, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _linksService.UpdateLinkBundle(linkBundleEntry);
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        private void ValidateVanityUrl(LinkBundle linkDocument)
        {
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            
            if (string.IsNullOrWhiteSpace(linkDocument.VanityUrl))
            {
                var code = new char[7];
                var rng = new RNGCryptoServiceProvider();

                var bytes = new byte[sizeof(uint)];
                for (int i = 0; i < code.Length; i++)
                {
                    rng.GetBytes(bytes);
                    uint num = BitConverter.ToUInt32(bytes, 0) % (uint)characters.Length;
                    code[i] = characters[(int)num];
                }

                linkDocument.VanityUrl = new String(code);
            }

            // force lowercase
            linkDocument.VanityUrl = linkDocument.VanityUrl.ToLower();
        }

        private static bool ValidatePayLoad(LinkBundle linkDocument, HttpRequest req, out ProblemDetails problems)
        {
            bool isValid = (linkDocument != null) && linkDocument.Links.Count() > 0;
            problems = null;

            if (!isValid)
            {
                problems = new ProblemDetails()
                {
                    Title = "Payload is invalid",
                    Detail = "No links provided",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                    Instance = req.Path
                };
            }
            return isValid;
        }
    }
}
