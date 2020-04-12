using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;
using WebApi.Core;
using WebApi.Ext;
using WebApi.Services;
using WebApi.ValueType;

namespace WebApi.Archive
{
    /// <summary>
    /// 
    /// </summary>
    public class VideoArchiveRep : IVideoArchiveRep, IUpdatedCache
    {
        private readonly IMemoryCache cache;
        private readonly IArchiveConfig config;
        private readonly IFileSystemService fs;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;
        private readonly ILog log;

        static readonly object updateTaskLock = new object();
        static Task updateTask = null;

        public VideoArchiveRep(IMemoryCache cache,
            IFileSystemService fs,
            IArchiveConfig config,
            IBrigadeHistoryRep brigadeHistoryRep,
            ILog log)
        {
            this.cache = cache;
            this.fs = fs;
            this.config = config;
            this.brigadeHistoryRep = brigadeHistoryRep;
            this.log = log;
        }

        public async Task<byte[]> TryGetVideoFileAsync(DateTime pdt, int camera)
        {
            var file = GetCache().FirstOrDefault(x => x.cameraNumber == camera && x.pdt == pdt);
            if (file == default)
                return null;
            string filePath = Path.Combine(config.VideoArchivePath, file.fullArchiveName);
            var stream = await fs.ReadFileAsync(filePath);
            return stream;
        }

        public FileVideoMp4[] GetFullStructure(DateTime startWith = default)
        {
            var res = GetCache();
            if (startWith == default)
                return res;
            return res.Where(x => x.pdt >= startWith).ToArray();
        }

        public FileVideoMp4[] GetStructureByCameraNumber(int cameraNumber, DateTime startWith = default)
        {
            var res = GetCache().Where(x=>x.cameraNumber == cameraNumber);
            if (startWith == default)
                return res.ToArray();
            return res.Where(x => x.pdt >= startWith).ToArray();
        }

        public void BeginUpdate()
        {
            lock (updateTaskLock)
            {
                if (updateTask == null)
                {
                    new Task(async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                var files = GetCompletedFiles();
                                cache.Set(CacheKeys.TrendsArchive, files);
                            }
                            catch (Exception e)
                            {
                                log.Error($"Can not update cache in TrendsArchiveUpCacheRep. [{e.Message}]", e);
                            }
                            await Task.Delay(config.TrendsArchiveUpdateTimeMs);
                        }
                    }, TaskCreationOptions.LongRunning).Start();
                }
            }
        }

        private FileVideoMp4[] GetCache()
        {
            if (cache.TryGetValue(CacheKeys.TrendsArchive, out object value))
            {
                return (FileVideoMp4[])value;
            }
            return new FileVideoMp4[0];
        }

        private FileVideoMp4[] GetCompletedFiles()
        {
            const string pattern = "*T*_*_*_*_*_*.mp4";
            var brigadeHistory = brigadeHistoryRep.GetBrigadeHistory();
            var fileFactory = ArchiveFileGenerator.Create(brigadeHistory, config);
            var res = new List<FileVideoMp4>();
            var directories = fs.GetDirectories(config.VideoArchivePath);
            foreach (var camDir in directories)
            {
                if (!TryParseIntFolder(camDir, "camera", out var cameraNumber)) continue;
                var files = GetFilesForAdd(fileFactory, camDir, cameraNumber, pattern);
                res.AddRange(files);
            }
            return res.ToArray();
        }

        private bool TryParseIntFolder(string dir, string dirType, out int res)
        {
            var dirName = fs.GetDirName(dir);
            if (int.TryParse(dirName, out res))
                return true;
            log.Error($"The directory {dir} must be {dirType}-directory. It must have the int type.");
            return false;
        }

        private IEnumerable<FileVideoMp4> GetFilesForAdd(ArchiveFileGenerator fileGenerator, string camDir, int cameraNumber, string pattern)
        {
            string cameraNumberStr = cameraNumber.ToString();
            var files = fs.GetFiles(camDir, SearchOption.AllDirectories, pattern)
                .Select(x => x.Replace(camDir, cameraNumberStr))
                .TrySelect(file => fileGenerator.CreateVideoMp4(file, cameraNumber), (file, e) =>
                {
                    log.Error($"The file {file} has bad name. It must match to patten {pattern} [{e.Message}]");
                })
                .Where(x => x.IsComplete);
            return files;
        }
    }
}
