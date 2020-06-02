using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Net;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 10;

            var configuration =  new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            try
            {
                Log.Information("Application is stating...");
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception e)
            {
                Log.Fatal(e, $"Application failed then started ({e.Message})");
            }
            finally
            {
                Log.CloseAndFlush();
            }
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
            })
            .UseSerilog();
    }
}
