using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Core.Archive
{
    public interface ITrendsArchiveStructureStore
    {
        public void Set(FileChannelJson[] value);
        public FileChannelJson[] GetAll();
    }
}
