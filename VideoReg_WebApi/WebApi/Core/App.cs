using Microsoft.Extensions.Logging;
using System;

namespace WebApi.Core
{
    public interface IApp
    {
        void Close(string sender);
    }

    public class App : IApp
    {
        private readonly ILogger<App> log;
        public App(ILogger<App> log)
        {
            this.log = log;
        }

        public void Close(string sender)
        {
            log.LogInformation($"Application close by {sender}");
            try
            {
                Environment.Exit(0);
            }
            catch (System.Security.SecurityException e)
            {
                log.LogError(e, $"Can not close application by {sender}. Error={e.Message}");
            }
        }
    }
}
