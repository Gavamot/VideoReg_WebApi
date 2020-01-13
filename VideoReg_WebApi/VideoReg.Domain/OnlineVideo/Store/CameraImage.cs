namespace VideoReg.Domain.OnlineVideo.Store
{
    public class CameraImage
    {
        public CameraImage(ImageTransformSettings settings, byte[] img, byte[] sourceImg)
        {
            this.settings = settings;
            this.img = img;
            this.sourceImg = sourceImg;
        }

        public readonly ImageTransformSettings settings;
        public readonly byte[] img;
        public readonly byte[] sourceImg;
    }
}