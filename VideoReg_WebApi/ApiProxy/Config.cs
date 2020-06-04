using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace ApiProxy
{
    class Config
    {
        public const string fileName = "config.txt";

        /// <summary>
        /// ReadConfig
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="System.UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public static Config ReadConfig()
        {
            var lines = File.ReadAllLines(fileName);
            var res = new Config(lines);
            return res;
        }

        public Config(string[] settings)
        {
            var _ = ReadSettings(settings);
            SetFields(_);
        }

        /// <summary>
        /// SetFields
        /// </summary>
        /// <param name="settings"></param>
        /// <exception cref="System.FieldAccessException">Ignore.</exception>
        /// <exception cref="TargetException">Ignore.</exception>
        private void SetFields(Dictionary<string, string> settings)
        {
            var fields = typeof(Config).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                string setting = settings[field.Name];
                object value = setting;
                if (field.FieldType == typeof(IPEndPoint))
                    value = ReadEndPoint(setting);
                if (field.FieldType == typeof(int))
                    value = int.Parse(setting);
                field.SetValue(this, value);
            }
        }

        private Dictionary<string, string> ReadSettings(string[] settings)
        {
            var res = new Dictionary<string, string>();
            foreach (var s in settings)
            {
                var str = s.Trim();
                if (string.IsNullOrEmpty(str)
                   || str.StartsWith("//")
                   || str.StartsWith("#")
                   || !str.Contains("=")) continue;
                var keyValue = str.Split("=");
                res.Add(keyValue[0].Trim(), keyValue[1].Trim());
            }
            return res;
        }

        private IPEndPoint ReadEndPoint(string value)
        {
            var ipPort = value.Split(":");
            return new IPEndPoint(IPAddress.Parse(ipPort[0]), int.Parse(ipPort[1]));
        }

        public readonly IPEndPoint client;
        public readonly IPEndPoint api;

        public readonly int receiveBufferSizeBytes = 2097152;
        public readonly int sendBufferSizeBytes = 2097152;
        public readonly int bufferSizeBytes = 4096;
        public readonly int receiveTimeoutMs = 3000;
        public readonly int sendTimeoutMs = 3000;
        public readonly int iterationDelayMs = 5;
        public readonly int firstDelayMs = 500;
        public readonly int exceptionDelayMs = 1000;
    }
}