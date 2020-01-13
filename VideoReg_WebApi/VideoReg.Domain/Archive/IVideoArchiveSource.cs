using System;
using System.Collections.Generic;
using System.Text;
using VideoReg.Domain.Archive.ArchiveFiles;

namespace VideoReg.Domain.Archive
{
    public interface IVideoArchiveSource
    {
        FileVideoMp4[] GetCompletedVideoFiles(string pattern = "*T*_*_*_*_*_*.mp4");
    }
}
