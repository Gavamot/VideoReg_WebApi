namespace WebApi.Configuration
{
    public interface ITrendsConfig
    {
        string TrendsFileName { get; set; }
        string TrendsAscWebSetUrl { get; set; }
        int TrendsIterationMs { get; set; }
    }
}
