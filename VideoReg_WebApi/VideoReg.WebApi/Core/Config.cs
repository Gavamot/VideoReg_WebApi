using Microsoft.Extensions.DependencyInjection;
using VideoReg.Domain.Archive.Config;

namespace VideoReg.WebApi.Core
{
    public class Config : IConfig
    {
        public string ChannelArchivePath { get; set; }
        public int UpdateChannelArchiveMs { get; set; }
        public string VideoArchivePath { get; set; }
        public int VideoArchiveUpdateTimeMs { get; set; }
        public int UpdateVideoArchiveMs { get; set; }
        public int CameraUpdateSleepIfErrorTimeoutMs { get; set; }
        public int CameraUpdateSleepIfAuthorizeErrorTimeoutMs { get; set; }
        public int CameraUpdateIntervalMs { get; set; }
        public int CameraGetImageTimeoutMs { get; set; }
        public string Redis { get;  set; }
        public string TrendsFileName { get; set; }
        public int ImagePollingAttempts { get; set; }
        public int ImagePollingDelayMs { get; set; }
        public string BrigadeHistoryFileName { get; set; }
    }
}