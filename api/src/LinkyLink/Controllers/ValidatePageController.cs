using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LinkyLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Todo - Change the name of the controller to something meaningful.
    /// <summary>
    /// This class handles API requests to get details of a Link such as title, description and image.
    /// </summary>
    public class ValidatePageController : Controller
    {
        private readonly IOpenGraphService _openGraphService;

        public ValidatePageController(IOpenGraphService openGraphService)
        {
            _openGraphService = openGraphService;
        }

        // POST: api/ValidatePage
        [HttpPost]
        public async Task<ActionResult<OpenGraphResult>> Post()
        {
            try
            {
                string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                if (data is JArray)
                {
                    // expecting a JSON array of objects with url(string), id(string)
                    IEnumerable<OpenGraphResult> result = await _openGraphService.GetMultipleGraphResults(Request, data);
                    return new OkObjectResult(result);
                }
                else if (data is JObject)
                {
                    // expecting a JSON object with url(string), id(string)
                    OpenGraphResult result = await _openGraphService.GetGraphResult(Request, data);
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
