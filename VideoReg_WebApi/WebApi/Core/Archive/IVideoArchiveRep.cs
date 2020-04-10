using System;
using System.IO;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Archive
{
    public interface IVideoArchiveRep
    {
        Task<byte[]> GetVideoFileStreamAsync(DateTime pdt, int camera);
        FileVideoMp4[] GetFullStructure(DateTime startWith = default);
        FileVideoMp4[] GetStructureByCameraNumber(int cameraNumber, DateTime startWith = default);
    }
}
