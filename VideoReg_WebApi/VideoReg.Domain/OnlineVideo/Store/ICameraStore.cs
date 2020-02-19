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
        Task<CameraResponse> GetCameraAsync(int cameraNumber, ImageTransformSettings settings, DateTime? timeStamp);
        IEnumerable<int> GetAvailableCameras();
    }
}
