using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Core.Archive
{
    public class TrendsArchiveStructureStore : ITrendsArchiveStructureStore
    {
        volatile FileChannelJson[] storeAll = new FileChannelJson[0];

        public void Set(FileChannelJson[] value)
        {
            storeAll = value;
        }

        public FileChannelJson[] GetAll()
        {
            return storeAll;
        }
    }
}
