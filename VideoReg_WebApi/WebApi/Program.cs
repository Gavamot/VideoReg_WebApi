using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
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
            ServicePointManager.DefaultConnectionLimit = 10;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureLogging((hostingContext, logging) =>
                 {
                     string v = hostingContext.Configuration["Log"];
                     if (!Startup.IsOn(v))
                     {
                         logging.ClearProviders();
                     }
                 })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
            .ConfigureWebHost((opt) =>
            {
                 opt.ConfigureKestrel((context, options) => {
                     options.AddServerHeader = false;
                 });
            });
    }
}
