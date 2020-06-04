using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;

namespace WebApi.Core
{
    public class AscHttpClient
    {
        private readonly HttpClient http;
        private readonly IConfig config;
        private readonly ILog log;

        public AscHttpClient(HttpClient http, IConfig config, ILog log)
        {
            this.http = http;
            this.config = config;
            this.log = log;
        }

        public async Task<bool> SendOnlineTrendsAsync(string vpn, string trends)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(vpn), "vpn");
                content.Add(new StringContent(trends), "trendsJson");
                using var response = await http.PostAsync(config.SetTrendsUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    log.Error($"{config.SetTrendsUrl} - return BadStatusCode");
                }
                return response.IsSuccessStatusCode;
            }
            catch(Exception e)
            {
               log.Error($"{config.SetTrendsUrl} - error ({e.Message})");
               return false;
            }
        }

        public async Task<bool> SendCameraImagesHttpAsync(string vpn, int cameraNumber, byte[] img, int convertMs)
        {
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(img), "file", "c");
            var urlBuilder = new UrlParameterBuilder(config.SetImageUrl);
            urlBuilder.AddParameter("vpn", vpn);
            urlBuilder.AddParameter("camera", cameraNumber);
            urlBuilder.AddParameter("convertMs", convertMs);
            var paramsUrl = urlBuilder.Build();
            try
            {
                var responce = await http.PostAsync(paramsUrl, content);
                if (!responce.IsSuccessStatusCode)
                {
                    log.Error($"Can not pass image to server cam={cameraNumber}");
                }
                return responce.IsSuccessStatusCode;
            }
            catch(Exception e)
            {
                log.Error($"{config.SetImageUrl} error - {e.Message}");
                return false;
            }
        }

        #region Archives

        public async Task UploadCameraFileAsync(string vpn, ArchiveFileData file, DateTime end, int camera)
        {
            var urlBuilder = new UrlParameterBuilder(config.SetCameraArchiveUrl);
            urlBuilder.AddParameter("camera", camera);
            await UploadFileAsync(vpn, file, end, urlBuilder);
        }

        public async Task UploadTrendsFileAsync(string vpn, ArchiveFileData file, DateTime end)
        {
            var urlBuilder = new UrlParameterBuilder(config.SetTrendsArchiveUrl);
            await UploadFileAsync(vpn, file, end, urlBuilder);
        }

        private MultipartFormDataContent CreateFileContent(ArchiveFileData file)
        {
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(file.Data), "file", file.FileName);
            return content;
        }

        private MultipartFormDataContent CreateEmptyFileContent()
        {
            var content = new MultipartFormDataContent();
            var empty = ArchiveFileData.EmptyFile;
            content.Add(new ByteArrayContent(empty.Data), "file", empty.FileName);
            return content;
        }

        private async Task UploadFileAsync(string vpn, ArchiveFileData file, DateTime end, UrlParameterBuilder urlBuilder)
        {
            urlBuilder.AddParameter("vpn", vpn);
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

        private bool IsEndOfTask(ArchiveFileData file, DateTime end)
        {
            return file == null || file.File.pdt > end;
        }

        private async Task<bool> ExecureWebReqvest(MultipartFormDataContent content, string url, HttpMethod method, HttpStatusCode correctCode)
        {
            using var message = new HttpRequestMessage(method, url);
            if (content != null)
            {
                message.Content = content;
            }
            try
            {
                using var response = await http.SendAsync(message);
                return response.StatusCode == correctCode;
            }
            catch(Exception e)
            {
                log.Error($"[{url}] error - {e.Message}");
                return false;
            }
            finally
            {
                content?.Dispose();
            }
        }

        private async Task<bool> IsFileExistAsync(string url)
            => await ExecureWebReqvest(null, url, HttpMethod.Head, HttpStatusCode.Found);

        private async Task<bool> UploadFileAsync(MultipartFormDataContent content, string url)
            => await ExecureWebReqvest(content, url, HttpMethod.Post, HttpStatusCode.OK);

        #endregion


    }
}
