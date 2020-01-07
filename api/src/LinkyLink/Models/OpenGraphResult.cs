using HtmlAgilityPack;
using OpenGraphNet;
using System.Linq;

namespace LinkyLink.Models
{
    public class OpenGraphResult
    {
        public OpenGraphResult() { }

        public OpenGraphResult(string id, OpenGraph graph, params HtmlNode[] nodes)
        {
            Id = id;
            nodes = nodes.Where(n => n != null).ToArray();
            //Use og:title else fallback to html title tag
            var title = nodes.SingleOrDefault(n => n.Name == "title")?.InnerText.Trim();
            Title = string.IsNullOrEmpty(graph.Title) ? title : HtmlEntity.DeEntitize(graph.Title);

            Image = graph.Metadata["og:image"].FirstOrDefault()?.Value;

            //Default to og:description else fallback to description meta tag
            var descriptionData = string.Empty;
            var descriptionNode = nodes.FirstOrDefault(n => n.Attributes.Contains("content")
                                              && n.Attributes.Contains("name")
                                              && n.Attributes["name"].Value == "description");

            Description = HtmlEntity.DeEntitize(graph.Metadata["og:description"].FirstOrDefault()?.Value) ?? descriptionNode?.Attributes["content"].Value;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}
