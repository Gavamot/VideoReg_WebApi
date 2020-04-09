namespace WebApi.Configuration
{
    public interface IImagePollingConfig
    {
        int ImagePollingAttempts { get; }
        int ImagePollingDelayMs { get; }
    }
}
