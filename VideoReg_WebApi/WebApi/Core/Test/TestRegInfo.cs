using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebApi.Contract;
using WebApi.Services;
using WebApi.Core;

namespace WebApiTest
{
    public class TestRegInfo : IRegInfoRep
    {
        public async Task<RegInfo> GetInfoAsync()
        {
            var json = await File.ReadAllTextAsync("Test/reg_info.json");
            var res = JsonConvert.DeserializeObject<RegInfo>(json);
            return res;
        }

        public Action<RegInfo> RegInfoChanged { get; set; }

        public string Vpn => "0.0.0.1";
    }
}
