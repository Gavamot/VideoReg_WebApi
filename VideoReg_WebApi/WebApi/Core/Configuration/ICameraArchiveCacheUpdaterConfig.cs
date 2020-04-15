namespace WebApi.Configuration
{
    public interface ICameraArchiveCacheUpdaterConfig : ICameraArchiveConfig
    {
        int UpdateVideoArchiveMs { get; }
    } 
}
