using System;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;
using WebApi.Core.Archive;

namespace WebApi.Archive
{
    public interface ICameraArchiveRep
    {
        Task<ArchiveFileData> GetNearestFrontTrendFileAsync(DateTime pdt, int camera);
        Task<ArchiveFileData> TryGetVideoFileAsync(DateTime pdt, int camera);
        FileVideoMp4[] GetFullStructure(DateTime startWith = default);
        FileVideoMp4[] GetStructureByCameraNumber(int cameraNumber, DateTime startWith = default);
        FileVideoMp4[] GetFullStructureByCameraNumberAndInterval(int cameraNumber, DateTime start, DateTime end);
    }
}
