using Microsoft.Extensions.DependencyInjection;
using VideoReg.Domain.Archive.Config;

namespace VideoReg.WebApi.Core
{
    public static class ConfigExt
    {
        public static void AddConfig(this IServiceCollection services, Config config)
        {
            var interfaces = config.GetType().GetInterfaces();
            foreach (var face in interfaces)
            {
                services.AddSingleton(face, config);
            }
        }
    }

    public class Config : IConfig
    {
        public string ChannelArchivePath { get; protected set; }
        public int UpdateChannelArchiveMs { get; protected set; }
        public string VideoArchivePath { get; protected set; }
        public int VideoArchiveUpdateTimeMs { get; }
        public int UpdateVideoArchiveMs { get; protected set; }
        public int CameraUpdateIntervalMs { get; protected set; }
        public int CameraGetImageTimeoutMs { get; protected set; }
        public string Redis { get; protected set; }
        public string DbConnectionString { get; protected set; }
        public string TrendsFileName { get; set; }
    }
}