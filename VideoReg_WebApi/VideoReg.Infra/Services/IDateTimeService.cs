using System;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Infra.Services
{
    public interface IDateTimeService
    {
        DateTime Now();
        string NowToStringFull();
        string NowToStringFullMs();
        DateTime Parse(string date);
        DateTime Parse(string date, string preferFormat);
        string ToStringFull(DateTime date);
        string ToStringFullMs(DateTime date);

    }
}
