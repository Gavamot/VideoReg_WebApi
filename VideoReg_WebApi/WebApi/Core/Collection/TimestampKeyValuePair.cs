using System;
using System.Collections.Generic;

namespace WebApi.Collection
{
    public class TimestampKeyValuePair<TKey, TValue> 
    {
        public DateTime Timestamp { get; internal set; }
        public TKey Key { get; internal set; }
        public TValue Value { get; internal set; }

        public TimestampValue<TValue> TimestampValue => new TimestampValue<TValue>(Timestamp, Value);
        public TimestampKeyValuePair() { }

        public TimestampKeyValuePair(DateTime timestamp, TKey key, TValue value)
        {
            this.Timestamp = timestamp;
            this.Key = key;
            this.Value = value;
        }

    

        protected bool Equals(TimestampKeyValuePair<TKey, TValue> other)
        {
            return Timestamp.Equals(other.Timestamp) && EqualityComparer<TKey>.Default.Equals(Key, other.Key) && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TimestampKeyValuePair<TKey, TValue>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Timestamp, Key, Value);
        }
    }
}
