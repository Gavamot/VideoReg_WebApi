namespace VideoReg.Domain.OnlineVideo.Store
{
    public class CameraImage
    {
        public CameraImage(ImageSettings settings, byte[] img)
        {
            this.settings = settings;
            this.img = img;
        }

        public readonly ImageSettings settings;
        public volatile byte[] img;
    }
}