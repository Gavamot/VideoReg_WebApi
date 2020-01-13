using System;
using System.Collections.Generic;
using System.Text;
using VideoReg.Domain.Archive.Config;

namespace VideoReg.Domain.Archive.Config
{
    public interface IArchiveConfig : IChannelArchiveConfig, IVideoArchiveConfig
    {

    }
}
