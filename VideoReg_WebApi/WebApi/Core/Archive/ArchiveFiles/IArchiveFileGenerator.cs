using System;

namespace WebApi.Archive.ArchiveFiles
{
    public interface IArchiveFileGenerator
    {
        /// <exception cref = "FormatException">Unrecognizable file format</exception>
        FileChannelJson CreteJson(string fileFullName);

        /// <exception cref = "FormatException">Unrecognizable file format</exception>
        FileVideoMp4 CreateVideoMp4(string fileFullName, int cameraNumber);

        FileVideoMp4 CreateVideoMp4(DateTime pdt, int cameraNumber);
    }
}