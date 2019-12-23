using Newtonsoft.Json;
using System.Collections.Generic;

namespace LinkyLink.Models
{
    public class LinkBundle
    {
        public LinkBundle()
        {
            Links = new HashSet<Link>();
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public string VanityUrl { get; set; }
        public string Description { get; set; }
        public ICollection<Link> Links { get; set; }
    }
}
