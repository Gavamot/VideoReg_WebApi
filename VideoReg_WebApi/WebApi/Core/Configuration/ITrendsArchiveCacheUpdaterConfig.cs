namespace WebApi.Configuration
{
    public interface ITrendsArchiveCacheUpdaterConfig : ITrendsArchiveConfig
    {
        int UpdateChannelArchiveMs { get; }
    } 
}
