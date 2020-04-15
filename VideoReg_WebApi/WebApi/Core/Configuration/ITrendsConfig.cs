namespace WebApi.Configuration
{
    public interface ITrendsConfig
    {
        string TrendsFileName { get; set; }
        string TrendsSetUrl { get; set; }
        int TrendsIterationMs { get; set; }
    }
}
