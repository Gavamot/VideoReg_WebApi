using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Core.Archive
{
    public class ArchiveFileData
    {
        public ArchiveFile File { get; set; }
        public byte[] Data { get; set; }
    }
}
