using System;
using System.IO;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Archive
{
    public interface ITrendsArchiveRep
    {
        Task<byte[]> GetTrendFileAsync(DateTime pdt);
        FileTrendsJson[] GetFullStructure(DateTime startWith = default);
        FileTrendsJson[] GetFullStructureByInterval(DateTime start, DateTime end);
    }
}
