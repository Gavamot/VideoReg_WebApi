using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Infra.Services;

namespace VideoReg.WebApi.Test
{
    public class BrigadeHistoryTest : IBrigadeHistoryRep
    {
        private ILog log;
        public BrigadeHistoryTest(ILog log)
        {
            this.log = log;
        }

        string GetFileText() => File.ReadAllText("../../../Test/brigade.history", Encoding.UTF8);

        public IBrigadeHistory GetBrigadeHistory() => new BrigadeHistory(GetFileText(), log);
    }
}
