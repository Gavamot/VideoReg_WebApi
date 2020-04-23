using System;
using Microsoft.Extensions.Logging;
//using Serilog;
using WebApi.Services;

namespace WebApi.Core
{

    public class EmptyLogger : ILog
    {
        public void Debug(string message, object obj = default)
        {
           
        }

        public void Info(string message, object obj = default)
        {
          
        }

        public void Warning(string message, object obj = default)
        {
           
        }

        public void Error(string message, Exception e = default, object obj = default)
        {
            
        }

        public void Error(Exception e)
        {
           
        }

        public void Fatal(string message, Exception e = default, object obj = default)
        {
            
        }
    }

    public class AppLogger : ILog
    {
        private readonly ILogger log;
        public AppLogger(ILogger<AppLogger> log)
        {
            this.log = log;
        }

        public void Debug(string message, object obj = default)
        {
            log.LogDebug(message, obj);
        }

        public void Info(string message, object obj = default)
        {
            log.LogInformation(message, obj);
        }

        public void Warning(string message, object obj = default)
        {
            log.LogWarning(message, obj);
        }

        public void Error(string message, Exception e = default, object obj = default)
        {
            log.LogError(message, e, obj);
        }

        public void Error(Exception e)
        {
            Error(e.Message, e);
        }

        public void Fatal(string message, Exception e = default, object obj = default)
        {
            log.LogError(message, e, obj);
        }
    }
}
