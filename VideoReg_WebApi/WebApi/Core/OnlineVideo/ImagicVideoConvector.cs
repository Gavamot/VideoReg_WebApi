using System;
using System.IO;
using ImageMagick;
using Microsoft.Extensions.Logging;
using WebApi.Contract;
using WebApi.OnlineVideo;


namespace WebApi.Core
{
    public class ImagicVideoConvector : IVideoConvector
    {
        readonly ILogger<ImagicVideoConvector> log;
        public ImagicVideoConvector(ILogger<ImagicVideoConvector> log)
        {
            this.log = log;
        }

        public byte[] ConvertVideo(byte[] img, ImageSettings settings)
        {
            try
            {
                using var image = new MagickImage(img, MagickFormat.Jpg);
                image.Resize(settings.Width, settings.Height);
                image.Quality = settings.Quality;
                using var stream = new MemoryStream();
                image.Write(stream);
                var res = stream.ToArray();
                return res;
            }
            catch(Exception e)
            {
                log.LogError($"Error then convert image err={e.Message}");
                return img;
            }
        }
    }
}
