namespace WebApi.Core.Configuration
{
    public class StartupConfig
    {
        public bool OnlineVideo { get; set; }
        public bool OnlineTrends { get; set; }
        public bool ArchiveTrends { get; set; }
        public bool ArchiveVideo { get; set; }
        public bool TransmitToAsc { get; set; }
    }
}
