using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Infra.Services
{
    public interface IDateTimeService
    {
        DateTime GetNow();
    }
}
