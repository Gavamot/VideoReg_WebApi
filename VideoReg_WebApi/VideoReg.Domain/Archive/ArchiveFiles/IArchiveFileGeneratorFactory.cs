using System;
using System.Collections.Generic;
using System.Text;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Domain.Archive.Config;

namespace VideoReg.Domain.Archive.ArchiveFiles
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
