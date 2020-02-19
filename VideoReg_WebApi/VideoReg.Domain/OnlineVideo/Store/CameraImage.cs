namespace VideoReg.Domain.OnlineVideo.Store
{
    public class CameraImage
    {
        public CameraImage(ImageTransformSettings settings, byte[] img)
        {
            this.settings = settings;
            this.img = img;
        }

        public readonly ImageTransformSettings settings;
        public volatile byte[] img;
    }
}