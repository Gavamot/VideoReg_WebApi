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
        volatile FileTrendsJson[] storeAll = new FileTrendsJson[0];

        public void Set(FileTrendsJson[] value)
        {
            storeAll = value;
        }

        public FileTrendsJson[] GetAll()
        {
            return storeAll;
        }
    }
}
