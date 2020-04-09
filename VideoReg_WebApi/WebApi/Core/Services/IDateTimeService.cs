using System;

namespace WebApi.Services
{
    public interface IDateTimeService
    {
        DateTime GetNow();
        string NowToStringFull();
        string NowToStringFullMs();
        DateTime Parse(string date);
        DateTime Parse(string date, string preferFormat);
        string ToStringFull(DateTime date);
        string ToStringFullMs(DateTime date);

    }
}
