using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;

namespace WebApi.Core.Archive
{
    public class VideoArchiveStructureStore : IVideoArchiveStructureStore
    {
        volatile Dictionary<int, FileVideoMp4[]> storeByCamera = new Dictionary<int, FileVideoMp4[]>();
        volatile FileVideoMp4[] storeAll = new FileVideoMp4[0];

        public void Set(FileVideoMp4[] value)
        {
            storeAll = value;
            storeByCamera = CreateStoreByCamera(value);
        }

        public FileVideoMp4[] GetAll()
        {
            return storeAll;
        }

        public FileVideoMp4[] GetByCameraNumber(int camera)
        {
            if (storeByCamera.TryGetValue(camera, out var res))
            {
                return res;
            }
            return new FileVideoMp4[0];
        }

        private Dictionary<int, FileVideoMp4[]> CreateStoreByCamera(FileVideoMp4[] value)
        {
            return value.GroupBy(x => x.cameraNumber)
                .ToDictionary(gdc => gdc.Key, gdc => gdc.ToArray());
        }
    }
}
