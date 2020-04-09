namespace WebApi.Configuration
{
    public interface ICameraConfig
    {
        string UserName { get; }
        string Password { get; }
        int CameraUpdateSleepIfErrorTimeoutMs { get; }
        int CameraUpdateSleepIfAuthorizeErrorTimeoutMs { get; }
        int CameraUpdateIntervalMs { get; }
        int CameraGetImageTimeoutMs { get; }
        string Redis { get; }
    }
}
