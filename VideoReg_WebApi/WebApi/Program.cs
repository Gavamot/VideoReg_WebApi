using System;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.AspNetCore;
using WebApi.Archive;
using WebApi.OnlineVideo;
using WebApi.OnlineVideo.SignalR;
using WebApi.Trends;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<CameraHostedService>();
                    services.AddHostedService<VideoTransmitterHostedService>();

                    services.AddHostedService<VideoArchiveUpdaterHostedService>();
                    services.AddHostedService<TrendsTransmitterHostedService>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
