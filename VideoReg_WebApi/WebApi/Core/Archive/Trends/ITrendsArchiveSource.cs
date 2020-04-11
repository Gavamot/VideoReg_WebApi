using WebApi.Archive.ArchiveFiles;

namespace WebApi.Archive
{
    public interface ITrendsArchiveSource
    {
        FileTrendsJson[] GetCompletedFiles(string pattern ="*T*_*_*_*_*.json");
    }
}
