using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Archive;
using WebApi.Archive.ArchiveFiles;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;
using WebApi.ValueType;

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

        private ArchiveFileData EmptyFile => new ArchiveFileData
        {
            Data = new byte[0],
            File = new FileTrendsJson(0, DateTime.MaxValue, new DeviceSerialNumber(), "")
        };
     

        public async Task UploadCameraFileAsync(DateTime pdt, DateTime end, int camera)
        {
            string url = config.SetCameraArchiveUrl;
            var file = await cameraArchiveRep.GetNearestFrontVideoFileAsync(pdt, camera);
            if(IsEndOfTask(file, end))
            {
                // В арахиве нет файла временная метка которого >= pdt. (При автозакачке это будет означать что задача закончила свое исполнение)
                var content = CreateFormDataCameraUpload(EmptyFile, camera);
                await UploadFileAsync(content, url);
            }
            else
            {
                var checkContent = CreateDataCameraCheck(file, camera);
                if (await IsFileExistAsync(checkContent, url))
                {
                    return;
                }
                var uploadContent = CreateFormDataCameraUpload(file, camera);
                await UploadFileAsync(uploadContent, url);
            }
        }

        public async Task UploadTrendsFileAsync(DateTime pdt, DateTime end)
        {
            var file = await trendsArchiveRep.GetNearestFrontTrendFileAsync(pdt);
            MultipartFormDataContent uploadContent = IsEndOfTask(file, end) ?
                CreateFormDataTrendsUpload(EmptyFile) :
                 CreateFormDataTrendsUpload(file);
            await UploadFileAsync(uploadContent, config.SetTrendsArchiveUrl);
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
            => await ExecureWebReqvest(content, url, HttpMethod.Head, HttpStatusCode.Found);

        private async Task<bool> UploadFileAsync(MultipartFormDataContent content, string url)
            => await ExecureWebReqvest(content, url, HttpMethod.Post, HttpStatusCode.OK);

        private MultipartFormDataContent CreateDataBase(ArchiveFileData file)
        {
            int brigadeCode = file.File.brigade;
            DateTime pdt = file.File.pdt;
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(regInfoRep.Vpn), "vpn");
            content.Add(new StringContent(brigadeCode.ToString()), "brigadeCode");
            content.Add(new StringContent(dateTimeService.ToStringFull(pdt)), "pdt");
            return content;
        }

        private MultipartFormDataContent CreateDataTrendsCheck(ArchiveFileData file)
        {
            var content = CreateDataBase(file);
            string fileName = file.File.fullArchiveName;
            content.Add(new StringContent(fileName), "fileName");
            return content;
        }

        private MultipartFormDataContent CreateDataCameraCheck(ArchiveFileData file, int camera)
        {
            var content = CreateDataTrendsCheck(file);
            content.Add(new StringContent(camera.ToString()), "camera");
            return content;
        }

        private MultipartFormDataContent CreateFormDataTrendsUpload(ArchiveFileData file)
        {
            var content = CreateDataBase(file);
            content.Add(new ByteArrayContent(file.Data), "file", file.File.fullArchiveName);
            return content;
        }

        private MultipartFormDataContent CreateFormDataCameraUpload(ArchiveFileData file, int camera)
        {
            var content = CreateFormDataTrendsUpload(file);
            content.Add(new StringContent(camera.ToString()), "camera");
            return content;
        }
    }
}
