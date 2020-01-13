using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.Archive.Config
{
    public interface IChannelArchiveCacheUpdaterConfig : IChannelArchiveConfig
    {
        int UpdateChannelArchiveMs { get; }
    } 
}
