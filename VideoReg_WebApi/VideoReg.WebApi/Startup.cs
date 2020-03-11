using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using VideoReg.Domain.Archive;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.OnlineVideo.SignalR;
using VideoReg.Domain.OnlineVideo.Store;
using VideoReg.Domain.Store;
using VideoReg.Domain.VideoRegInfo;
using VideoReg.Infra.Services;
using VideoReg.WebApi.Core;
using VideoRegService.Core;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace VideoReg.WebApi
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
            services.AddSerilogServices(configuration);
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
            
            services.AddTransient<IVideoArchiveSource, VideoArchiveSourceFS>();
            services.AddSingleton<IBrigadeHistoryRep, BrigadeHistoryRep>();
            services.AddSingleton<IVideoArchiveRep, VideoArchiveRep>();

            services.AddSingleton<IVideoRegInfoStore, VideoRegInfoStore>();
            services.AddSingleton<IClientVideoHub, ClientVideoHub>();

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
