using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApi.Configuration;
using WebApi.Contract;
using WebApi.OnlineVideo.OnlineVideo;

namespace WebApi.Core
{
    /// <summary>
    /// Hub is not thread-safe ( Use only in 1 thread this instance)
    /// </summary>
    public class ClientAscHub : IClientAscHub
    {
        private const int ConnectRetryTimeoutMs = 500;
        private HubConnection connection;
        private ILogger<ClientAscHub> log;
        private CancellationToken token;
        private volatile Uri serverUrl;
        private IVideoTransmitterConfig config;

        public Action<CameraSettings[]> OnInitShow { get; set; }
        public Action<int> OnStopShow { get; set; }
        public Action<int> OnStartShow { get; set; }
        public Action<CameraSettings> OnSetCameraSettings { get; set; }
        
        
        public Action OnStartTrends { get; set; }
        public Action OnStopTrends { get; set; }

        public Action OnTrendsArchiveTask { get; set; }
        public Action OnCameraArchiveTask { get; set; }

        public ClientAscHub(ILogger<ClientAscHub> log, IVideoTransmitterConfig config)
        {
            this.log = log;
            this.config = config;
            if (!string.IsNullOrEmpty(config.AscRegServiceEndpoint))
            {
                this.serverUrl = new Uri(config.AscRegServiceEndpoint);
                this.connection = ConfigureConnection(serverUrl, token);
            }
        }

        private HubConnection ConfigureConnection(Uri serverUrl, CancellationToken token)
        {
            this.token = token;
            //var reconnects = GenerateReconnects().ToArray();
            //var cert = new X509Certificate2(Path.Combine("public.crt"), "v1336pwd");
            //var _clientHandler = new HttpClientHandler();
            //_clientHandler.ClientCertificates.Add(cert);

            this.connection = new HubConnectionBuilder()
                .WithUrl(serverUrl, HttpTransportType.WebSockets, options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                    options.DefaultTransferFormat = TransferFormat.Binary;
                    //options.UseDefaultCredentials = true;
                    //options.ClientCertificates.Add(cert);
                    //options.SkipNegotiation = true;
                    //options.HttpMessageHandlerFactory = handler =>
                    //{
                    //    var _clientHandler = new HttpClientHandler();
                    //    _clientHandler.CheckCertificateRevocationList = false;
                    //    _clientHandler.PreAuthenticate = false;
                    //    _clientHandler.ClientCertificates.Add(cert);
                    //    return _clientHandler;
                    //};
                    
                    //options.WebSocketConfiguration = sockets =>
                    //{
                    //    sockets.RemoteCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
                    //    sockets.ClientCertificates.Add(cert);
                    //};
                })
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


            connection.On<CameraSettings[]>("SendInitShow", cameras =>
            {
                OnInitShow?.Invoke(cameras);
            });
               
            connection.On<int>("SendStopShow", camera =>
            {
                OnStopShow?.Invoke(camera);
            });

            connection.On<int>("SendStartShow", camera =>
            {
                OnStartShow?.Invoke(camera);
            });
                
            connection.On<CameraSettings>("SendCameraSettings", settings =>
            {
                OnSetCameraSettings?.Invoke(settings);
            });

            connection.On("SendStartTrends", () => { OnStartTrends?.Invoke(); });

            connection.On("SendStopTrends", () =>
            {
                OnStopTrends?.Invoke();
            });

            connection.On("SendTrendsArchiveTask", () =>
            {
                OnTrendsArchiveTask?.Invoke();
            });

            connection.On("SendCameraArchiveTask", () =>
            {
                OnCameraArchiveTask?.Invoke();
            });

            return connection;
        }

        public async Task Close()
        {
            await connection.StopAsync(CancellationToken.None);
            await connection.DisposeAsync();
        }

        public async Task ConnectWithRetryAsync()
        {
            while (connection.State != HubConnectionState.Connected)
            {
                try
                {
                    if (connection.State == HubConnectionState.Disconnected)
                    {
                        await connection.StartAsync(token);
                        log.LogInformation("[hub] -------- {serverUrl}", serverUrl);
                        return;
                    }
                    await Task.Delay(ConnectRetryTimeoutMs, token);
                }
                catch when (token.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception e)
                {
                    await Task.Delay(ConnectRetryTimeoutMs, token);
                    log.LogError(e, "hub xxxxxxxx {serverUrl}", serverUrl);
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
