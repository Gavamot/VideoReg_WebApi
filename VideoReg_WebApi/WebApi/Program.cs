using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
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
