namespace WebApi.Configuration
{
    public interface ITrendsConfig
    {
        string TrendsFileName { get; set; }
        string SetTrendsUrl { get; set; }
        int TrendsIterationMs { get; set; }
    }
}
