using System;
using System.IO;
using VideoReg.Domain.Archive.ArchiveFiles;

namespace VideoReg.Domain.Archive
{
    public interface IVideoArchiveRep
    {
        MemoryStream GetVideoFileStream(string fileName);
        FileVideoMp4[] GetStructure(DateTime startWith);
    }
}
