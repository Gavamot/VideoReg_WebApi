using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Formatting.Display;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.OnlineVideo.SignalR;
using VideoReg.Domain.VideoRegInfo;

namespace VideoReg.WebApi.Core
{
    /// <summary>
    /// Hub is not thread-safe ( Use only in 1 thread this instance)
    /// </summary>
    public class ClientVideoHub : IClientVideoHub
    {
        private const int ConnectRetryTimeoutMs = 500;
        private Uri GenerateServerUrl(string endpoint) => new Uri($"{endpoint}/onlineVideoHub");
        private HubConnection connection;
        private ILogger<ClientVideoHub> log;
        private CancellationToken token;
        private volatile Uri serverUrl;
        public Action<int[]> OnInitShow { get; set; }
        public Action<int> OnStopShow { get; set; }
        public Action<int> OnStartShow { get; set; }
        public Action<int, ImageSettings> OnSetCameraSettings { get; set; }
        private IVideoTransmitterConfig config;

        public ClientVideoHub(ILogger<ClientVideoHub> log, IVideoTransmitterConfig config)
        {
            this.log = log;
            this.config = config;
            this.serverUrl = GenerateServerUrl(config.AscRegServiceEndpoint);
            this.connection = ConfigureConnection(serverUrl, token);
        }

     

        //const string secretJwt = "133D41F6DA1D79B8A432423432432432432432432432546567657567431FFAF55C21E9B1521AE4021";
        //private const string tokenJwt =
        //    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ2LTEzMzYiLCJuYW1lIjoib3JlaG92IiwiaWF0IjoxNTE2MjM5MDIyfQ.jGc9yRqSjqfu5wMwemuQadz06NF2TGt3B3oPJnASBUI";

        //private Task<string> GetToken()
        //{
        //    return Task.FromResult(tokenJwt);
        //}

        private HubConnection ConfigureConnection(Uri serverUrl, CancellationToken token)
        {
            this.token = token;
            //var reconnects = GenerateReconnects().ToArray();
            var cert = new X509Certificate2(Path.Combine("public.crt"), "v1336pwd");
            var _clientHandler = new HttpClientHandler();
            _clientHandler.ClientCertificates.Add(cert);

            this.connection = new HubConnectionBuilder()
                .WithUrl(serverUrl, HttpTransportType.WebSockets, options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                    options.DefaultTransferFormat = TransferFormat.Binary;
                    options.UseDefaultCredentials = true;
                    options.ClientCertificates.Add(cert);
                    options.SkipNegotiation = true;
                    options.HttpMessageHandlerFactory = handler =>
                    {
                        var _clientHandler = new HttpClientHandler();
                        _clientHandler.CheckCertificateRevocationList = false;
                        _clientHandler.PreAuthenticate = false;
                        _clientHandler.ClientCertificates.Add(cert);
                        return _clientHandler;
                    };
                    
                    options.WebSocketConfiguration = sockets =>
                    {
                        sockets.RemoteCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
                        sockets.ClientCertificates.Add(cert);
                    };
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


            connection.On<int[]>("SendInitShow", cameras => OnInitShow(cameras));
            connection.On<int>("SendStopShow", camera => OnStopShow(camera));
            connection.On<int>("SendStartShow", camera => OnStartShow(camera));
            connection.On<int, ImageSettings>("SendCameraSettings", (camera, settings) => OnSetCameraSettings(camera, settings));

            return connection;
        }

        public async Task ConnectWithRetryAsync()
        {
            // TODO : обработать ошибку при перезапуске сервера
            while (connection.State != HubConnectionState.Connected)
            {
                log.LogInformation("[hub] ....... {serverUrl}", serverUrl);
                try
                {
                    await connection.StartAsync(token);
                    log.LogInformation("[hub] -------- {serverUrl}", serverUrl);
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
