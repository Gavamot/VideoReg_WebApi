using System;
using Serilog;
using WebApi.Services;

namespace WebApi.Core
{
    public class AppLogger : ILog
    {
        private readonly ILogger log;
        public AppLogger()
        {
            this.log = Log.Logger;
        }

        public void Debug(string message, object obj = default)
        {
            log.Debug(message, obj);
        }

        public void Info(string message, object obj = default)
        {
            log.Information(message, obj);
        }

        public void Warning(string message, object obj = default)
        {
            log.Warning(message, obj);
        }

        public void Error(string message, Exception e = default, object obj = default)
        {
            log.Error(message, e, obj);
        }

        public void Fatal(string message, Exception e = default, object obj = default)
        {
            log.Fatal(message, e, obj);
        }
    }
}
