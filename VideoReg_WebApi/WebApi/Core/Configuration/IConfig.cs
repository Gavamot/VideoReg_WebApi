using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;

namespace WebApi.Configuration
{
    public interface IConfig : ICameraConfig, IArchiveConfig, ITrendsConfig, 
        IImagePollingConfig, IBrigadeHistoryConfig, IVideoTransmitterConfig, IRegInfoConfig
    {
    }
}
