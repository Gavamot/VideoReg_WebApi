using System;
using System.IO;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;
using WebApi.Core.Archive;

namespace WebApi.Archive
{
    public interface ITrendsArchiveRep
    {
        Task<ArchiveFileData> GetNearestFrontTrendFileAsync(DateTime pdt);
        Task<ArchiveFileData> GetTrendFileAsync(DateTime pdt);
        FileTrendsJson[] GetFullStructure(DateTime startWith = default);
        FileTrendsJson[] GetFullStructureByInterval(DateTime start, DateTime end);
    }
}
