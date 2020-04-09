using System;

namespace WebApi.Core.Dto
{
    public class FileVideoMp4Dto
    {
        public int Camera { get; set; }
        public int? Brigade { get; set; }
        public DateTime Pdt { get; set; }
        public int Duration { get; set; }
    }
}