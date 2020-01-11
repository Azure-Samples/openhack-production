using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkyLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// This class handles API requests to get details of a Link such as title, description and image.
    /// </summary>
    public class OpenGraphController : Controller
    {
        private readonly IOpenGraphService _openGraphService;

        public OpenGraphController(IOpenGraphService openGraphService)
        {
            _openGraphService = openGraphService;
        }

        /// <summary>
        /// Validates and lists details for one or more urls.
        /// </summary>
        /// <remarks>
        /// Sample body:
        ///
        ///     [{
        ///        "id": 1,
        ///        "url": "https://www.microsoft.com"
        ///     }]
        ///
        /// </remarks>        
        // POST: api/OpenGraph
        [HttpPost]
        public async Task<ActionResult<OpenGraphResult>> Post(IEnumerable<OpenGraphRequest> openGraphRequests)
        {
            try
            {
                if (openGraphRequests != null && openGraphRequests.Count() > 0)
                {
                    IEnumerable<OpenGraphResult> result = await _openGraphService.GetGraphResults(Request, openGraphRequests);
                    return new OkObjectResult(result);
                }

                ProblemDetails problemDetails = new ProblemDetails
                {
                    Title = "Could not validate links",
                    Detail = "Payload must be a valid JSON object or array",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                };
                return new BadRequestObjectResult(problemDetails);
            }
            catch (Exception ex)
            {
                ProblemDetails problemDetails = new ProblemDetails
                {
                    Title = "Could not validate links",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                };
                return new BadRequestObjectResult(problemDetails);
            }
        }
    }
}
