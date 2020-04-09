using System;
using System.Threading.Tasks;
using WebApi.Contract;
using WebApi.Services;
using WebApi.Core;

namespace WebApiTest
{
    public class TestRegInfo : IRegInfoRep
    {
        readonly IDateTimeService dateService;
        public TestRegInfo(IDateTimeService dateService)
        {
            this.dateService = dateService;
        }

        public async Task<RegInfo> GetInfoAsync()
        {
            return new RegInfo
            {
                BrigadeCode = 1,
                Ip = "240.11.12.12",
                Vpn = "10.1.3.1"
            };
        }

        public Action<RegInfo> RegInfoChanged { get; set; }
    }
}
