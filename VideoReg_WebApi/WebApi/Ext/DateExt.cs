using System;

namespace WebApi.Ext
{
    public static class DateExt
    {
        public static DateTime RoundToHour(this DateTime self)
        {
            return new DateTime(self.Year, self.Month, self.Day,
                self.Hour, 0, 0, self.Kind);
        }

        public static DateTime RoundToEndHour(this DateTime self)
        {
            return new DateTime(self.Year, self.Month, self.Day,
                self.Hour, 59, 59, self.Kind);
        }

        public static DateTime StartOfDay(this DateTime self)
        {
            return new DateTime(self.Year, self.Month, self.Day);
        }

        public static DateTime EndOfHour(this DateTime self)
        {
            return new DateTime(self.Year, self.Month, self.Day, self.Hour, 59, 59);
        }

        public static DateTime EndOfMinute(this DateTime self)
        {
            return new DateTime(self.Year, self.Month, self.Day, self.Hour, self.Minute, 59);
        }

        public static DateTime StartOfMinute(this DateTime self)
        {
            return new DateTime(self.Year, self.Month, self.Day, self.Hour, self.Minute, 0);
        }

        public static bool IsBetween(this DateTime input, DateTime date1, DateTime date2)
        {
            return (input >= date1 && input <= date2);
        }
    }
}