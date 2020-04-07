using System;
using System.Text;
using VideoReg.Infra.Services;

namespace VideoReg.Domain.Archive.BrigadeHistory
{
    public class BrigadeHistoryRep : IBrigadeHistoryRep
    {
        private readonly IFileSystemService fileSystem;
        private readonly IBrigadeHistoryConfig config;
        private readonly ILog log;
        private readonly IDateTimeService dateTimeService;

        public BrigadeHistoryRep(IBrigadeHistoryConfig config, IDateTimeService dateTimeService, IFileSystemService fileSystem, ILog log)
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
                log.Error($"Error then GetBrigadeHistory - {e.Message}");
                return new BrigadeHistory(log, dateTimeService);
            }
        }
    }
}