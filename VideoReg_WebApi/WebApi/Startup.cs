using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
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

namespace WebApi
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        readonly Config config = new Config();

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
            configuration.GetSection("Settings").Bind(config);
            config.Validate(new AppLogger());
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSerilogServices(configuration);
            services.AddConfig(config);
            services.AddMemoryCache();

            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IFileSystemService, FileSystemService>();
            services.AddTransient<ILog, AppLogger>();
            services.AddSingleton<IRedisRep>(x => new RedisRep(config.Redis));

            services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("AppDbContext"));

            //������ 
            services.AddTransient<IBrigadeHistoryRep, BrigadeHistoryRep>();
            services.AddTransient<IArchiveFileGeneratorFactory, ArchiveFileGeneratorFactory>();
            services.AddSingleton<ICameraArchiveRep, CameraArchiveRep>();
            services.AddSingleton<ITrendsArchiveRep, TrendsArchiveUpCacheRep>();

            services.AddTransient<IVideoConvector, ImagicVideoConvector>();
            services.AddSingleton<IClientAscHub, ClientAscHub>();
            services.AddSingleton<ICameraStore, TransformImageStore>();
            services.AddSingleton<ICameraSettingsStore, CameraSettingsStore>();

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
