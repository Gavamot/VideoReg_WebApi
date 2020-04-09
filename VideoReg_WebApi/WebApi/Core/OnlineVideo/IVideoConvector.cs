using WebApi.Contract;

namespace WebApi.OnlineVideo
{
    public interface IVideoConvector
    {
        byte[] ConvertVideo(byte[] img, ImageSettings settings);
    }
}