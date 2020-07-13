using System;
using System.Globalization;
using Newtonsoft.Json;

namespace WebApi.CoreService.Core
{
    public class MultiFormatDateConverter : JsonConverter
    {
        public string[] DateTimeFormats = new[]
        {
            "d.M.yyyyTH:m:s.fff",
            "d.M.yyyyTH:m:s.ff",
            "d.M.yyyyTH:m:s.f",
            "d.M.yyyyTH:m:s.ffff",
            "d.M.yyyyTH:m:s",
            "dd.MM.yyyyTHH:mm:ss.ffff",
            "dd.MM.yyyyTHH:mm:ss"
        };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string dateString = (string)reader.Value;
            DateTime date;
            foreach (string format in DateTimeFormats)
            {
                // adjust this as necessary to fit your needs
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    return date;
            }
            throw new JsonException("Unable to parse \"" + dateString + "\" as a date.");
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}