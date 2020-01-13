namespace VideoReg.Domain.Archive.Config
{
    public interface IVideoArchiveConfig
    {
        string VideoArchivePath { get; }
        int VideoArchiveUpdateTimeMs { get; }
    }
}