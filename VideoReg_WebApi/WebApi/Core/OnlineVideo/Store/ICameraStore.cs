using System;
using System.Threading.Tasks;
using WebApi.Contract;

namespace WebApi.OnlineVideo.Store
{
    public interface ICameraStore
    {
        void SetCamera(int cameraNumber, byte[] img);
        /// <exception cref="NoNModifiedException">Then camera image exist but timestamp is same and all attentions is waited.</exception>
        Task<CameraResponse> GetCameraAsync(int cameraNumber, ImageSettings settings, DateTime? timeStamp);
        CameraImageTimestamp GetImage(int cameraNumber);
        int[] GetAvailableCameras();
        Action<int, byte[]> OnImageChanged { get; set; }
    }
}
