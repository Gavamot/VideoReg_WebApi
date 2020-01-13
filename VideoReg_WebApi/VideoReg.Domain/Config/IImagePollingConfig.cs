using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.Archive.Config
{
    public interface IImagePollingConfig
    {
        int ImagePollingAttempts { get; }
        int ImagePollingDelayMs { get; }
    }
}
