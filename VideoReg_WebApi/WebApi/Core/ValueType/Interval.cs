using System;

namespace WebApi.ValueType
{
    public struct Interval
    {
        public static readonly Interval ErrInterval = new Interval(DateTime.MinValue, DateTime.MinValue);
        public static readonly Interval EmptyInterval = new Interval(DateTime.MinValue, DateTime.MinValue);
        public static readonly Interval FullInterval = new Interval(DateTime.MinValue, DateTime.MaxValue);

        public readonly DateTime Start;
        public readonly DateTime End;

        public Interval(DateTime start, DateTime end)
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

        public static bool operator ==(Interval interval1, Interval interval2) =>
            interval1.Equals(interval2);

        public static bool operator !=(Interval interval1, Interval interval2)
            => !interval1.Equals(interval2);

        public Interval SetEnd(DateTime end) => new Interval(Start, end);

        public Interval SetStart(DateTime start) => new Interval(start, End);

        public static Interval operator +(Interval interval, TimeSpan timeSpan) =>
            interval.SetEnd(interval.End + timeSpan);

        public static Interval operator -(Interval interval, TimeSpan timeSpan) =>
            interval.SetEnd(interval.End - timeSpan);
        public override string ToString() => $"{Start:dd-MM-yyyyTHH:mm:ss} - {End:dd-MM-yyyyTHH:mm:ss}";

        public override bool Equals(object obj)
        {
            if (!(obj is Interval))
            {
                return false;
            }

            var interval = (Interval)obj;
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