using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Infra.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime GetNow()
        {
            return DateTime.Now;
        }
    }
}
