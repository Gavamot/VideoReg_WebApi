namespace WebApi.Configuration
{
    public interface IVideoTransmitterConfig
    {
        string AscRegServiceEndpoint { get; set; }
        string AscRegServiceBufferSize { get; set; }
    }
}