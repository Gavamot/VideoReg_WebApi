using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebApi.Configuration;
using WebApi.Contract;
using WebApi.Services;
using WebApi.Core;
using WebApi.OnlineVideo.OnlineVideo;

namespace WebApi.Trends
{
    public class TrendsTransmitterHostedService : ServiceUpdater, IHostedService
    {
        class State
        {
            public DateTime Timestamp { get; set; }
            public RegInfo RegInfo { get; set; }
        }

        readonly ITrendsRep trends;
        readonly ITrendsConfig config;
        readonly IHttpClientFactory httpClientFactory;
        readonly IDateTimeService dateTimeService;
        readonly IRegInfoRep regInfoRep;
        private readonly IClientAscHub hub;
        public override object Context { get; protected set; } = new State();
        public override string Name => "TrendsTransmitter";

        public TrendsTransmitterHostedService(ITrendsRep trends, 
            ITrendsConfig config,
            IHttpClientFactory httpClientFactory, 
            ILog log,
            IDateTimeService dateTimeService,
            IRegInfoRep regInfoRep, 
            IClientAscHub hub) : base(config.TrendsIterationMs, log)
        {
            this.trends = trends;
            this.config = config;
            this.httpClientFactory = httpClientFactory;
            this.dateTimeService = dateTimeService;
            this.regInfoRep = regInfoRep;
            this.hub = hub;

        }

        public override async Task DoWorkAsync(object context, CancellationToken cancellationToken)
        {
            var c = (State) context;
            var res = await trends.GetTrendsIfChangedAsync(c.Timestamp);
            if (res != null)
            {
                c.Timestamp = res.Timestamp;
                await PassTrends(c.RegInfo.Vpn, res.Value);
            }
        }

        private async Task PassTrends(string vpn, string trends)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(vpn), "vpn");
            content.Add(new StringContent(trends), "trendsJson");
            var ascRegService = httpClientFactory.CreateClient(Global.AscWebClient);
            using var response = await ascRegService.PostAsync(config.TrendsSetUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                log.Error($"{config.TrendsSetUrl} - return BadStatusCode");
            }
        }

        public new Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override async Task<bool> BeforeStart(object context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(config.TrendsSetUrl))
            {
                log.Warning("In the file [appsettings.json] parameter [Settings.TrendsAscWebSetUrl] - is empty. Trends will not pass to asc_reg_service.");
                return false;
            }

            hub.OnStartTrends = this.Continue;
            hub.OnStopTrends = this.Pause;

            var c = (State)context;
            c.Timestamp = dateTimeService.GetNow();
            c.RegInfo = await Must.Do(async () => await regInfoRep.GetInfoAsync(), updateTimeMs, cancellationToken, exception =>
            {
                log.Error($"TrendsTransmitterHostedService can not get RegInfo ({exception.Message})", exception);
            });
            return true;
        }

    }
}
