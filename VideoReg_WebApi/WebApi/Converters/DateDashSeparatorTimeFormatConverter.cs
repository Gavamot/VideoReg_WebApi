using Newtonsoft.Json.Converters;
using WebApi.Services;

namespace WebApi.Converters
{
    public class DateDashSeparatorTimeFormatConverter : IsoDateTimeConverter
    {
        public DateDashSeparatorTimeFormatConverter()
        {
            base.DateTimeFormat = DateTimeService.DefaultFormat;
        }
    }
}