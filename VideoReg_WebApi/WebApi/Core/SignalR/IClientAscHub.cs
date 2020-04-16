using System;
using System.Threading.Tasks;
using WebApi.Contract;

namespace WebApi.OnlineVideo.OnlineVideo
{
    public interface IClientAscHub
    {
        Task ConnectWithRetryAsync();
        Task InitSessionAsync(RegInfo info);

        #region Video
        Task SendCameraImageAsync(int camera, byte[] image);
        Task SendNewRegInfoAsync(RegInfo info);
        Task Close();
        Action<CameraSettings[]> OnInitShow { get; set; }
        Action<int> OnStopShow { get; set; }
        Action<int> OnStartShow { get; set; }
        Action<CameraSettings> OnSetCameraSettings { get; set; }
        #endregion

        Action OnStartTrends { get; set; }
        Action OnStopTrends { get; set; }

        Action<DateTime> OnTrendsArchiveTask { get; set; }
        Action<DateTime, int> OnCameraArchiveTask { get; set; }

    }
}
