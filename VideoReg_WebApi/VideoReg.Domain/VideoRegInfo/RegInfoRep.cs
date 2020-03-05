using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using VideoReg.Domain.VideoRegInfo;
using VideoReg.Infra.Services;

namespace VideoRegService
{
    public class RegInfoRep : IRegInfoRep
    {
        public const string BrigadeCodeFile = "/home/v-1336/projects/dist/ASCWeb/brigade_code";
        public const int EmptyBrigadeCode = int.MinValue;

        private readonly ILog log;
        public Action<RegInfo> RegInfoChanged { get; set; }
        public RegInfoRep(ILog log)
        {
            this.log = log;
        }

        public async Task<RegInfo> GetInfo()
        {
            var (ip, vpn) = GetIpAddress();
            var brigadeCode = await GetBrigadeCode();
            return new RegInfo
            {
                Ip = ip,
                Vpn = vpn,
                BrigadeCode = brigadeCode
            };
        }

        private NetworkInterface[] TryGetNetworkInterfaces()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces();
            }
            catch(Exception e)
            {
                log.Error($"Cannot get Network interfaces [RegInfoRep.TryGetNetworkInterfaces()]({e.Message})");
            }
            return new NetworkInterface[0];
        }

        private (string, string) GetIpAddress()
        {
            string ip = "";
            string vpnIp = "";
            foreach (var netInterface in TryGetNetworkInterfaces())
            {
                var ipProps = netInterface.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    var netAddr = addr.Address.MapToIPv4().ToString();
                    if (netAddr.StartsWith("10."))
                    {
                        vpnIp = netAddr;
                    }
                    if (!netAddr.StartsWith("10.") &&
                        !netAddr.StartsWith("192.") &&
                        !netAddr.StartsWith("0.") &&
                        !netAddr.StartsWith("127."))
                    {
                        ip = netAddr;
                    }
                }
            }
            return (ip, vpnIp);
        }

        private async Task<string> TryReadFirstStringFromFileAsync(string file)
        {
            try
            {
                var fd = await File.ReadAllLinesAsync(BrigadeCodeFile);
                return fd[0];
            }
            catch (Exception e)
            {
                log.Error($"Cannot read file {BrigadeCodeFile}. [RegInfoRep.TryReadFirstStringFromFileAsync] ({e.Message})");
                return string.Empty;
            }
        }

        private async Task<int> GetBrigadeCode()
        { 
            var brigadeString = await TryReadFirstStringFromFileAsync(BrigadeCodeFile);
            if(string.IsNullOrEmpty(brigadeString))
                return EmptyBrigadeCode;
            try
            {
                return int.Parse(brigadeString);
            }
            catch(Exception e)
            {
                log.Error($"Cannot parse {brigadeString} to int value. [RegInfoRep.GetBrigadeCode] ({e.Message})");
                return EmptyBrigadeCode;
            }
        }

       
    }
}
