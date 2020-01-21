using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using VideoReg.Domain.Archive;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.OnlineVideo.Store;
using VideoReg.Domain.Store;
using VideoReg.Domain.VideoRegInfo;
using VideoReg.Infra.Services;
using VideoReg.WebApi.Core;
using VideoReg.WebApi.Test;
using VideoRegService;
using VideoRegService.Core;
using VideoRegService.Core.Archive;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace VideoReg.WebApi
{
    public static class ServicesExt
    {
        public static IServiceCollection AddSerilogServices(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger(); 
            //Log.Logger = new LoggerConfiguration()
            //    .MinimumLevel.Warning()
            //    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //    .Enrich.FromLogContext()
            //     .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            //     .WriteTo.File(new RenderedCompactJsonFormatter(), "logs/log.ndjson")
            //    .CreateLogger();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
            return services.AddSingleton(Log.Logger);
        }

        public static void AddConfig(this IServiceCollection services, Config config)
        {
            var interfaces = config.GetType().GetInterfaces();
            foreach (var face in interfaces)
            {
                services.AddSingleton(face, config);
            }
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Video registrators",
                    Version = "v1"
                });

                var xmlFile = $"{ Assembly.GetExecutingAssembly().GetName().Name }.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.DescribeAllEnumsAsStrings();
                c.DescribeStringEnumsInCamelCase();
            });
        }

        public static void AddDependencies(this IServiceCollection services)
        {
            services.AddSingleton<ICameraSourceRep, RedisCameraSourceRep>();
            services.AddSingleton<IVideoRegInfoRep, RedisVideoRegInfoRep>();
            services.AddSingleton<ITrendsRep, FileTrendsRep>();
            services.AddSingleton<IImgRep, HttpImgRep>();
        }

        public static void AddTestDependencies(this IServiceCollection services)
        {
            services.AddSingleton<ICameraSourceRep, TestCameraRep>();
            services.AddSingleton<IVideoRegInfoRep, TestVideoRegInfo>();
            services.AddSingleton<ITrendsRep, TestFileTrendsRep>();
            services.AddSingleton<IImgRep, TestRandomImgRep>();
        }

        public static void AddMapper(this IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            mappingConfig.AssertConfigurationIsValid();
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
    public class Startup
    {
        private readonly IConfiguration configuration;
        readonly IHostingEnvironment env;
        readonly Config config = new Config();
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            this.env = env;
            this.configuration = configuration;
            configuration.GetSection("Settings").Bind(config);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSerilogServices(configuration);

            services.AddHttpClient();
            services.AddConfig(config);
            services.AddMemoryCache();

            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IFileSystemService, FileSystemService>();

            services.AddSingleton<ILog, AppLogger>();
            services.AddSingleton<IRedisRep>(x => new RedisRep(config.Redis));

            services.AddDependencies();
            services.AddSingleton<IVideoConvector, ImagicVideoConvector>();
            services.AddSingleton<ICameraStore, TransformImageStore>();
            services.AddSingleton<ICameraSettingsStore, CameraSettingsStore>();
            services.AddHostedService<VideoArchiveUpdateService>();

            services.AddSingleton<IVideoArchiveSource, VideoArchiveSourceFS>();
            services.AddSingleton<IBrigadeHistoryRep, BrigadeHistoryRep>();
            services.AddSingleton<IVideoArchiveRep, VideoArchiveRep>();
            services.AddHostedService<CameraUpdateService>();
            services.AddMapper();

            services.AddHttpClient();

            services.AddControllers()
                .AddMvcOptions(opt =>
                {
                    opt.ModelBinderProviders.Insert(0, new DateTimeMvc.DateTimeModelBinderProvider());
                })
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.Converters.Add(new DateTimeMvc.DateTimeConverter());
                });

            services.AddSwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSerilogRequestLogging();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

        }
    }
}
