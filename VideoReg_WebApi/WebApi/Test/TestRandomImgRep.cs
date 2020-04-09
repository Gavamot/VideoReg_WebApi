using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebApi.OnlineVideo;

namespace WebApiTest
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

        public Task<byte[]> GetImgAsync(Uri url, int timeoutMs, CancellationToken token = default)
        {
            var images = Directory.GetFiles(env.ContentRootPath, "*.jpg", SearchOption.AllDirectories);
            int imgIndex = rnd.Next(0, images.Length - 1);
            string fileName = images[imgIndex];
            return File.ReadAllBytesAsync(fileName, token);
        }

        public Task<byte[]> GetImgAsync(Uri url, int timeoutMs)
        {
            throw new NotImplementedException();
        }
    }
}
