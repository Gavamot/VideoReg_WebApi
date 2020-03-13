using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.OnlineVideo.Store;

namespace VideoReg.Domain.Store
{
    public interface ICameraStore
    {
        void SetCamera(int cameraNumber, byte[] img);
        /// <exception cref="NoNModifiedException">Then camera image exist but timestamp is same and all attentions is waited.</exception>
        Task<CameraResponse> GetCameraAsync(int cameraNumber, ImageSettings settings, DateTime? timeStamp);
        byte[] GetOrDefaultTransformedImage(int cameraNumber);
        IEnumerable<int> GetAvailableCameras();
        Action<int, byte[]> OnImageChanged { get; set; }
    }
}
