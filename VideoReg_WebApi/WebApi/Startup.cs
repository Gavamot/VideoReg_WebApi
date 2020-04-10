using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using WebApi.Core;
using WebApi.CoreService;
using WebApi.CoreService.Core;
using WebApi.Archive;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.OnlineVideo;
using WebApi.OnlineVideo.SignalR;
using WebApi.OnlineVideo.Store;
using WebApi.Services;
using WebApi.Trends;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace WebApi
{
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
            services.AddHttpClient();
             

            services.AddSerilogServices(configuration);
            services.AddConfig(config);
            //services.AddMemoryCache();

            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IFileSystemService, FileSystemService>();
            services.AddSingleton<ILog, AppLogger>();
            services.AddSingleton<IRedisRep>(x => new RedisRep(config.Redis));

            services.AddSingleton<ITrendsArchiveStructureStore, TrendsArchiveStructureStore>();
            services.AddSingleton<IVideoArchiveStructureStore, VideoArchiveStructureStore>();
            services.AddSingleton<IVideoArchiveSource, VideoArchiveSourceFS>();
            services.AddSingleton<IBrigadeHistoryRep, BrigadeHistoryRep>();
            services.AddSingleton<IVideoArchiveRep, VideoArchiveRep>();
            services.AddSingleton<IArchiveFileGeneratorFactory, ArchiveFileGeneratorFactory>();
            
            services.AddSingleton<IVideoConvector, ImagicVideoConvector>();
            services.AddSingleton<IClientVideoHub, ClientVideoHub>();

            services.AddSingleton<ICameraStore, TransformImageStore>();
            services.AddSingleton<ICameraSettingsStore, CameraSettingsStore>();

            services.AddTestDependencies();
//#if (DEBUG)
//            services.AddTestDependencies();
//            #else
//                services.AddDependencies();
//            #endif

            services.AddMapper();

            services.AddHttpClient();
            services.AddControllers()
                .AddMvcOptions(opt =>
                {
                    opt.ModelBinderProviders.Insert(0, new DateTimeMvc.DateTimeModelBinderProvider());
                })
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.Converters.Add(new DateTimeMvc.DateTimeConverter(new DateTimeService()));
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
            app.UseEndpoints( endpoints => { endpoints.MapControllers(); });
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
