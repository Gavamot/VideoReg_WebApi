using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
//using Serilog;
using Swashbuckle.AspNetCore.Filters;
using WebApi.Core;
using WebApiTest;
using WebApi.CoreService;
using WebApi.Configuration;
using WebApi.Controllers;
using WebApi.OnlineVideo;
using WebApi.Trends;

namespace WebApi
{
    public static class ServicesExt
    {
        //public static IServiceCollection AddSerilogServices(this IServiceCollection services, IConfiguration configuration)
        //{
        //    Log.Logger = new LoggerConfiguration()
        //        .ReadFrom.Configuration(configuration)
        //        .CreateLogger();
        //    AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
        //    return services.AddSingleton(Log.Logger);
        //}

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

                c.OperationFilter<AddHeaderOperationFilter>(AppController.HeaderTimestamp, AppController.ImageDateHeaderFormat, false);
                c.OperationFilter<AddResponseHeadersFilter>();

                //c.OperationFilter<SecurityRequirementsOperationFilter>();
                //c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                //{
                //    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                //    In = ParameterLocation.Header,
                //    Name = "Authorization",
                //    Type = SecuritySchemeType.ApiKey
                //});
            });
        }

        public static void AddDependencies(this IServiceCollection services)
        {
            services.AddTransient<ICameraSourceRep, RedisCameraSourceRep>();
            services.AddTransient<IRegInfoRep, RegInfoRep>();
            services.AddTransient<ITrendsRep, FileTrendsRep>();
            services.AddSingleton<IImgRep, HttpImgRep>();
        }

        public static void AddTestDependencies(this IServiceCollection services)
        {
            services.AddSingleton<ICameraSourceRep, TestCameraRep>();
            services.AddSingleton<IRegInfoRep, TestRegInfo>();
            services.AddTransient<ITrendsRep, FileTrendsRep>();
            //services.AddSingleton<ITrendsRep, TestTrendsRep>();
            services.AddSingleton<IImgRep, HttpImgRep>();
            //services.AddSingleton<IImgRep, TestRandomImgRep>();
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