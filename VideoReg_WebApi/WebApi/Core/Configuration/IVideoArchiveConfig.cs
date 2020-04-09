namespace WebApi.Configuration
{
    public interface IVideoArchiveConfig
    {
        string VideoArchivePath { get; }
        int VideoArchiveUpdateTimeMs { get; }
    }
}