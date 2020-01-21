using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.Archive.Config
{
    public interface ICameraConfig
    {
        int CameraUpdateSleepIfErrorTimeoutMs { get; }
        int CameraUpdateSleepIfAuthorizeErrorTimeoutMs { get; }
        int CameraUpdateIntervalMs { get; }
        int CameraGetImageTimeoutMs { get; }
        string Redis { get; }
    }
}
