using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VideoReg.Infra.Services
{
    public class DateTimeService : IDateTimeService
    {
        public const string DefaultMsFormat = "dd.MM.yyyyTHH:mm:ss.fff";
        public const string DefaultFormat = "dd.MM.yyyyTHH:mm:ss";

        /// <summary>
        /// Формат архивных файлов .json и .mp4
        /// </summary>
        public const string ArchiveFileFormat = "yyyy.MM.ddTHH.mm.ss";
        public const string BrigadeHistoryFormat = "yyyy-MM-ddTHH:mm:ss";


        readonly CultureInfo culture = CultureInfo.InvariantCulture;
        readonly string[] Formats = new string[]
        {
            DefaultFormat,
            DefaultMsFormat,
            ArchiveFileFormat,
            BrigadeHistoryFormat,
            "dd.MM.yyyy HH:mm:ss",
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

        public DateTime Now() => DateTime.Now;
        public string NowToStringFull() => ToStringFull(Now());
        public string NowToStringFullMs() => ToStringFullMs(Now());
        public string ToStringFull(DateTime date) => date.ToString(DefaultFormat);
        public string ToStringFullMs(DateTime date) => date.ToString(DefaultMsFormat);
    }
}
