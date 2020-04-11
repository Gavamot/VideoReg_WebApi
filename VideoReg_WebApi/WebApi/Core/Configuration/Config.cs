using Microsoft.Extensions.DependencyInjection;
using WebApi.Configuration;


namespace WebApi.Configuration
{
    public class Config : IConfig
    {
        public string TrendsArchivePath { get; set; }
        public int TrendsArchiveUpdateTimeMs { get; set; }

        public string VideoArchivePath { get; set; }
        public int VideoArchiveUpdateTimeMs { get; set; }
        
        public string UserName { get; set; }
        public string Password { get; set; }

        public int CameraUpdateSleepIfErrorTimeoutMs { get; set; }
        public int CameraUpdateSleepIfAuthorizeErrorTimeoutMs { get; set; }
        public int CameraUpdateIntervalMs { get; set; }
        public int CameraGetImageTimeoutMs { get; set; }
        public string Redis { get;  set; }
        public int ImagePollingAttempts { get; set; }
        public int ImagePollingDelayMs { get; set; }
        public string BrigadeHistoryFileName { get; set; }
        public string AscRegServiceEndpoint { get; set; }
        public string AscRegServiceBufferSize { get; set; }
        public string BrigadeCodePath { get; set; }

        public string TrendsFileName { get; set; }
        public string TrendsAscWebSetUrl { get; set; }
        public int TrendsIterationMs { get; set; }
    }
}