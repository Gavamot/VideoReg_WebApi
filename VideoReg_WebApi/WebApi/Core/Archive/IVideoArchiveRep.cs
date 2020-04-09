using System;
using System.IO;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Archive
{
    public interface IVideoArchiveRep
    {
        Stream GetVideoFileStream(DateTime pdt, int camera);
        FileVideoMp4[] GetFullStructure(DateTime startWith = default);
        FileVideoMp4[] GetStructureByCameraNumber(int cameraNumber, DateTime startWith = default);
    }
}
