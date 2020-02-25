public class QueryOptions
{
    public static QueryOptions Default = new QueryOptions();

    public int Top { get; set; } = 20;
    public int Skip { get; set; } = 0;
}
