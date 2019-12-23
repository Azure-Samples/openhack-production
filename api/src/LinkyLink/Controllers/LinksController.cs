using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LinkyLink.Models;
using LinkyLink.Infrastructure;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Security.Claims;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Cosmos;
using System.IO;
using Microsoft.AspNetCore.JsonPatch;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Identity;

namespace LinkyLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinksController : Controller
    {
        private readonly linkbundles1 _context;

        protected IHttpContextAccessor _contextAccessor;
        protected Hasher _hasher;
        protected TelemetryClient _telemetryClient;

        public LinksController(linkbundles1 context, IHttpContextAccessor contextAccessor, Hasher hasher)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _hasher = hasher;
            TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.TelemetryInitializers.Add(new HeaderTelemetryInitializer(contextAccessor));
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
        }

        // GET: api/Links
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LinkBundle>>> GetLinkBundle()
        {
            return await _context.LinkBundle.ToListAsync();
        }

        // GET: api/Links/{{vanityUrl}}
        [HttpGet("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> GetLinkBundle(string vanityUrl)
        {
            var linkBundle = await _context.LinkBundle
                .SingleAsync(b => b.VanityUrl == vanityUrl.ToLower());

            if (linkBundle == null)
            {
                return NotFound();
            }

            return linkBundle;
        }

        // GET: api/Links/User/{{userId}}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<LinkBundle>> GetLinkBundlesForUser(string userId)
        {
            string userHandle = GetAccountInfo().HashedID;
            if (string.IsNullOrEmpty(userHandle) || userHandle != userId)
            {
                //log.LogInformation("Client is not authorized");
                return new UnauthorizedResult();
            }

            var linkBundlesForUser = await _context.LinkBundle
                .Where(s => s.UserId == "")
                .ToListAsync();

            if (!linkBundlesForUser.Any())
            {
                //log.LogInformation($"No links for user: '{userId}'  found.");
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
                //log.LogError(problems.Detail);
                return new BadRequestObjectResult(problems);
            }

            string handle = GetAccountInfo().HashedID;
            linkBundle.UserId = handle;
            this.EnsureVanityUrl(linkBundle);

            const string vanity_regex = @"^([\w\d-])+(/([\w\d-])+)*$";
            Match match = Regex.Match(linkBundle.VanityUrl, vanity_regex, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                // does not match
                return new BadRequestResult();
            }

            await _context.LinkBundle.AddAsync(linkBundle);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LinkBundleExists(linkBundle.VanityUrl))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            //return CreatedAtAction("GetLinkBundle", new { id = linkBundle.Id }, linkBundle);
            return new CreatedResult($"/{linkBundle.VanityUrl}", linkBundle);
        }

        // DELETE: api/Links/{{vanityUrl}}
        [HttpDelete("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> DeleteLinkBundle(string vanityUrl)
        {
            string handle = GetAccountInfo().HashedID;

            //not logged in? Bye...
            //if (string.IsNullOrEmpty(handle)) return new UnauthorizedResult();

            var linkBundle = await _context.LinkBundle
                .SingleAsync(b => b.VanityUrl == vanityUrl);

            if (linkBundle == null)
            {
                return NotFound();
            }

            try
            {
                string userId = linkBundle.UserId;

                if (!handle.Equals(userId, StringComparison.InvariantCultureIgnoreCase))
                {
                    //log.LogWarning($"{userId} is trying to delete {vanityUrl} but is not the owner.");
                    return new StatusCodeResult(StatusCodes.Status403Forbidden);
                }

                _context.LinkBundle.Remove(linkBundle);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                //log.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        // Patch: api/Links/{{vanityUrl}}
        [HttpPatch("{vanityUrl}")]
        public async Task<ActionResult<LinkBundle>> PatchLinkBundle(string vanityUrl, JsonPatchDocument<LinkBundle> linkBundle)
        {
            string handle = GetAccountInfo().HashedID;
            //if (string.IsNullOrEmpty(handle)) return new UnauthorizedResult();

            var linkBundleEntry = await _context.LinkBundle
                .SingleAsync(b => b.VanityUrl == vanityUrl.ToLower());

            if (linkBundleEntry == null)
            {
                //log.LogInformation($"Bundle for {vanityUrl} not found.");
                return NotFound();
            }

            if (linkBundle != null)
            {
                linkBundle.ApplyTo(linkBundleEntry, ModelState);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                _context.Entry(linkBundleEntry).State = EntityState.Modified;
               await _context.SaveChangesAsync();
            }

            else
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }

        private bool LinkBundleExists(string vanityUrl)
        {
            return _context.LinkBundle.Any(e => e.VanityUrl == vanityUrl);
        }

        private async Task CreateDatabaseAsync()
        {
            // TODO - Add logs
            await _context.Database.EnsureCreatedAsync();
        }

        private void EnsureVanityUrl(LinkBundle linkDocument)
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            
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

        protected UserInfo GetAccountInfo()
        {
            var socialIdentity = _contextAccessor.HttpContext.User.Identities.FirstOrDefault();

            if (socialIdentity.Claims.Count() != 0)
            {
                var provider = _contextAccessor.HttpContext.Request.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].FirstOrDefault();
                var email = _contextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var userInfo = new UserInfo(provider, _hasher.HashString(email));

                return userInfo;
            }

            return UserInfo.Empty;
        }
    }
}
