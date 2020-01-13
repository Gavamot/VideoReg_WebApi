using System;
using System.Collections.Generic;
using System.Text;
using VideoReg.Domain.Config;

namespace VideoReg.Domain.Archive.Config
{
    public interface IConfig : ICameraConfig, IArchiveConfig, ITrendsConfig
    {
    }
}
