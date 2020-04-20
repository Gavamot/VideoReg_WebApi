﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Archive;
using WebApi.Configuration;
using WebApi.OnlineVideo.OnlineVideo;
using WebApi.OnlineVideo.Store;
using WebApi.Services;

namespace WebApi.Core.SignalR
{
    public interface IArchiveTransmitter
    {
        public Task UploadCameraFileAsync(DateTime pdt, int camera);
        public Task UploadTrendsFileAsync(DateTime pdt);
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
        private readonly IDateTimeService dateTimeService;

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
            IDateTimeService dateTimeService,
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
            this.dateTimeService = dateTimeService;
        }

        public async Task UploadCameraFileAsync(DateTime pdt, int camera)
        {
            var file = await cameraArchiveRep.GetNearestFrontTrendFileAsync(pdt, camera);
            int brigadeCode = file.File.brigade;
            string fileName = file.File.fullArchiveName;
            var checkContent = CreateBaseFormData(brigadeCode, fileName, file.File.pdt, camera);
            string url = config.SetCameraArchiveUrl;
            if (await IsFileExistAsync(checkContent, url))
            {
                return;
            }
            var uploadContent = CreateBaseFormData(brigadeCode, fileName, file.File.pdt, file.Data, camera);
            await UploadFileAsync(uploadContent, url);
        }

        public async Task UploadTrendsFileAsync(DateTime pdt)
        {
            var file = await trendsArchiveRep.GetNearestFrontTrendFileAsync(pdt);
            int brigadeCode = file.File.brigade;
            string fileName = file.File.fullArchiveName;
            var checkContent = CreateBaseFormData(brigadeCode, fileName, file.File.pdt);
            string url = config.SetTrendsArchiveUrl;
            if (await IsFileExistAsync(checkContent, url))
            {
                return;
            }
            var uploadContent = CreateBaseFormData(brigadeCode, fileName, file.File.pdt, file.Data);
            await UploadFileAsync(uploadContent, url);
        }


        private async Task UploadFileAsync(string url, MultipartFormDataContent uploadContent)
        {
            var client = httpClientFactory.CreateClient(Global.AscWebClient);
            var res = await client.PostAsync(url, uploadContent);
        }

        private async Task<bool> IsFileExistAsync(MultipartFormDataContent content, string url)
        {
            var client = httpClientFactory.CreateClient(Global.AscWebClient);
            using var message=  new HttpRequestMessage(HttpMethod.Head, url);
            message.Content = content;
            using var response = await client.SendAsync(message);
            content.Dispose();
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        private async Task<bool> UploadFileAsync(MultipartFormDataContent content, string url)
        {
            var client = httpClientFactory.CreateClient(Global.AscWebClient);
            using var message = new HttpRequestMessage(HttpMethod.Head, url);
            message.Content = content;
            using var response = await client.SendAsync(message);
            content.Dispose();
            return response.StatusCode == HttpStatusCode.NoContent;
            //204
        }

        private MultipartFormDataContent CreateBaseFormData(int brigadeCode, string fileName, DateTime pdt)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Vpn), "vpn");
            content.Add(new StringContent(brigadeCode.ToString()), "brigadeCode");
            content.Add(new StringContent(fileName), "fileName");
            content.Add(new StringContent(dateTimeService.ToStringFull(pdt)), "pdt");
            return content;
        }

        private MultipartFormDataContent CreateBaseFormData(int brigadeCode, string fileName, DateTime pdt, int camera)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(camera.ToString()), "camera");
            return content;
        }


        private MultipartFormDataContent CreateBaseFormData(int brigadeCode, string fileName, DateTime pdt, byte[] file)
        {
            var content = CreateBaseFormData(brigadeCode, fileName, pdt);
            content.Add(new ByteArrayContent(file), "file");
            return content;
        }

        private MultipartFormDataContent CreateBaseFormData(int brigadeCode, string fileName, DateTime pdt, byte[] file, int camera)
        {
            var content = CreateBaseFormData(brigadeCode, fileName, pdt, file);
            content.Add(new StringContent(camera.ToString()), "camera");
            return content;
        }

    }
}
