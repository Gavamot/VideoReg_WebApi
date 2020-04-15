namespace WebApi.Configuration
{
    public interface ICameraArchiveConfig
    {
        string VideoArchivePath { get; }
        int VideoArchiveUpdateTimeMs { get; }
        string SetCameraArchiveUrl { get; }
    }
}