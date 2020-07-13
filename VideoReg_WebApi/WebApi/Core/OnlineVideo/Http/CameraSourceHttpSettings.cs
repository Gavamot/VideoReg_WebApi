namespace WebApi.OnlineVideo
{
    public struct CameraSourceHttpSettings
    {
        public CameraSourceHttpSettings(int number, string snapshotUrl)
        {
            this.number = number;
            this.snapshotUrl = snapshotUrl;
        }
        
        public readonly int number;
        public readonly string snapshotUrl;
        public override string ToString() => $"CameraNumber={number} URL={snapshotUrl}";
    }
}
