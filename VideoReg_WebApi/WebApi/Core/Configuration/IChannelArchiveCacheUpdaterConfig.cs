namespace WebApi.Configuration
{
    public interface IChannelArchiveCacheUpdaterConfig : IChannelArchiveConfig
    {
        int UpdateChannelArchiveMs { get; }
    } 
}
