namespace VideoReg.Domain.Archive.Config
{
    public interface IVideoArchiveCacheUpdaterConfig : IVideoArchiveConfig
    {
        int UpdateVideoArchiveMs { get; }
    } 
}
