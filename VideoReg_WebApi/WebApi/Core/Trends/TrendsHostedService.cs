using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Hosting;
using WebApi.Configuration;
using WebApi.Contract;
using WebApi.Services;
using WebApi.Core;

namespace WebApi.Trends
{
    public class TrendsHostedService : IHostedService
    {
        readonly ITrendsRep trends;
        readonly ITrendsConfig config;
        readonly HttpClient ascRegService;
        readonly ILog log;
        readonly IDateTimeService dateTimeService;
        readonly IRegInfoRep regInfoRep;
        public TrendsHostedService(ITrendsRep trends, 
            ITrendsConfig config,
            IHttpClientFactory httpClientFactory, ILog log,
            IDateTimeService dateTimeService,
            IRegInfoRep regInfoRep)
        {
            this.trends = trends;
            this.config = config;
            this.ascRegService = httpClientFactory.CreateClient("ascRegService");
            this.log = log;
            this.dateTimeService = dateTimeService;
            this.regInfoRep = regInfoRep;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(config.TrendsAscWebSet))
            {
                log.Warning("In the file [appsettings.json] parameter [Settings.TrendsAscWebSet] - is empty. Trends will not pass to asc_reg_service.");
                return Task.CompletedTask;
            }
            log.Info($"Starting to pass trends to {config.TrendsAscWebSet}");
            TrendsLoop(cancellationToken);
            return Task.CompletedTask;
        }

        public async Task TrendsLoop(CancellationToken cancellationToken)
        {
            DateTime timestamp = dateTimeService.GetNow();

            RegInfo regInfo = await Must.Do(async ()=> await regInfoRep.GetInfoAsync(), config.TrendsIterationMs, cancellationToken, exception =>
            {
                log.Error($"TrendsHostedService can not get RegInfo ({exception.Message})", exception);
            });

            while (true)
            {
                try
                {
                    var res = await trends.GetTrendsIfChangedAsync(timestamp);
                    if(res != null)
                    {
                        timestamp = res.Timestamp;
                        await PassTrends(regInfo.Vpn, res.Value);
                    }
                }
                catch(IOException e)
                {
                    log.Error($"Can not get trends from repository ({e.Message})", e);
                }
                catch(HttpRequestException e)
                {
                    log.Error($"Can not pass data to asc reg service ({e.Message})", e);
                }
                catch(Exception e)
                {
                    log.Error(e.Message, e);
                }

                await Task.Delay(config.TrendsIterationMs, cancellationToken);
            }
        }


        private async Task PassTrends(string vpn, string trends)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(vpn), "vpn");
            content.Add(new StringContent(trends), "trendsJson");
            var responce = await ascRegService.PostAsync(config.TrendsAscWebSet, content);
            if (!responce.IsSuccessStatusCode)
            {
                log.Error($"{config.TrendsAscWebSet} - return BadStatusCode");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
