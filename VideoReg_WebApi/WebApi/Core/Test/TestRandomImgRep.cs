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
        [Obsolete]
        readonly IHostingEnvironment env;
        readonly Random rnd = new Random(Environment.TickCount);

        [Obsolete]
        public TestRandomImgRep(IHostingEnvironment env)
        {
            this.env = env ?? throw new ArgumentNullException(nameof(env));
        }

        /// <summary>
        /// GetImg
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException">Ignore.</exception>
        /// <exception cref="DirectoryNotFoundException">Ignore.</exception>
        /// <exception cref="PathTooLongException">Ignore.</exception>
        /// <exception cref="IOException">Ignore.</exception>
        /// <exception cref="System.Security.SecurityException">Ignore.</exception>
        [Obsolete]
        public byte[] GetImg(Uri url, int timeoutMs)
        {
            string[] images = Directory.GetFiles(env.ContentRootPath, "*.jpg", SearchOption.AllDirectories);
            int imgIndex = rnd.Next(0, images.Length - 1);
            string fileName = images[imgIndex];
            return File.ReadAllBytes(fileName);
        }

        /// <summary>
        /// GetImgAsync
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeoutMs"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException">Ignore.</exception>
        /// <exception cref="DirectoryNotFoundException">Ignore.</exception>
        /// <exception cref="PathTooLongException">Ignore.</exception>
        /// <exception cref="IOException">Ignore.</exception>
        [Obsolete]
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
