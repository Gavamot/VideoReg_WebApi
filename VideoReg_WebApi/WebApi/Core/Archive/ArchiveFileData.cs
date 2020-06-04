using WebApi.Archive.ArchiveFiles;

namespace WebApi.Core.Archive
{
    public class ArchiveFileData
    {
        public ArchiveFile File { get; set; }
        public byte[] Data { get; set; }
        public string FileName => File?.FileName;

        public static ArchiveFileData EmptyFile => new ArchiveFileData()
        {
            Data = new byte[0],
            File = FileTrendsJson.EmptyFile,
        };
    }
}
