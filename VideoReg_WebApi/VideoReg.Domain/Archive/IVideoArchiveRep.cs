using System;
using System.IO;
using VideoReg.Domain.Archive.ArchiveFiles;

namespace VideoReg.Domain.Archive
{
    public interface IVideoArchiveRep
    {
        Stream GetVideoFileStream(DateTime pdt, int camera);
        FileVideoMp4[] GetStructure(DateTime startWith);
    }
}
