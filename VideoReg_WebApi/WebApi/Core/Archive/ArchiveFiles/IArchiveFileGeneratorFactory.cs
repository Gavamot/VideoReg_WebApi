using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;

namespace WebApi.Archive.ArchiveFiles
{
    public interface IArchiveFileGeneratorFactory
    {
        ArchiveFileGenerator Create();
    }
    public class ArchiveFileGeneratorFactory : IArchiveFileGeneratorFactory
    {
        private readonly IBrigadeHistoryRep brigadeHistoryRep;
        private readonly IArchiveConfig config;
        public ArchiveFileGeneratorFactory(IBrigadeHistoryRep brigadeHistoryRep, IArchiveConfig config)
        {
            this.brigadeHistoryRep = brigadeHistoryRep;
            this.config = config;
        }
      
        public ArchiveFileGenerator Create()
        {
            var brigadeHistory = this.brigadeHistoryRep.GetBrigadeHistory();
            return ArchiveFileGenerator.Create(brigadeHistory, config);
        }
    }
}
