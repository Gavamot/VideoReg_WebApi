using System;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;
using WebApi.Core.Archive;

namespace WebApi.Archive
{
    public interface ICameraArchiveRep
    {
        string GetFullArchiveFileName(ArchiveFile file);
        bool TryGetVideoFilInfo(DateTime pdt, int camera, out ArchiveFile file);
        Task<ArchiveFileData> TryGetVideoFileAsync(DateTime pdt, int camera);
        Task<ArchiveFileData> TryGetNearestFrontVideoFileAsync(DateTime pdt, int camera);
        FileVideoMp4[] GetFullStructure(DateTime startWith = default);
        FileVideoMp4[] GetStructureByCameraNumber(int cameraNumber, DateTime startWith = default);
        FileVideoMp4[] GetFullStructureByCameraNumberAndInterval(int cameraNumber, DateTime start, DateTime end);
    }
}
