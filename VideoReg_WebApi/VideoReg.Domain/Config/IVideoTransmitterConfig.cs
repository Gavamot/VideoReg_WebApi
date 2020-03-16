namespace VideoReg.Domain.Archive.Config
{
    public interface IVideoTransmitterConfig
    {
        string AscRegServiceEndpoint { get; set; }
        string AscRegServiceBufferSize { get; set; }
    }
}