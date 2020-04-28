namespace WebApi.Configuration
{
    public interface IVideoTransmitterConfig
    {
        string SetImageUrl { get; set; }
        string AscRegServiceEndpoint { get; set; }
        string AscRegServiceBufferSize { get; set; }
    }
}