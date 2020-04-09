using System;
using System.Collections.Generic;

namespace WebApi.Collection
{
    public class TimestampValue<T>
    {
        public DateTime Timestamp { get; set; }
        public T Value { get; set; }

        public TimestampValue() { }

        public TimestampValue(DateTime timestamp, T value)
        {
            Timestamp = timestamp;
            Value = value;
        }

        protected bool Equals(TimestampValue<T> other)
        {
            return Timestamp.Equals(other.Timestamp) 
                && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TimestampValue<T>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Timestamp, Value);
        }
    }
}
