using System.Threading.Tasks;
using ApiServicePack;
using VideoReg.Domain.VideoRegInfo;
using VideoReg.Infra.Services;

namespace VideoReg.WebApi.Test
{
    public class TestRegInfo : IRegInfoRep
    {
        readonly IDateTimeService dateService;
        public TestRegInfo(IDateTimeService dateService)
        {
            this.dateService = dateService;
        }

        public async Task<RegInfo> GetInfo()
        {
            return new RegInfo
            {
                BrigadeCode = 1,
                Ip = "240.11.12.12",
                Vpn = "10.1.3.1"
            };
        }
    }
}
