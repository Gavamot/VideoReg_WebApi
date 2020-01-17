using System.Threading.Tasks;
using ApiServicePack;
using VideoReg.Domain.VideoRegInfo;
using VideoReg.Infra.Services;

namespace VideoReg.WebApi.Test
{
    public class TestVideoRegInfo : IVideoRegInfoRep
    {
        readonly IDateTimeService dateService;
        public TestVideoRegInfo(IDateTimeService dateService)
        {
            this.dateService = dateService;
        }

        public async Task<VideoRegInfoDto> GetInfo()
        {
            return new VideoRegInfoDto
            {
                BrigadeCode = 1,
                Cameras = new []{ 1, 2 },
                CurrentDate = dateService.GetNow(),
                IveSerial = "FSW-2333",
                Version = "0.0.1 Beta",
                VideoRegSerial = "QQQ-QXE123"
            };
        }
    }
}
