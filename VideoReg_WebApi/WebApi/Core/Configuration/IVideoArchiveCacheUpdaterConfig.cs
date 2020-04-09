namespace WebApi.Configuration
{
    public interface IVideoArchiveCacheUpdaterConfig : IVideoArchiveConfig
    {
        int UpdateVideoArchiveMs { get; }
    } 
}
