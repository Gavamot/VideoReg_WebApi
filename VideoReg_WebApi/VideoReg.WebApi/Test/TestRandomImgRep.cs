using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using VideoReg.Domain.OnlineVideo;

namespace VideoReg.WebApi.Test
{
    public class TestRandomImgRep : IImgRep
    {
        readonly IHostingEnvironment env;
        readonly Random rnd = new Random(Environment.TickCount);
        public TestRandomImgRep(IHostingEnvironment env)
        {
            this.env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public byte[] GetImg(Uri url, int timeoutMs)
        {
            var images = Directory.GetFiles(env.ContentRootPath, "*.jpg", SearchOption.AllDirectories);
            int imgIndex = rnd.Next(0, images.Length - 1);
            string fileName = images[imgIndex];
            return File.ReadAllBytes(fileName);
        }

        public Task<byte[]> GetImgAsync(string url, int timeoutMs)
        {
            var images = Directory.GetFiles(env.ContentRootPath, "*.jpg", SearchOption.AllDirectories);
            int imgIndex = rnd.Next(0, images.Length - 1);
            string fileName = images[imgIndex];
            return File.ReadAllBytesAsync(fileName);
        }

    }
}
