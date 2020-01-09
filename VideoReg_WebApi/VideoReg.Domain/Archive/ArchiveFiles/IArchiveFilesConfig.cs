using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.Archive.ArchiveFiles
{
    public interface IArchiveFilesConfig
    {
        string VideoArchiveDirectory { get; }
        string ChannelArchiveDirectory { get; }
    }
}
