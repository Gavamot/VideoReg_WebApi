using System;
using System.IO;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VideoReg.Domain.Config;
using VideoReg.Domain.Contract;
using VideoReg.Infra.Services;

namespace VideoRegService
{
    public class RegInfoRep : IRegInfoRep
    {
        public const int EmptyBrigadeCode = int.MinValue;

        private readonly ILog log;
        private readonly IRegInfoConfig config;
        public Action<RegInfo> RegInfoChanged { get; set; }
        public string BrigadeCodeFile => config.BrigadeCodePath;
        private const string VpnStartWith = "10.";
        private FileSystemWatcher watcher;
        public RegInfoRep(ILog log, IRegInfoConfig config)
        {
            this.log = log;
            this.config = config;
            WatchToBrigadeCode();
        }

        ~RegInfoRep()
        {
            watcher.Dispose();
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

        private void WatchToBrigadeCode()
        {
            if (!File.Exists(BrigadeCodeFile))
            {
                var message = $"Brigade code file is does not exist {BrigadeCodeFile}";
                log.Fatal(message);
                Environment.Exit(1);
            }

            FileInfo fd = new FileInfo(config.BrigadeCodePath);
            watcher = new FileSystemWatcher
            {
                Path = fd.Directory.FullName,
            };
            watcher.Filter = fd.Name;
            watcher.Changed += (sender, args) =>
            {
                var regInfo = GetInfo().Result;
                log.Info($"BrigadeCode was changed to {regInfo.BrigadeCode}");
                RegInfoChanged(regInfo);
            };
            watcher.EnableRaisingEvents = true;
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
                    var netAddress = addr.Address.MapToIPv4().ToString();
                    if (netAddress.StartsWith(VpnStartWith))
                    {
                        vpnIp = netAddress;
                    }
                    if (IsInternalAddress(netAddress))
                    {
                        ip = netAddress;
                    }
                }
            }
            return (ip, vpnIp);
        }

        private bool IsInternalAddress(string netAddress)
        {
            var localAddress = new string[]
            {
                VpnStartWith,
                "192.",
                "0",
                "127."
            };

            foreach (var address in localAddress)
            {
                if (netAddress.StartsWith(address))
                    return false;
            }
            return true;
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
