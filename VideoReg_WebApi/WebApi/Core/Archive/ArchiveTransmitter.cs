using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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
        private readonly ICameraSettingsStore settingsStore;
        private readonly IRegInfoRep regInfoRep;
        private readonly IArchiveConfig config;
        private readonly IHttpClientFactory httpClientFactory;

        public ArchiveTransmitter(
            ILog log,
            IArchiveConfig config,
            IRegInfoRep regInfoRep,
            IClientAscHub hub,
            ICameraStore cameraStore,
            ICameraSettingsStore settingsStore,
            IHttpClientFactory httpClientFactory)
        {
            this.config = config;
            this.cameraStore = cameraStore;
            this.settingsStore = settingsStore;
            this.regInfoRep = regInfoRep;
            this.hub = hub;
            this.log = log;
            this.httpClientFactory = httpClientFactory;
        }

        public Task UploadCameraFile(DateTime pdt)
        {
            throw new NotImplementedException();
        }

        public Task UploadTrendsFile(DateTime pdt)
        {

            throw new NotImplementedException();
        }

        private async Task UploadFile(int camera, string vpn, int brigadeCode, string fileName, byte[] file)
        {
            
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(vpn), "vpn");
            content.Add(new StringContent(brigadeCode.ToString()), "brigadeCode");
            content.Add(new StringContent(fileName), "fileName");
            content.Add(new ByteArrayContent(file), "file");
            content.Add(new StringContent(camera.ToString()), "camera");
            if (camera == 0)
            {

            } 
            var client = httpClientFactory.CreateClient(Global.AscWebClient);

        }
    }
}
