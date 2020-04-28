using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Archive;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;

namespace WebApi.Core.SignalR
{
    public interface IArchiveTransmitter
    {
        public Task UploadCameraFileAsync(DateTime pdt, DateTime end, int camera);
        public Task UploadTrendsFileAsync(DateTime pdt, DateTime end);
    }

    public class ArchiveTransmitter : IArchiveTransmitter
    {
        private readonly ILog log;
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
            IDateTimeService dateTimeService,
            ICameraArchiveRep cameraArchiveRep,
            ITrendsArchiveRep trendsArchiveRep,
            IHttpClientFactory httpClientFactory)
        {
            this.log = log;
            this.config = config;
            this.regInfoRep = regInfoRep;
            this.cameraArchiveRep = cameraArchiveRep;
            this.trendsArchiveRep = trendsArchiveRep;
            this.httpClientFactory = httpClientFactory;
            this.dateTimeService = dateTimeService;
        }

        public async Task UploadCameraFileAsync(DateTime pdt, DateTime end, int camera)
        {
            string url = config.SetCameraArchiveUrl;
            var file = await cameraArchiveRep.GetNearestFrontTrendFileAsync(pdt, camera);
            if(IsEndOfTask(file, end))
            {
                // В арахиве нет файла временная метка которого >= pdt. (При автозакачке это будет означать что задача закончила свое исполнение)
                var uploadContent = CreateBaseFormData(0, string.Empty, DateTime.MaxValue, new byte[0], 0);
                await UploadFileAsync(uploadContent, url);
            }
            else
            {
                int brigadeCode = file.File.brigade;
                string fileName = file.File.fullArchiveName;
                DateTime timestamp = file.File.pdt;
                var checkContent = CreateBaseFormData(brigadeCode, fileName, timestamp, camera);
                if (await IsFileExistAsync(checkContent, url))
                {
                    return;
                }
                var uploadContent = CreateBaseFormData(brigadeCode, fileName, timestamp, file.Data, camera);
                await UploadFileAsync(uploadContent, url);
            }
        }

        public async Task UploadTrendsFileAsync(DateTime pdt, DateTime end)
        {
            string url = config.SetTrendsArchiveUrl;
            var file = await trendsArchiveRep.GetNearestFrontTrendFileAsync(pdt);
            if (IsEndOfTask(file, end))
            {
                // В арахиве нет файла временная метка которого >= pdt. (При автозакачке это будет означать что задача закончила свое исполнение)
                var uploadContent = CreateBaseFormData(0, string.Empty, DateTime.MaxValue, new byte[0]);
                await UploadFileAsync(uploadContent, url);
            }
            else
            {
                int brigadeCode = file.File.brigade;
                string fileName = file.File.fullArchiveName;
                DateTime timestamp = file.File.pdt;
                var uploadContent = CreateBaseFormData(brigadeCode, fileName, timestamp, file.Data);
                await UploadFileAsync(uploadContent, url);
            }
        }

        private bool IsEndOfTask(ArchiveFileData file, DateTime end)
        {
            return file == null || file.File.pdt > end;
        }

        private async Task<bool> ExecureWebReqvest(MultipartFormDataContent content, string url, HttpMethod method, HttpStatusCode correctCode)
        {
            var client = httpClientFactory.CreateClient(Global.AscWebClient);
            using var message = new HttpRequestMessage(method, url);
            message.Content = content;
            using var response = await client.SendAsync(message);
            content.Dispose();
            return response.StatusCode == correctCode;
        }

        private async Task<bool> IsFileExistAsync(MultipartFormDataContent content, string url) 
            => await ExecureWebReqvest(content, url, HttpMethod.Head, HttpStatusCode.NoContent);

        private async Task<bool> UploadFileAsync(MultipartFormDataContent content, string url)
            => await ExecureWebReqvest(content, url, HttpMethod.Post, HttpStatusCode.OK);
        
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
            var content = CreateBaseFormData(brigadeCode, fileName, pdt);
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
