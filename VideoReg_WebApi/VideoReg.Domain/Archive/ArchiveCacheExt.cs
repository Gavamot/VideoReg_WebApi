using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using VideoReg.Domain.Archive.ArchiveFiles;

namespace VideoReg.Domain.Archive
{
    public static class ArchiveCacheExt
    {
        public const string VideoArcCacheKey = nameof(VideoArcCacheKey);

        public static void SetVideoArchiveCache(this IMemoryCache cache, FileVideoMp4[] newCache)
        {
            cache.Set(VideoArcCacheKey, newCache);
        }

        public static FileVideoMp4[] GetVideoArchiveCache(this IMemoryCache cache)
        {
            return cache.Get<FileVideoMp4[]>(VideoArcCacheKey) ?? new FileVideoMp4[0];
        }
    }
}
