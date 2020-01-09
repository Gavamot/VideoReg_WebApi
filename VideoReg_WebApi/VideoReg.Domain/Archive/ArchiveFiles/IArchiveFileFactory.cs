using System;

namespace VideoReg.Domain.Archive.ArchiveFiles
{
    public interface IArchiveFileFactory
    {
        /// <exception cref = "FormatException">Unrecognizable file format</exception>
        FileChannelJson CreteJson(string fileFullName);

        /// <exception cref = "FormatException">Unrecognizable file format</exception>
        FileVideoMp4 CreateVideoMp4(string fileFullName, int cameraNumber);
    }
}