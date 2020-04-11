using System;
using Newtonsoft.Json;
using WebApi.Converters;

namespace WebApi.Dto
{
    public class FileTrendsDto
    {
        public int? Brigade { get; set; }

        [JsonConverter(typeof(DateDashSeparatorTimeFormatConverter))]
        public DateTime Pdt { get; set; }
    }
}