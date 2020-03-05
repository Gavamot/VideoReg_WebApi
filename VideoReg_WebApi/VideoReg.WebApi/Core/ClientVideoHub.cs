using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.OnlineVideo.SignalR;
using VideoReg.Domain.VideoRegInfo;
using VideoReg.WebApi.Core;

namespace VideoReg.WebApi.Core
{

    /// <summary>
    /// Hub is not thread-safe ( Use only in 1 thread this instance)
    /// </summary>
    public class ClientVideoHub : IClientVideoHub
    {
        private const int ConnectRetryTimeoutMs = 1000;
        private Uri GenerateServerUrl(string endpoint) => new Uri($"http://{endpoint}/onlineVideoHub");
        private HubConnection connection;
        private ILogger<ClientVideoHub> log;
        private CancellationToken token;
        private volatile Uri serverUrl;
        public Action<int[]> OnInitShow { get; set; }
        public Action<int> OnStopShow { get; set; }
        public Action<int> OnStartShow { get; set; }
        public Action<int, ImageTransformSettings> OnSetCameraSettings { get; set; }

        public ClientVideoHub(ILogger<ClientVideoHub> log)
        {
            this.log = log;
        }

        IEnumerable<TimeSpan> GenerateReconnects()
        {
            for (int i = 0; i < 3; i++)
            {
                yield return TimeSpan.Zero;
            }

            for (int i = 100; i < 50; i++)
            {
                yield return TimeSpan.FromSeconds(1);
            }

            for (int i = 0; i < 100; i++)
            {
                yield return TimeSpan.FromSeconds(3);
            }
        }

        private HubConnection ConfigureConnection(Uri serverUrl, CancellationToken token)
        {
            this.token = token;
            var reconnects = GenerateReconnects().ToArray();
            this.connection = new HubConnectionBuilder()
                .WithUrl(serverUrl, HttpTransportType.WebSockets)
                .WithAutomaticReconnect(reconnects)
                .AddMessagePackProtocol(options =>
                {
                    options.FormatterResolvers = new List<IFormatterResolver>()
                    {
                        MessagePack.Resolvers.DynamicEnumAsStringResolver.Instance,
                        MessagePack.Resolvers.StandardResolver.Instance,
                        MessagePack.Resolvers.DynamicContractlessObjectResolver.Instance
                    };
                })
                .Build();

            connection.On<int[]>("SendInitShow", cameras => OnInitShow(cameras));
            connection.On<int>("SendStopShow", camera => OnStopShow(camera));
            connection.On<int>("SendStartShow", camera => OnStartShow(camera));
            connection.On<int, ImageTransformSettings>("SendCameraSettings", (camera, settings) => OnSetCameraSettings(camera, settings));

            return connection;
        }

        public async Task ConnectAsync(string endpoint, CancellationToken token)
        {
            if (connection != null) throw new Exception("ClientVideoHub is already started");
            this.token = token;
            this.serverUrl = GenerateServerUrl(endpoint);
            this.connection = ConfigureConnection(serverUrl, token);
            await ConnectWithRetryAsync(serverUrl);
        }

        private async Task ConnectWithRetryAsync(Uri serverUrl)
        {
            while (connection.State != HubConnectionState.Connected)
            {
                log.LogInformation("[hub].......{serverUrl}", serverUrl);
                try
                {
                    await connection.StartAsync(token);
                    log.LogInformation("[hub]--------{serverUrl}", serverUrl);
                }
                catch when (token.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception e)
                {
                    log.LogError(e, "hub xxxxxxxx {serverUrl}", serverUrl);
                    try
                    {
                        await Task.Delay(ConnectRetryTimeoutMs, token);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        private async Task Send(string method, params object[] parameters)
        {
            await connection.SendCoreAsync(method, parameters, cancellationToken: token);
        }

        public async Task InitSessionAsync(RegInfo info) => 
            await Send("ReceiveInitSession", info);

        public async Task SendCameraImageAsync(int camera, byte[] image) =>
            await Send("ReceiveCameraImage", camera, image);

        public async Task SendNewRegInfoAsync(RegInfo info) =>
            await Send("ReceiveRegInfoChanged", info);
    }
}
