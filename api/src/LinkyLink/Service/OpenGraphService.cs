using HtmlAgilityPack;
using LinkyLink.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OpenGraphNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkyLink.Service
{
    public class OpenGraphService : IOpenGraphService
    {
        public async Task<OpenGraphResult> GetGraphResult(HttpRequest req, dynamic singleLinkItem)
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
                    catch (Exception)
                    {
                        // Todo - Add logging
                    }
                }
            }
            return new OpenGraphResult { Id = id };
        }

        public async Task<IEnumerable<OpenGraphResult>> GetMultipleGraphResults(HttpRequest req, dynamic multiLinkItem)
        {
            IEnumerable<OpenGraphResult> allResults =
                await Task.WhenAll((multiLinkItem as JArray)
                .Select(item => GetGraphResult(req, item)));

            return allResults;
        }
    }
}
