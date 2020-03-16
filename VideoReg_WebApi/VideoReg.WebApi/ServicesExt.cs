using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Serilog;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.Contract;
using VideoReg.WebApi.Core;
using VideoReg.WebApi.Test;
using VideoRegService;

namespace VideoReg.WebApi
{
    public static class ServicesExt
    {
        public static IServiceCollection AddSerilogServices(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
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
            services.AddSingleton<IRegInfoRep, RegInfoRep>();
            services.AddSingleton<ITrendsRep, FileTrendsRep>();
            services.AddSingleton<IImgRep, HttpImgRep>();
        }

        public static void AddTestDependencies(this IServiceCollection services)
        {
            services.AddSingleton<ICameraSourceRep, TestCameraRep>();
            services.AddSingleton<IRegInfoRep, TestRegInfo>();
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
}