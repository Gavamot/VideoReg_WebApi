using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Services;
using WebApi.ValueType;

namespace WebApi.Archive.BrigadeHistory
{
    public class BrigadeHistory : IBrigadeHistory
    {
        class BrigadeDateInterval
        {
            public BrigadeDateInterval() { }

            public BrigadeDateInterval(int brigadeCode, DateTime start, DateTime end)
            {
                this.BrigadeCode = brigadeCode;
                this.Interval = new Interval(start, end);
            }
            public Interval Interval { get; set; }
            public int BrigadeCode { get; set; }

            public override string ToString()
            {
                return $"brigadeCode={BrigadeCode} [{Interval}]";
            }
        }

        private readonly ILog log;
        private readonly BrigadeDateInterval[] history = new BrigadeDateInterval[0];
        private readonly IDateTimeService dateTimeService;
        public const int EmptyBrigadeCode = -1;
        public BrigadeHistory(ILog log, IDateTimeService dateTimeService)
        {
            this.log = log;
            this.dateTimeService = dateTimeService;
        }

        public BrigadeHistory(string fileText, IDateTimeService dateTimeService, ILog log) : this(log, dateTimeService)
        {
            try
            {
                history = CreateFromText(fileText).ToArray();
            }
            catch(Exception e)
            {
                log.Error($"Error BrigadeHistory parse. (BrigadeHistory.CreateFromText).", e);
            }
        }

        private List<BrigadeDateInterval> CreateFromText(string fileText)
        {
            var res = ParseTextFile(fileText);
            //List<BrigadeDateInterval> PrepareIntervals(List<BrigadeDateInterval> list)
            //{
            //    if (!list.Any()) return list.ToList();
            //    var firstRow = list.First();
            //    firstRow.Interval = firstRow.Interval.SetStart(DateTime.MinValue);
            //    var lastRow = list.Last();
            //    lastRow.Interval = lastRow.Interval.SetEnd(DateTime.MaxValue);
            //    return list.ToList();
            //}
            //res = PrepareIntervals(res);
            return res;
        }

        private DateTime ParseVideoRegDateTime(string v)
        {
            if (v == "NULL") 
                return DateTime.MaxValue;
            return dateTimeService.Parse(v);
        }

        private BrigadeDateInterval ParseLine(string line)
        {
            try
            {
                string[] values = line.Split(' ');
                DateTime start = ParseVideoRegDateTime(values[0]);
                DateTime end = ParseVideoRegDateTime(values[1]);
                var interval = new Interval(start, end);
                int brigadeCode = int.Parse(values[2]);
                var brigadeDateInterval = new BrigadeDateInterval
                {
                    Interval = interval,
                    BrigadeCode = brigadeCode
                };
                return brigadeDateInterval;
            }
            catch (Exception e)
            {
                log.Error($"{nameof(BrigadeHistory)} - string[{line}] has the bad format. [{e.Message}]");
                return default;
            }
        }

        private List<BrigadeDateInterval> ParseTextFile(string fileText)
        {
            // Каждая строка файла содержит дату начала и дату конца в формате yyyy-MM-ddTHH:mm:ss,  
            // код бригады - все эти значения разделенны пробелом
            // Формат [start end brigadeCode] 
            // Если end имеем значение NULL - значит в настоящие время установленна 
            string[] lines = fileText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var res = new List<BrigadeDateInterval>();
            foreach (var line in lines)
            {
                var brigadeDateInterval = ParseLine(line);
                if(brigadeDateInterval == default) continue;
                res.Add(brigadeDateInterval);
                if (brigadeDateInterval.Interval.End == DateTime.MaxValue)
                    break;
            }
            return res;
        }

        public int GetBrigadeCode(DateTime pdt)
        {
            var res = history.FirstOrDefault(x => x.Interval.IsInInterval(pdt))?.BrigadeCode;
            return res ?? EmptyBrigadeCode;
        }

        public bool IsEmptyCode(int brigade)
        {
            return brigade == EmptyBrigadeCode;
        }
    }
}
