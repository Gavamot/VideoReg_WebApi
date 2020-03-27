using System;
using System.Threading;
using System.Threading.Tasks;
using VideoReg.Domain.Contract;

namespace VideoReg.Domain.OnlineVideo.SignalR
{
    public interface IClientVideoHub
    {
        Task ConnectWithRetryAsync();
        Task InitSessionAsync(RegInfo info);
        Task SendCameraImageAsync(int camera, byte[] image);
        Task SendNewRegInfoAsync(RegInfo info);
        Task Close();
        Action<CameraSettings[]> OnInitShow { get; set; }
        Action<int> OnStopShow { get; set; }
        Action<int> OnStartShow { get; set; }
        Action<CameraSettings> OnSetCameraSettings { get; set; }
    }
}
