﻿using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using WebApi.Configuration;
using WebApi.Contract;
using WebApi.Services;
using WebApi.Core;
using WebApi.CoreService.Core;
using Microsoft.Extensions.Logging;

namespace WebApi
{
    public class RegInfoRep : IRegInfoRep
    {
        public const int EmptyBrigadeCode = int.MinValue;

        readonly IRedisRep redis;
        private readonly ILogger<RegInfoRep> log;
        private readonly IRegInfoConfig config;
        public Action<RegInfo> RegInfoChanged { get; set; }
        public string BrigadeCodeFile => config.BrigadeCodePath;

        string vpn = string.Empty;
        public string Vpn
        {
            get
            {
                if (string.IsNullOrEmpty(vpn))
                {
                    vpn = GetIpAddress().Item2;
                }
                return vpn;
            }
        }

        public string ApiVersion => "10.0";

        private string regSerial = "";
        public string RegSerial 
        { 
            get
            {
                if(string.IsNullOrEmpty(regSerial))
                    regSerial = redis.GetString("serial_number").Result;
                return regSerial;
            } 
        }

        private const string VpnStartWith = "10.";

        public RegInfoRep(ILogger<RegInfoRep> log, IRegInfoConfig config, IRedisRep redis)
        {
            this.log = log;
            this.config = config;
            this.redis = redis;
            _ = WatchToBrigadeCode();
        }

        public async Task<RegInfo> GetInfoAsync()
        {
            var brigadeCode = await GetBrigadeCodeAsync();
            return GetInfo(brigadeCode);
        }

        RegInfo GetInfo(int brigadeCode)
        {
            var (ip, vpn) = GetIpAddress();
            return new RegInfo
            {
                Ip = ip,
                Vpn = vpn,
                BrigadeCode = brigadeCode,
                RegSerial = regSerial
            };
        }

        private async Task WatchToBrigadeCode()
        {
            if (!File.Exists(BrigadeCodeFile))
            {
                var message = $"Brigade code file is does not exist {BrigadeCodeFile}";
                log.LogError(message);
               // Environment.Exit(1);
            }

            int oldBrigadeCode = -1;
            while (true)
            {
                var newBrigadeCode = await GetBrigadeCodeAsync();
                if (oldBrigadeCode != newBrigadeCode)
                {
                    oldBrigadeCode = newBrigadeCode;
                    var regInfo = GetInfo(newBrigadeCode);
                    log.LogInformation($"BrigadeCode was changed to {regInfo.BrigadeCode}");
                    RegInfoChanged?.Invoke(regInfo);
                }
                await Task.Delay(1000);
            }
        }

        private NetworkInterface[] TryGetNetworkInterfaces()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces();
            }
            catch(Exception e)
            {
                log.LogError($"Cannot get Network interfaces [RegInfoRep.TryGetNetworkInterfaces()]({e.Message})");
            }
            return new NetworkInterface[0];
        }

        private (string, string) GetIpAddress()
        {
            string ip = "";
            string vpnIp = "";
            foreach (var netInterface in TryGetNetworkInterfaces())
            {
                foreach (var addr in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if(addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
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
                log.LogError($"Cannot read file {BrigadeCodeFile}. [RegInfoRep.TryReadFirstStringFromFileAsync] ({e.Message})");
                return string.Empty;
            }
        }

        public async Task<int> GetBrigadeCodeAsync()
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
                log.LogError($"Cannot parse {brigadeString} to int value. [RegInfoRep.GetBrigadeCode] ({e.Message})");
                return EmptyBrigadeCode;
            }
        }
    }
}
