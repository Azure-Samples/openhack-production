using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkyLink.Models
{
    public class LinkBundle
    {
        public LinkBundle()
        {
            Links = new HashSet<Link>();
        }

        [Key]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string VanityUrl { get; set; }
        public string Description { get; set; }
        public ICollection<Link> Links { get; set; }
    }
}
