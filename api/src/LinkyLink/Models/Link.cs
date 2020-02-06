namespace LinkyLink.Models
{
    /// <summary>
    /// Data model class to represent Link in a Link Bundle.
    /// </summary>
    public class Link
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}
