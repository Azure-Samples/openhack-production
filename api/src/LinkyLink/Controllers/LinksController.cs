﻿using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LinkyLink.Controllers
{
    /// <summary>
    /// This class handles API requests for Link bundles and defines set of actions to peform on Link bundles.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LinksController : ControllerBase
    {
        private readonly ILinksService _linksService;

        public LinksController(ILinksService linksService)
        {
            _linksService = linksService;
        }

        /// <summary>
        /// List all the LinkBundles.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<LinkBundle>> GetLinkBundlesAsync([FromQuery]QueryOptions queryOptions)
        {
            var linkBundles = await _linksService.AllLinkBundlesAsync(queryOptions);
            return Ok(linkBundles);
        }

        /// <summary>
        /// List a LinkBundles by Vanity Url.
        /// </summary>
        // GET: api/Links/{vanityUrl}
        [HttpGet("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> GetLinkBundleAsync(string vanityUrl)
        {
            var linkBundle = await _linksService.FindLinkBundleAsync(vanityUrl);

            if (linkBundle == null)
            {
                return NotFound();
            }

            return linkBundle;
        }

        /// <summary>
        /// List all the LinkBundles for a user.
        /// </summary>
        // GET: api/Links/User/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<LinkBundle>> GetLinkBundlesForUserAsync(string userId, [FromQuery]QueryOptions queryOptions)
        {
            string userHandle = _linksService.GetUserAccountEmail();

            if (string.IsNullOrEmpty(userHandle) || userHandle != userId)
            {
                return Unauthorized();
            }

            var linkBundlesForUser = await _linksService.FindLinkBundlesForUserAsync(userId, queryOptions);

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
            return Ok(results);
        }

        /// <summary>
        /// Create one or more LinkBundles.
        /// </summary>
        /// <remarks>
        /// Sample body:
        ///
        /// {
        /// "links": [
        ///    {
        ///    "id": "cjsorrho200023h5poelwd47z",
        ///    "url": "facebook.com",
        ///    "title": "Facebook - Log In or Sign Up",
        ///    "description": "Create an account or log into Facebook. Connect with friends, family and other people you know. Share photos and videos, send messages and get updates.",
        ///    "image": "//www.facebook.com/images/fb_icon_325x325.png"
        ///    }
        ///   ],
        /// "vanityUrl": "postman-test",
        /// "description": "",
        /// "userId": "cecilphillip"
        /// }
        /// </remarks>         
        // POST: api/Links
        [HttpPost]
        public async Task<ActionResult<LinkBundle>> PostLinkBundleAsync(LinkBundle linkBundle)
        {
            if (linkBundle.Links.Count() == 0)
            {
                return BadRequest("Invalid Payload. No Links are provided with LinkBundle.");
            }

            string userHandle = _linksService.GetUserAccountEmail();
            linkBundle.UserId = userHandle;

            ValidateVanityUrl(linkBundle);

            string vanity_regex = @"^([\w\d-])+(/([\w\d-])+)*$";
            Match match = Regex.Match(linkBundle.VanityUrl, vanity_regex, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return BadRequest("Invalid Payload. Vanity Url name is invalid.");
            }

            try
            {
                await _linksService.CreateLinkBundleAsync(linkBundle);
            }
            catch (DbUpdateException)
            {
                if (await _linksService.LinkBundleExistsAsync(linkBundle.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return CreatedAtAction("GetLinkBundle", new { vanityUrl = linkBundle.VanityUrl }, linkBundle);
        }

        /// <summary>
        /// Delete a LinkBundle by Vanity Url.
        /// </summary>
        // DELETE: api/Links/{vanityUrl}
        [HttpDelete("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> DeleteLinkBundleAsync(string vanityUrl)
        {
            string userHandle = _linksService.GetUserAccountEmail();

            if (string.IsNullOrEmpty(userHandle))
            {
                return Unauthorized();
            }

            var linkBundle = await _linksService.FindLinkBundleAsync(vanityUrl);

            if (linkBundle == null)
            {
                return NotFound();
            }

            try
            {
                await _linksService.RemoveLinkBundleAsync(linkBundle);
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Update a LinkBundle by a Vanity Url.
        /// </summary>
        // PATCH: api/Links/{vanityUrl}
        [HttpPatch("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> PatchLinkBundleAsync(string vanityUrl, JsonPatchDocument<LinkBundle> linkBundle)
        {
            string userHandle = _linksService.GetUserAccountEmail();
            if (string.IsNullOrEmpty(userHandle))
            {
                return Unauthorized();
            }

            var linkBundleEntry = await _linksService.FindLinkBundleAsync(vanityUrl);

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

                await _linksService.UpdateLinkBundleAsync(linkBundleEntry);
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        /// <summary>
        /// Validates that the Vanity Url name is not empty. If its empty, the system generates the new name. 
        /// </summary>
        /// <param name="linkDocument"></param>
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
    }
}
