using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using Serilog;
using WebApi.Archive;
using WebApi.OnlineVideo;
using WebApi.OnlineVideo.OnlineVideo;
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
                 //.UseSerilog()
                 .ConfigureLogging((hostingContext, logging) =>
                 {
                     string v = hostingContext.Configuration["DisableLog"];
                     if (!Startup.IsOn(v))
                     {
                         logging.ClearProviders();
                     }
                 })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<CameraHostedService>();
                    services.AddHostedService<VideoTransmitterHostedService>();

                    services.AddHostedService<InitHostedService>();
                    services.AddHostedService<TrendsTransmitterHostedService>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
