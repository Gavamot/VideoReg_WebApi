namespace WebApi.Configuration
{
    public interface ITrendsConfig
    {
        string TrendsFileName { get; set; }
        string TrendsAscWebSet { get; set; }
        int TrendsIterationMs { get; set; }
    }
}
