using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace ApiProxy
{
    class Config
    {
        public Config(string[] settings)
        {
            var _ = ReadSettings(settings);
            SetFields(_);
        }

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
                if(string.IsNullOrEmpty(str) 
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