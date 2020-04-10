using System;
using Newtonsoft.Json;
using WebApi.Converters;

namespace WebApi.Dto
{
    public class FileVideoMp4Dto
    {
        public int Camera { get; set; }
        public int? Brigade { get; set; }

        [JsonConverter(typeof(DateDashSeparatorTimeFormatConverter))]
        public DateTime Pdt { get; set; }
        public int Duration { get; set; }
    }
}