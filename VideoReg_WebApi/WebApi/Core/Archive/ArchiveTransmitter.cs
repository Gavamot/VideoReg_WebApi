using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebApi.Archive;
using WebApi.Archive.ArchiveFiles;
using WebApi.Configuration;
using WebApi.OnlineVideo.OnlineVideo;
using WebApi.OnlineVideo.Store;
using WebApi.Services;

namespace WebApi.Core.SignalR
{
    public interface IArchiveTransmitter
    {
        public Task UploadCameraFile(DateTime pdt);
        public Task UploadTrendsFile(DateTime pdt);
    }

    public class ArchiveTransmitter : IArchiveTransmitter
    {
        private readonly ILog log;
        private readonly IClientAscHub hub;
        private readonly ICameraStore cameraStore;
        private readonly IRegInfoRep regInfoRep;
        private readonly IArchiveConfig config;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ICameraArchiveRep cameraArchiveRep;
        readonly ITrendsArchiveRep trendsArchiveRep;
        string vpn = null;
        string Vpn
        {
            get
            {
                if (string.IsNullOrEmpty(vpn))
                {
                    vpn = regInfoRep.GetInfoAsync().Result.Vpn;
                }
                return vpn;
            }
        }

        public ArchiveTransmitter(
            ILog log,
            IArchiveConfig config,
            IRegInfoRep regInfoRep,
            IClientAscHub hub,
            ICameraArchiveRep cameraArchiveRep,
            ITrendsArchiveRep trendsArchiveRep,
            IHttpClientFactory httpClientFactory)
        {
            this.log = log;
            this.config = config;
            this.regInfoRep = regInfoRep;
            this.hub = hub;
            this.cameraArchiveRep = cameraArchiveRep;
            this.trendsArchiveRep = trendsArchiveRep;
            this.httpClientFactory = httpClientFactory;
        }

        public Task UploadCameraFile(DateTime pdt)
        {
            throw new NotImplementedException();
        }

        public Task UploadTrendsFile(DateTime pdt)
        {

            var checkContent = CreateBaseFormData(brigadeCode, fileName);
            var uploadContent = CreateBaseFormData(vpn, brigadeCode, fileName, file);

        }

        private ArchiveFile GetFileInfo(DateTime pdt)
        {

        }

        private MultipartFormDataContent CreateBaseFormData(int brigadeCode, string fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Vpn), "vpn");
            content.Add(new StringContent(brigadeCode.ToString()), "brigadeCode");
            content.Add(new StringContent(fileName), "fileName");
            return content;
        }

        private async Task CreateBaseFormData(int brigadeCode, string fileName, byte[] file)
        {
            var content = CreateBaseFormData(Vpn, brigadeCode, fileName);
            content.Add(new ByteArrayContent(file), "file");
        }

        private async Task<FtpStatusCode> Head()
        {
            
        }

        private async Task UploadFile()
        {

        }

        private async Task CheckFile(string vpn, int brigadeCode, string fileName)
        {
         
            var client = httpClientFactory.CreateClient(Global.AscWebClient);
            //204
        }


        

        private async Task UploadFile(string vpn, int brigadeCode, string fileName, byte[] file)
        {
           



            var client = httpClientFactory.CreateClient(Global.AscWebClient);
        }
    }
}
