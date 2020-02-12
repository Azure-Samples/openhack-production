using HtmlAgilityPack;
using LinkyLink.Models;
using Microsoft.AspNetCore.Http;
using OpenGraphNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkyLink.Service
{
    /// <summary>
    /// Class is used to retrieve OpenGraph information for one or more links
    /// </summary>
    public class OpenGraphService : IOpenGraphService
    {
        /// <summary>
        /// Gets OpenGraph results
        /// </summary>
        /// <param name="req"></param>
        /// <param name="openGraphRequests"></param>
        /// <returns>List of OpenGraphResults</returns>        
        public async Task<IEnumerable<OpenGraphResult>> GetGraphResultsAsync(HttpRequest req, IEnumerable<OpenGraphRequest> openGraphRequests)
        {
            IEnumerable<OpenGraphResult> allResults =
                await Task.WhenAll((openGraphRequests)
                .Select(item => GetGraphResultAsync(req, item)));

            return allResults;
        }

        private async Task<OpenGraphResult> GetGraphResultAsync(HttpRequest req, OpenGraphRequest openGraphRequest)
        {
            string url = openGraphRequest.Url, id = openGraphRequest.Id;
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
    }
}
