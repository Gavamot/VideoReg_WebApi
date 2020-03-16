using System;
using System.Collections.Generic;
using System.Text;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Domain.Config;

namespace VideoReg.Domain.Archive.Config
{
    public interface IConfig : ICameraConfig, IArchiveConfig, ITrendsConfig, 
        IImagePollingConfig, IBrigadeHistoryConfig, IVideoTransmitterConfig, IRegInfoConfig
    {
    }
}
