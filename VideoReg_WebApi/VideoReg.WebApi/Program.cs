using System;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.AspNetCore;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.OnlineVideo.SignalR;
using VideoRegService.Core.Archive;

namespace VideoReg.WebApi
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
                    services.AddHostedService<CameraUpdateService>();
                    services.AddHostedService<VideoArchiveUpdateService>();
                    services.AddHostedService<VideoTransmitterService>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
