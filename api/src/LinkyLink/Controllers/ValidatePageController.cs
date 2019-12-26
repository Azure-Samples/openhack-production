using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenGraphNet;
using LinkyLink.Models;

namespace LinkyLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Todo - Change the name of the controller
    public class ValidatePageController : Controller
    {
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
                    IEnumerable<OpenGraphResult> result = await GetMultipleGraphResults(Request, data);
                    return new OkObjectResult(result);
                }
                else if (data is JObject)
                {
                    // expecting a JSON object with url(string), id(string)
                    OpenGraphResult result = await GetGraphResult(Request, data);
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

        private async Task<OpenGraphResult> GetGraphResult(HttpRequest req, dynamic singleLinkItem)
        {
            string url = singleLinkItem.url, id = singleLinkItem.id;
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(id))
            {
                /**
                    This check is a hack in support of adding our own URLs to lists. Rendered URLs return no Open Graph
                    metadata and deep links return HTTP 404s when hosting in Blob storage. So we skip the HTTP request.
                */
                if (!req.Host.HasValue || !url.Contains(req.Host.Host))
                {
                    try
                    {
                        OpenGraph graph = await OpenGraph.ParseUrlAsync(url, "Urlist");
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(graph.OriginalHtml);
                        var descriptionMetaTag = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
                        var titleTag = doc.DocumentNode.SelectSingleNode("//head/title");
                        return new OpenGraphResult(id, graph, descriptionMetaTag, titleTag);
                    }
                    catch (Exception ex)
                    {
                        // Todo - Add logging
                    }
                }
            }
            return new OpenGraphResult { Id = id };
        }

        private async Task<IEnumerable<OpenGraphResult>> GetMultipleGraphResults(HttpRequest req, dynamic multiLinkItem)
        {
            IEnumerable<OpenGraphResult> allResults =
                await Task.WhenAll((multiLinkItem as JArray).Select(item => GetGraphResult(req, item)));

            return allResults;
        }
    }
}
