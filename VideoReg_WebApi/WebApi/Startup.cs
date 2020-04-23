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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApi.Core.SignalR;

namespace WebApi
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        readonly Config config = new Config();
        private readonly bool swagger = false;
        private readonly bool disableLog = false;
        public static bool IsOn(string v) => v.ToLower() == "on";

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
            configuration.GetSection("Settings").Bind(config);
            this.swagger = IsOn(configuration["Swagger"]);
            this.disableLog = IsOn(configuration["DisableLog"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            //services.AddSerilogServices(configuration);
            services.AddConfig(config);
            services.AddMemoryCache();

            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IFileSystemService, FileSystemService>();

            if (disableLog)
            {
                services.AddTransient<ILog, EmptyLogger>();
            }
            else
            {
                services.AddTransient<ILog, AppLogger>();
            }
           
            services.AddSingleton<IRedisRep>(x => new RedisRep(config.Redis));

            services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("AppDbContext"));

            //Архивы 
            services.AddTransient<IBrigadeHistoryRep, BrigadeHistoryRep>();
            services.AddTransient<IArchiveFileGeneratorFactory, ArchiveFileGeneratorFactory>();
            services.AddSingleton<ICameraArchiveRep, CameraArchiveRep>();
            services.AddSingleton<ITrendsArchiveRep, TrendsArchiveUpCacheRep>();

            services.AddTransient<IVideoConvector, ImagicVideoConvector>();
            services.AddSingleton<IClientAscHub, ClientAscHub>();
            services.AddSingleton<ICameraStore, TransformImageStore>();
            services.AddSingleton<ICameraSettingsStore, CameraSettingsStore>();
            services.AddSingleton<IArchiveTransmitter, ArchiveTransmitter>();
            services.AddTestDependencies();
//#if (DEBUG)
//            services.AddTestDependencies();
//            #else
//                services.AddDependencies();
//            #endif

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
