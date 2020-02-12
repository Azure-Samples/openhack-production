using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace LinkyLink.Models
{
    /// <summary>
    /// Data model class to represent a Link Bundle.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
