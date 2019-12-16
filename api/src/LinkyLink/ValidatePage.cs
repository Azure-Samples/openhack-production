using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenGraphNet;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;
using LinkyLink.Models;

namespace LinkyLink
{
    public partial class LinkOperations
    {
        [FunctionName(nameof(ValidatePage))]
        public async Task<IActionResult> ValidatePage(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "validatePage")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            try
            {
                if (data is JArray)
                {
                    // expecting a JSON array of objects with url(string), id(string)
                    IEnumerable<OpenGraphResult> result = await GetMultipleGraphResults(req, data, log);
                    return new OkObjectResult(result);
                }
                else if (data is JObject)
                {
                    // expecting a JSON object with url(string), id(string)
                    OpenGraphResult result = await GetGraphResult(req, data, log);
                    return new OkObjectResult(result);
                }

                log.LogError("Invalid playload");
                ProblemDetails problemDetails = new ProblemDetails
                {
                    Title = "Could not validate links",
                    Detail = "Payload must be a valid JSON object or array",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                    Instance = req.Path
                };
                return new BadRequestObjectResult(problemDetails);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                ProblemDetails problemDetails = new ProblemDetails
                {
                    Title = "Could not validate links",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                    Instance = req.Path
                };
                return new BadRequestObjectResult(problemDetails);
            }
        }

        private async Task<OpenGraphResult> GetGraphResult(HttpRequest req, dynamic singleLinkItem, ILogger log)
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
                        log.LogError(ex, "Processing URL {URL} failed. {Message}", url, ex.Message);
                    }
                }
            }
            return new OpenGraphResult { Id = id };
        }

        private async Task<IEnumerable<OpenGraphResult>> GetMultipleGraphResults(HttpRequest req, dynamic multiLinkItem, ILogger log)
        {
            log.LogInformation("Running batch url validation");
            IEnumerable<OpenGraphResult> allResults =
                await Task.WhenAll((multiLinkItem as JArray).Select(item => GetGraphResult(req, item, log)));

            return allResults;
        }
    }
}
