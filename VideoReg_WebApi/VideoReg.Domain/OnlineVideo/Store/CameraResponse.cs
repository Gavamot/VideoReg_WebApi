using System;

namespace VideoReg.Domain.OnlineVideo.Store
{
    public class CameraResponse
    {
        public CameraResponse() { }
        public CameraResponse(DateTime dt, byte[] img)
        {
            this.Timestamp = dt;
            this.Img = img;
        }
        public DateTime Timestamp { get; set; }
        public byte[] Img { get; set; }
    }
}
