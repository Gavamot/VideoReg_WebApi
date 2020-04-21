using WebApi.Contract;

namespace WebApi.OnlineVideo.Store
{
    public class CameraImage
    {
        public readonly ImageSettings Settings;
        public byte[] Image { get; set; }
        public int ConvertMs { get; set; }
        public CameraImage(ImageSettings settings, byte[] image, int convertMs)
        {
            this.Settings = settings;
            this.Image = image;
            this.ConvertMs = convertMs;
        }
    }
}