using System.Security.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using Serilog;
using WebApi.Core;
using WebApi.CoreService.Core;
using WebApi.Archive;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;
using WebApi.OnlineVideo;
using WebApi.OnlineVideo.OnlineVideo;
using WebApi.OnlineVideo.Store;
using WebApi.Services;
using Microsoft.Extensions.Logging;
using WebApi.Core.SignalR;
using WebApi.Trends;

namespace WebApi
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        readonly Config config = new Config();
        private readonly bool swagger = false;
        private readonly bool log = false;
        private readonly bool passDataToServer = false;
        public static bool IsOn(string v) => v?.ToLower() == "on";
        

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
            configuration.GetSection("Settings").Bind(config);
            this.swagger = IsOn(configuration["Swagger"]);
            this.log = IsOn(configuration["Log"]);
            this.passDataToServer = IsOn(configuration["PassDataToServer"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            //services.AddSerilogServices(configuration);
            services.AddConfig(config);
            services.AddMemoryCache();
            //services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("AppDbContext"));

            // Передача на сервер asc web
            if (passDataToServer)
            {
                services.AddHostedService<VideoTransmitterHostedService>();
                services.AddHostedService<TrendsTransmitterHostedService>();
                services.AddSingleton<IClientAscHub, ClientAscHub>();
                services.AddSingleton<IArchiveTransmitter, ArchiveTransmitter>();
            }

            // Обновление камер
            services.AddHostedService<CameraHostedService>();
            services.AddSingleton<IRedisRep>(x => new RedisRep(config.Redis));

            services.AddHostedService<InitHostedService>();
          
            //Архивы 
            services.AddSingleton<IBrigadeHistoryRep, BrigadeHistoryRep>();
            services.AddSingleton<IArchiveFileGeneratorFactory, ArchiveFileGeneratorFactory>();
            services.AddSingleton<ICameraArchiveRep, CameraArchiveRep>();
            services.AddSingleton<ITrendsArchiveRep, TrendsArchiveUpCacheRep>();

            services.AddSingleton<IImgRep, HttpImgRep>();
            services.AddSingleton<ICameraStore, TransformImageStore>();
            services.AddSingleton<ICameraSettingsStore, CameraSettingsStore>();
            services.AddSingleton<IVideoConvector, ImagicVideoConvector>();

            services.AddSingleton<ITrendsRep, FileTrendsRep>();

#if (DEBUG)
            services.AddTestDependencies();
#else
            services.AddDependencies();
#endif

            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IFileSystemService, FileSystemService>();

            services.AddMapper();
            services.AddControllers()
                .AddMvcOptions(opt =>
                {
                    opt.ModelBinderProviders.Insert(0, new DateTimeMvc.DateTimeModelBinderProvider(new DateTimeService()));
                })
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.Converters.Add(new DateTimeMvc.DateTimeConverter(new DateTimeService()));
                });

            if (swagger)
            {
                services.AddSwagger();
            }

            if (log)
            {
                services.AddTransient<ILog, EmptyLogger>();
            }
            else
            {
                services.AddTransient<ILog, AppLogger>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> log)
        {
            config.Validate(log);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseSerilogRequestLogging();

            if (swagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.RoutePrefix = string.Empty;
                });
            }
        }
    }
}
