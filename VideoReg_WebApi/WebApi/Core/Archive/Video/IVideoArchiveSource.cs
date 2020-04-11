using System.Collections.Generic;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Archive
{
    public interface IVideoArchiveSource
    {
        FileVideoMp4[] GetCompletedFiles();
    }
}
