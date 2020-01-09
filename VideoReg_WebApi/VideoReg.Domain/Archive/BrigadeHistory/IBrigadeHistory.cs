using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.Archive.BrigadeHistory
{
    public interface IBrigadeHistory
    {
        int? GetBrigadeCode(DateTime pdt);
    }
}
