using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApi.Core;
using WebApi.Configuration;
using WebApi.OnlineVideo.OnlineVideo;
using Microsoft.Extensions.Logging;
using WebApi.Core.SignalR;
using WebApi.Trends;
using WebApi.Ext;
using Serilog;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApi.Core.Configuration;

namespace WebApi
{
    public class Startup
    {
        //private readonly IConfiguration configuration;
        readonly Config config = new Config();
        readonly StartupConfig startupConfig = new StartupConfig();

        public Startup(IConfiguration configuration)
        {
            //this.configuration = configuration;
            configuration.GetSection("Settings").Bind(config);
            configuration.GetSection("Startup").Bind(startupConfig);

        }

        private void StartAppServices(IServiceCollection services)
        {
            services.TryAddSingleton<IRegInfoRep, RegInfoUpdatebleRep>();

            if (startupConfig.TransmitToAsc) 
            {
                // Тут я реализовал все очень коряво. 
                // Без видеопередачи все остальное не будет работать и неимеет смысла 
                // Поидее надо переделать и выделить отдельный уровень соединения 
                // Но раз так получилось тут можно забыть о модульности.
                // Просто включаем все сервисы если стоит передача на сервер
                services.AddOnlineVideo();
                services.TryAddSingleton<IClientAscHub, ClientAscHub>();
                services.AddAscHttpClient();

                services.AddHostedService<VideoTransmitterHostedService>();
                //services.AddHostedService<TrendsTransmitterHostedService>();
                services.AddSingleton<IArchiveTransmitter, ArchiveTransmitter>();

                services.AddOnlineVideo();
                //services.AddOnlineTrends();
                services.AddVideoArchive();
                services.AddTrendsArchive();
            }
            else
            {
                if(startupConfig.OnlineVideo) services.AddOnlineVideo();
                if(startupConfig.OnlineTrends) services.AddOnlineTrends();
                if(startupConfig.ArchiveVideo) services.AddVideoArchive();
                if(startupConfig.ArchiveTrends) services.AddTrendsArchive();
            }
        }

        [System.Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCommonServices(config); // This is required for all services

            StartAppServices(services);

            services.AddControllerAndInfrastrusture();
            services.AddSwagger();

            //services.AddRequestLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logFactory)
        {
            config.Validate(logFactory.CreateLogger<Startup>());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseRequestLogging();
            app.UseSerilogRequestLogging();
            app.ConfigureException(logFactory);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
        }
      
    }
}
