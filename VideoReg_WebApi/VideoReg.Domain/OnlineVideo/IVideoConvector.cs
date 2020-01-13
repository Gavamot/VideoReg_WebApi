namespace VideoReg.Domain.OnlineVideo
{
    public interface IVideoConvector
    {
        byte[] ConvertVideo(byte[] img, ImageTransformSettings settings);
    }
}