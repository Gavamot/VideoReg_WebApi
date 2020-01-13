using System;
using System.IO;
using ImageMagick;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Infra.Services;


namespace VideoReg.WebApi.Core
{
    public class ImagicVideoConvector : IVideoConvector
    {
        readonly ILog log;
        public ImagicVideoConvector(ILog log)
        {
            this.log = log;
        }

        public byte[] ConvertVideo(byte[] img, ImageTransformSettings settings)
        {
            try
            {
                using var image = new MagickImage(img);
                image.Resize(settings.width, settings.height);
                image.Quality = settings.quality;
                using var stream = new MemoryStream();
                image.Write(stream);
                var res = stream.ToArray();
                return res;
            }
            catch(Exception e)
            {
                log.Error("Error then convert image", e);
                return img;
            }
        }
    }
}
