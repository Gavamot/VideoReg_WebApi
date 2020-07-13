using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using WebApi.Core;
using WebApi.CoreService;
using WebApi.Configuration;
using WebApi.OnlineVideo;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebApi.Services;
using WebApi.CoreService.Core;
using WebApi.OnlineVideo.Store;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Archive;
using WebApi.Core.OnlineVideo;
using WebApi.Trends;

namespace WebApi
{
    public static class ServicesExt
    {
        [Obsolete]
        public static IServiceCollection AddSwagger(this IServiceCollection services)
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

                //c.OperationFilter<AddHeaderOperationFilter>(AppController.HeaderTimestamp, AppController.ImageDateHeaderFormat, false);
                //c.OperationFilter<AddResponseHeadersFilter>();

                //c.OperationFilter<SecurityRequirementsOperationFilter>();
                //c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                //{
                //    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                //    In = ParameterLocation.Header,
                //    Name = "Authorization",
                //    Type = SecuritySchemeType.ApiKey
                //});
            });
            return services;
        }

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            mappingConfig.AssertConfigurationIsValid();
            IMapper mapper = mappingConfig.CreateMapper();
            services.TryAddSingleton(mapper);
            return services;
        }

        public static IServiceCollection AddControllerAndInfrastrusture(this IServiceCollection services)
        {
            services.AddMapper();
            services.TryAddTransient<IDateTimeService, DateTimeService>();
            services.AddControllers()
                .AddMvcOptions(opt =>
                {
                    opt.ModelBinderProviders.Insert(0, new DateTimeMvc.DateTimeModelBinderProvider(new DateTimeService()));
                })
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.Converters.Add(new DateTimeMvc.DateTimeConverter(new DateTimeService()));
                });
            return services;
        }

        public static IServiceCollection AddAscHttpClient(this IServiceCollection services)
        {
            services.TryAddSingleton<ICertificateRep, CertificateFileRep>();
            services.AddHttpClient<IAscHttpClient, AscHttpClient>()
                .ConfigurePrimaryHttpMessageHandler(serviceProvider => {
                    var clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    var cert = serviceProvider.GetService<ICertificateRep>().GetCertificate();
                    clientHandler.ClientCertificates.Add(cert);
                    clientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    clientHandler.MaxConnectionsPerServer = 12; // 9 онайн камер + 1 онлайн тренды + 1 архив камеры + 1 архив тренды
                    return clientHandler;
                });
            return services;
        }
  
        /// <summary>
        /// AddCommonServices
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="TargetInvocationException">Ignore.</exception>
        public static IServiceCollection AddCommonServices(this IServiceCollection services, Config config)
        {
            IServiceCollection AddConfig()
            {
                var interfaces = config.GetType().GetInterfaces();
                foreach (var face in interfaces)
                {
                    services.AddSingleton(face, config);
                }
                return services;
            }

            AddConfig();
            services.AddSingleton<IFileSystemService, FileSystemService>();
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IApp, App>();
            return services;
        }

        public static IServiceCollection AddOnlineVideoHttp(this IServiceCollection services)
        {
            services.AddHostedService<CameraHttpHostedService>();
            services.AddHttpClient();
            services.TryAddSingleton<IImgHttpRep, HttpImgRep>();
            services.TryAddSingleton<ICameraStore, TransformImageStore>();
            services.TryAddSingleton<ICameraSettingsStore, CameraSettingsStore>();
            services.TryAddSingleton<IVideoConvector, ImagicVideoConvector>();
            services.TryAddSingleton<ICameraHttpSourceRep, RedisCameraHttpSourceRep>();
            services.TryAddSingleton<IRedisRep, RedisRep>();
            return services;
        }

        public static IServiceCollection AddOnlineVideoRedis(this IServiceCollection services)
        {
            services.TryAddSingleton<IRedisRep, RedisRep>();
            services.TryAddSingleton<ISnapshotRep, RedisSnapshotRep>();
            services.AddHostedService<CameraRedisHostedService>();
            services.TryAddSingleton<ICameraStore, TransformImageStore>();
            services.TryAddSingleton<ICameraSettingsStore, CameraSettingsStore>();
            services.TryAddSingleton<IVideoConvector, ImagicVideoConvector>();
            return services;
        }

        public static IServiceCollection AddOnlineTrends(this IServiceCollection services)
        {
            services.TryAddSingleton<ITrendsRep, FileTrendsRep>();
            services.TryAddSingleton<IFileSystemService, FileSystemService>();
            return services;
        }

        public static IServiceCollection AddTrendsArchive(this IServiceCollection services)
        {
            services.TryAddSingleton<IArchiveFileGeneratorFactory, ArchiveFileGeneratorFactory>();
            services.TryAddSingleton<IBrigadeHistoryRep, BrigadeHistoryRep>();
            services.AddMemoryCache();
            services.TryAddSingleton<ITrendsArchiveRep, TrendsArchiveCahceUpdatebleRep>();
            return services;
        }

        public static IServiceCollection AddVideoArchive(this IServiceCollection services)
        {
            services.TryAddSingleton<IArchiveFileGeneratorFactory, ArchiveFileGeneratorFactory>();
            services.TryAddSingleton<IBrigadeHistoryRep, BrigadeHistoryRep>();
            services.AddMemoryCache();
            services.TryAddSingleton<ICameraArchiveRep, CameraArchiveCahceUpdatebleRep>();
            return services;
        }
    }
}