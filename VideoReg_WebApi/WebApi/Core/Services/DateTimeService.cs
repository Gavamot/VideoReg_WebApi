using System;
using System.Globalization;

namespace WebApi.Services
{
    public class DateTimeService : IDateTimeService
    {
        public const string DefaultMsFormat = "yyyy-MM-ddTHH:mm:ss.fff";
        public const string DefaultFormat = "yyyy-MM-ddTHH:mm:ss";

        /// <summary>
        /// Формат архивных файлов .json и .mp4
        /// </summary>
        public const string DateDotSeparatorTimeFormat = "yyyy.MM.ddTHH.mm.ss";
        public const string DateDotSeparatorTimeFormatShortFormat = "yyyy.M.dTH.m.s";

        readonly CultureInfo culture = CultureInfo.InvariantCulture;
        readonly string[] Formats = new string[]
        {
            DefaultFormat,
            DefaultMsFormat,
            DateDotSeparatorTimeFormatShortFormat
        };

        public DateTime Parse(string date)
        {
            foreach (var format in Formats)
            {
                if (DateTime.TryParseExact(date, format, culture, DateTimeStyles.None, out var res))
                    return res;
            }
            throw new ArgumentException($"Cannot parse data[{date}] it has got bad format");
        }

        public DateTime Parse(string date, string preferFormat)
        {
            if (DateTime.TryParseExact(date, preferFormat, culture, DateTimeStyles.None, out var res))
                return res;
            return Parse(date);
        }

        public DateTime GetNow() => DateTime.Now;
        public string NowToStringFull() => ToStringFull(GetNow());
        public string NowToStringFullMs() => ToStringFullMs(GetNow());
        public string ToStringFull(DateTime date) => date.ToString(DefaultFormat);
        public string ToStringFullMs(DateTime date) => date.ToString(DefaultMsFormat);
    }
}
