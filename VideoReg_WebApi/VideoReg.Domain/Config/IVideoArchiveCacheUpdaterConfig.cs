using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.Archive.Config
{
    public interface IVideoArchiveCacheUpdaterConfig : IVideoArchiveConfig
    {
        int UpdateVideoArchiveMs { get; }
    } 
}
