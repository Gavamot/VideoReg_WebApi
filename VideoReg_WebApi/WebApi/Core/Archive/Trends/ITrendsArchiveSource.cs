using WebApi.Archive.ArchiveFiles;

namespace WebApi.Archive
{
    public interface ITrendsArchiveSource
    {
        FileTrendsJson[] GetCompletedFiles();
    }
}
