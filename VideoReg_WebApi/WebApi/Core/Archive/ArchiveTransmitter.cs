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

        private MultipartFormDataContent CreateFileContent(ArchiveFileData file)
        {
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(file.Data), "file", file.FileName);
            return content;
        }

        public const string EmptyFileName = "e";
        private MultipartFormDataContent CreateEmptyFileContent()
        {
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(new byte[0]), "file", EmptyFileName);
            return content;
        }

        private async Task UploadCameraFileAsync(ArchiveFileData file, DateTime end, UrlParameterBuilder urlBuilder)
        {
            urlBuilder.AddParameter("vpn", regInfoRep.Vpn);
            if (IsEndOfTask(file, end))
            {
                // В арахиве нет файла временная метка которого >= pdt. (При автозакачке это будет означать что задача закончила свое исполнение)
                urlBuilder.AddParameter("brigade", -1);
                var url = urlBuilder.Build();
                var content = CreateEmptyFileContent();
                await UploadFileAsync(content, url);
            }
            else
            {
                urlBuilder.AddParameter("brigade", file.File.brigade);
                var url = urlBuilder.Build();
                urlBuilder.AddParameter("file", file.File.FileName);
                var urlCheck = urlBuilder.Build();
                if (await IsFileExistAsync(urlCheck))
                {
                    return;
                }
                var content = CreateFileContent(file);
                await UploadFileAsync(content, url);
            }
        }

        public async Task UploadCameraFileAsync(DateTime pdt, DateTime end, int camera)
        {
            var file = await cameraArchiveRep.GetNearestFrontVideoFileAsync(pdt, camera);
            var urlBuilder = new UrlParameterBuilder(config.SetCameraArchiveUrl);
            urlBuilder.AddParameter("camera", camera);
            await UploadCameraFileAsync(file, end, urlBuilder);
        }

        public async Task UploadTrendsFileAsync(DateTime pdt, DateTime end)
        {
            var file = await trendsArchiveRep.GetNearestFrontTrendFileAsync(pdt);
            var urlBuilder = new UrlParameterBuilder(config.SetTrendsArchiveUrl);
            await UploadCameraFileAsync(file, end, urlBuilder);
        }

        private bool IsEndOfTask(ArchiveFileData file, DateTime end)
        {
            return file == null || file.File.pdt > end;
        }

        private async Task<bool> ExecureWebReqvest(MultipartFormDataContent content, string url, HttpMethod method, HttpStatusCode correctCode)
        {
            var client = httpClientFactory.CreateClient(Global.AscWebClient);
            using var message = new HttpRequestMessage(method, url);
            if(content != null)
            {
                message.Content = content;
            }
            using var response = await client.SendAsync(message);
            content?.Dispose();
            return response.StatusCode == correctCode;
        }

        private async Task<bool> IsFileExistAsync(string url) 
            => await ExecureWebReqvest(null, url, HttpMethod.Head, HttpStatusCode.Found);

        private async Task<bool> UploadFileAsync(MultipartFormDataContent content, string url)
            => await ExecureWebReqvest(content, url, HttpMethod.Post, HttpStatusCode.OK);

        //private MultipartFormDataContent CreateDataBase(ArchiveFileData file)
        //{
        //    int brigadeCode = file.File.brigade;
        //    var content = new MultipartFormDataContent();
        //    content.Add(new StringContent(regInfoRep.Vpn), "vpn");
        //    content.Add(new StringContent(brigadeCode.ToString()), "brigade");
        //    return content;
        //}

        //private MultipartFormDataContent CreateDataTrendsCheck(ArchiveFileData file)
        //{
        //    var content = CreateDataBase(file);
        //    string fileName = file.File.fullArchiveName;
        //    content.Add(new StringContent(fileName), "fileName");
        //    return content;
        //}

        //private MultipartFormDataContent CreateDataCameraCheck(ArchiveFileData file, int camera)
        //{
        //    var content = CreateDataTrendsCheck(file);
        //    content.Add(new StringContent(camera.ToString()), "camera");
        //    return content;
        //}

        //private MultipartFormDataContent CreateFormDataTrendsUpload(ArchiveFileData file)
        //{
        //    var content = CreateDataBase(file);
        //    content.Add(new ByteArrayContent(file.Data), "file", file.File.fullArchiveName);
        //    return content;
        //}

        //private MultipartFormDataContent CreateFormDataCameraUpload(ArchiveFileData file, int camera)
        //{
        //    var content = CreateFormDataTrendsUpload(file);
        //    content.Add(new StringContent(camera.ToString()), "camera");
        //    return content;
        //}
    }
}
