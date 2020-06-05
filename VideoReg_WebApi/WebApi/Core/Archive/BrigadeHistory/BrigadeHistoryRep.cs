using Microsoft.Extensions.Logging;
using System;
using System.Text;
using WebApi.Services;

namespace WebApi.Archive.BrigadeHistory
{
    public class BrigadeHistoryRep : IBrigadeHistoryRep
    {
        private readonly IFileSystemService fileSystem;
        private readonly IBrigadeHistoryConfig config;
        private readonly ILogger<BrigadeHistoryRep> log;
        private readonly IDateTimeService dateTimeService;

        public BrigadeHistoryRep(IBrigadeHistoryConfig config, IDateTimeService dateTimeService, IFileSystemService fileSystem, ILogger<BrigadeHistoryRep> log)
        {
            this.config = config;
            this.log = log;
            this.dateTimeService = dateTimeService;
            this.fileSystem = fileSystem;
        }

        string GetFileText() => fileSystem.ReadFileText(config.BrigadeHistoryFileName, Encoding.UTF8);

        public IBrigadeHistory GetBrigadeHistory()
        {
            try
            {
                var text = GetFileText();
                return new BrigadeHistory(text, dateTimeService, log);
            }
            catch (Exception e)
            {
                log.LogError($"Error then GetBrigadeHistory - {e.Message}");
                return new BrigadeHistory(log, dateTimeService);
            }
        }
    }
}