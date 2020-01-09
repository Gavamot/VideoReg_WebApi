using System.Text;
using VideoReg.Infra.Services;

namespace VideoReg.Domain.Archive.BrigadeHistory
{
    public class BrigadeHistoryRep : IBrigadeHistoryRep
    {
        private readonly IFileSystemService fileSystem;
        private readonly IBrigadeHistoryConfig config;
        private readonly ILog log;

        public BrigadeHistoryRep(IFileSystemService fileSystem, IBrigadeHistoryConfig config, ILog log)
        {
            this.fileSystem = fileSystem;
            this.config = config;
            this.log = log;
        }

        string GetFileText() 
            => fileSystem.ReadFileText(config.BrigadeHistoryFileName, Encoding.UTF8);

        public IBrigadeHistory GetBrigadeHistory() =>
            new BrigadeHistory(GetFileText(), log);
    }
}