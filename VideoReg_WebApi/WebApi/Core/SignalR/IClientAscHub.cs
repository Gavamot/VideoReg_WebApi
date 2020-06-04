﻿using System;
using System.Threading.Tasks;
using WebApi.Contract;

namespace WebApi.OnlineVideo.OnlineVideo
{
    public interface IClientAscHub
    {
        Task ConnectWithRetryAsync();
        Task InitSessionAsync(RegInfo info);
        Task Close();

        #region Video
        //Task SendCameraImageAsync(int camera, byte[] image, int convertMs);
        Task SendNewRegInfoAsync(RegInfo info);
    
        Action<CameraSettings[]> OnInitShow { get; set; }
        Action<CameraSettings> OnSetCameraSettings { get; set; }
        #endregion
        Action OnStartTrends { get; set; }
        Action OnStopTrends { get; set; }

        Action<DateTime, DateTime> OnTrendsArchiveUploadFile { get; set; }
        Action<DateTime, DateTime, int> OnCameraArchiveUploadFile { get; set; }

        Task SendTestConnectionAsync();
    }
}
