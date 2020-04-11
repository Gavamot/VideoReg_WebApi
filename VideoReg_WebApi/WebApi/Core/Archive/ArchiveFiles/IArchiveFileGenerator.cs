using System;

namespace WebApi.Archive.ArchiveFiles
{
    public interface IArchiveFileGenerator
    {
        /// <exception cref = "FormatException">Unrecognizable file format</exception>
        FileTrendsJson CreteJson(string fileFullName);

        /// <exception cref = "FormatException">Unrecognizable file format</exception>
        FileVideoMp4 CreateVideoMp4(string fileFullName, int cameraNumber);
    }
}