using LinkyLink.Helpers;
using LinkyLink.Models;
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
    public class LinksController : Controller
    {
        private readonly LinksContext _context;
        protected UserAuth _userAuth;

        public LinksController(LinksContext context, UserAuth userAuth)
        {
            _context = context;
            _userAuth = userAuth;
        }

        // GET: api/Links
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LinkBundle>>> GetLinkBundles()
        {
            return await _context.LinkBundle.ToListAsync();
        }

        // GET: api/Links/{vanityUrl}
        [HttpGet("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> GetLinkBundle(string vanityUrl)
        {
            // Get links for specified vanityUrl
            var linkBundle = await _context.LinkBundle
                .SingleAsync(b => b.VanityUrl == vanityUrl.ToLower());

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
            string userHandle = _userAuth.GetUserAccountInfo().HashedID;
            if (string.IsNullOrEmpty(userHandle) || userHandle != userId)
            {
                return Unauthorized();
            }

            var linkBundlesForUser = await _context.LinkBundle
                .Where(s => s.UserId == userId)
                .ToListAsync();

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
            await CreateDatabaseAsync();

            if (!ValidatePayLoad(linkBundle, Request, out ProblemDetails problems))
            {
                return new  BadRequestObjectResult(problems);
            }

            string handle = _userAuth.GetUserAccountInfo().HashedID;
            linkBundle.UserId = handle;
            
            ValidateVanityUrl(linkBundle);

            string vanity_regex = @"^([\w\d-])+(/([\w\d-])+)*$";
            Match match = Regex.Match(linkBundle.VanityUrl, vanity_regex, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return new BadRequestResult();
            }

            await _context.LinkBundle.AddAsync(linkBundle);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LinkBundleExists(linkBundle.Id))
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
            string userHandle = _userAuth.GetUserAccountInfo().HashedID;

            if (string.IsNullOrEmpty(userHandle))
            {
                return Unauthorized();
            }

            var linkBundle = await _context.LinkBundle
                .SingleAsync(b => b.VanityUrl == vanityUrl.ToLower());

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
                _context.LinkBundle.Remove(linkBundle);
                await _context.SaveChangesAsync();
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
            string userHandle = _userAuth.GetUserAccountInfo().HashedID;
            if (string.IsNullOrEmpty(userHandle))
            {
                return Unauthorized();
            }

            var linkBundleEntry = await _context.LinkBundle
                .SingleAsync(b => b.VanityUrl == vanityUrl.ToLower());

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

                _context.Entry(linkBundleEntry).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        private bool LinkBundleExists(string id)
        {
            return _context.LinkBundle.Any(e => e.Id == id);
        }

        private async Task CreateDatabaseAsync()
        {
            await _context.Database.EnsureCreatedAsync();
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
