using System;
using System.Buffers.Text;
using System.Text.Json.Serialization;

namespace WebApi.Core.OnlineVideo
{
    public class Snapshot
    {
        [JsonPropertyName("timestamp")]
        public  DateTime Timestamp { get; set; }
        [JsonPropertyName("image")]
        public string Image { get; set; }
        public byte[] GetImage() => Convert.FromBase64String(Image);
    }
}