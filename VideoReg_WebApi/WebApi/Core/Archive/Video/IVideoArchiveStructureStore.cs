using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Core.Archive
{
    public interface IVideoArchiveStructureStore
    {
        public void Set(FileVideoMp4[] value);
        public FileVideoMp4[] GetAll();
        public FileVideoMp4[] GetByCameraNumber(int camera);
    }
}
