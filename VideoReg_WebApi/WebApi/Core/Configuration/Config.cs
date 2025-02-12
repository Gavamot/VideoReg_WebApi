﻿using Microsoft.Extensions.Logging;


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
        public string BrigadeCodePath { get; set; }

        public string TrendsFileName { get; set; }
        public string SetTrendsUrl { get; set; }
        public int TrendsIterationMs { get; set; }

        public string SetCameraArchiveUrl { get; set; }
        public string SetTrendsArchiveUrl { get; set; }
        public string SetImageUrl { get; set; }

        public void Validate(ILogger log)
        {
            ShowErrorAndThrowException(log);
            TryFixAndShowWarning(log);
        }


        private void ShowErrorAndThrowException(ILogger log)
        {
            CheckReq(log, nameof(TrendsArchivePath), TrendsArchivePath);
            CheckReq(log, nameof(VideoArchivePath), VideoArchivePath);
            CheckReq(log, nameof(BrigadeCodePath), BrigadeCodePath);
            CheckReq(log, nameof(TrendsFileName), TrendsFileName);
            CheckReq(log, nameof(BrigadeHistoryFileName), BrigadeHistoryFileName);
#if (!DEBUG)
            CheckWarning(log, nameof(UserName), UserName);
            CheckWarning(log, nameof(Password), Password);
            CheckWarning(log, nameof(Redis), Redis);
            CheckWarning(log, nameof(AscRegServiceEndpoint), AscRegServiceEndpoint);
            CheckWarning(log, nameof(SetTrendsArchiveUrl), SetTrendsArchiveUrl);
            CheckWarning(log, nameof(SetCameraArchiveUrl), SetCameraArchiveUrl);
#endif
        }

        private void CheckWarning(ILogger log, string parameterName, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                log.LogWarning($"In config parameter {parameterName} should having value.");
            }
        }

        private void CheckReq(ILogger log, string parameterName, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                log.LogCritical($"In config parameter {parameterName} must having value.");
                throw new System.Exception();
            }
        }

        private void TryFixAndShowWarning(ILogger log)
        {
            TrendsArchiveUpdateTimeMs = CheckDelay(log, nameof(TrendsArchiveUpdateTimeMs), TrendsArchiveUpdateTimeMs);
            VideoArchiveUpdateTimeMs = CheckDelay(log, nameof(VideoArchiveUpdateTimeMs), VideoArchiveUpdateTimeMs);
            CameraUpdateSleepIfErrorTimeoutMs = CheckDelay(log, nameof(CameraUpdateSleepIfErrorTimeoutMs), CameraUpdateSleepIfErrorTimeoutMs);
            CameraUpdateSleepIfAuthorizeErrorTimeoutMs = CheckDelay(log, nameof(CameraUpdateSleepIfAuthorizeErrorTimeoutMs), CameraUpdateSleepIfAuthorizeErrorTimeoutMs);
            CameraUpdateIntervalMs = CheckDelay(log, nameof(CameraUpdateIntervalMs), CameraUpdateIntervalMs);
            CameraGetImageTimeoutMs = CheckDelay(log, nameof(CameraGetImageTimeoutMs), CameraGetImageTimeoutMs);
            ImagePollingDelayMs = CheckDelay(log, nameof(ImagePollingDelayMs), ImagePollingDelayMs);
            TrendsIterationMs = CheckDelay(log, nameof(TrendsIterationMs), TrendsIterationMs);
        }

        private int CheckDelay(ILogger log, string parameterName, int value)
        {
            if (value <= 0)
            {
                log.LogWarning($"In config parameter {parameterName} should be more then 0 fix it. Now value changed to 1");
                return 1;
            }
            return value;
        }

    }
}