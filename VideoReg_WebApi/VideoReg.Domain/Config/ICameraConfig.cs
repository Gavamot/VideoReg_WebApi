using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.Archive.Config
{
    public interface ICameraConfig
    {
        int CameraUpdateIntervalMs { get; }
        int CameraGetImageTimeoutMs { get; }
        string Redis { get; }
    }
}
