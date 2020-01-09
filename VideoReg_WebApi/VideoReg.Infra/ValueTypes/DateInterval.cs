using System;

namespace VideoReg.Infra.ValueTypes
{
    public struct DateInterval
    {
        public static readonly DateInterval ErrInterval = new DateInterval(DateTime.MinValue, DateTime.MinValue);
        public static readonly DateInterval EmptyInterval = new DateInterval(DateTime.MinValue, DateTime.MinValue);
        public static readonly DateInterval FullInterval = new DateInterval(DateTime.MinValue, DateTime.MaxValue);

        public readonly DateTime Start;
        public readonly DateTime End;

        public DateInterval(DateTime start, DateTime end)
        {
            if (start > end)
            {
                Start = ErrInterval.Start;
                End = ErrInterval.End;
            }
            else
            {
                Start = start;
                End = end;
            }
        }

        public int TotalMinutes => (int)(End - Start).TotalMinutes;
        public bool IsErrInterval => this == ErrInterval;
        public bool IsEmptyInterval => this == EmptyInterval;
        public bool IsFullInterval => this == FullInterval;
        public bool IsInInterval(DateTime dt) => Start <= dt && End >= dt;

        public static bool operator ==(DateInterval interval1, DateInterval interval2) =>
            interval1.Equals(interval2);

        public static bool operator !=(DateInterval interval1, DateInterval interval2)
            => !interval1.Equals(interval2);

        public DateInterval SetEnd(DateTime end) => new DateInterval(Start, end);

        public DateInterval SetStart(DateTime start) => new DateInterval(start, End);

        public static DateInterval operator +(DateInterval interval, TimeSpan timeSpan) =>
            interval.SetEnd(interval.End + timeSpan);

        public static DateInterval operator -(DateInterval interval, TimeSpan timeSpan) =>
            interval.SetEnd(interval.End - timeSpan);
        public override string ToString() => $"{Start:dd-MM-yyyyTHH:mm:ss} - {End:dd-MM-yyyyTHH:mm:ss}";

        public override bool Equals(object obj)
        {
            if (!(obj is DateInterval))
            {
                return false;
            }

            var interval = (DateInterval)obj;
            return Start == interval.Start &&
                   End == interval.End;
        }

        public override int GetHashCode()
        {
            var hashCode = -1676728671;
            hashCode = hashCode * -1521134295 + Start.GetHashCode();
            hashCode = hashCode * -1521134295 + End.GetHashCode();
            return hashCode;
        }
    }
}