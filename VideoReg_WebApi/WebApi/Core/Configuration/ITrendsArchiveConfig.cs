namespace WebApi.Configuration
{
    public interface ITrendsArchiveConfig
    {
        string TrendsArchivePath { get; }
        int TrendsArchiveUpdateTimeMs { get; }
        string SetTrendsArchiveUrl { get; }
      
    }
}