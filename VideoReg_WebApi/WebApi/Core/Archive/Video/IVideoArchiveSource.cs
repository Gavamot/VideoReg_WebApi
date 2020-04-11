using WebApi.Archive.ArchiveFiles;

namespace WebApi.Archive
{
    public interface IVideoArchiveSource
    {
        FileVideoMp4[] GetCompletedFiles(string pattern = "*T*_*_*_*_*_*.mp4");
    }
}
