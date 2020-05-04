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
using WebApi.Core.Archive;
using WebApi.Ext;
using WebApi.Services;

namespace WebApi.Archive
{
    /// <summary>
    /// 
    /// </summary>
    public class CameraArchiveRep : ICameraArchiveRep, IUpdatedCache
    {
        private readonly IMemoryCache cache;
        private readonly IArchiveConfig config;
        private readonly IFileSystemService fs;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;
        private readonly ILog log;

        static readonly object updateTaskLock = new object();
        static Task updateTask = null;

        public CameraArchiveRep(IMemoryCache cache,
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

        private async Task<ArchiveFileData> TryGetFileAsync(Func<FileVideoMp4, bool> selector)
        {
            var files = GetCache();
            var file = files.FirstOrDefault(selector);
            if (file == default)
                return null;
            string filePath = GetFullArchiveFileName(file);
            var data = await fs.ReadFileAsync(filePath);
            return new ArchiveFileData
            {
                File = file,
                Data = data
            };
        }

        public async Task<ArchiveFileData> GetNearestFrontVideoFileAsync(DateTime pdt, int camera)
        {
            return await TryGetFileAsync(x => x.cameraNumber == camera && x.pdt >= pdt);
        }

        public async Task<ArchiveFileData> TryGetVideoFileAsync(DateTime pdt, int camera)
        {
            return await TryGetFileAsync(x => x.cameraNumber == camera && x.pdt == pdt);
        }

        public FileVideoMp4[] GetFullStructure(DateTime startWith = default)
        {
            var res = GetCache();
            if (startWith == default)
                return res;
            return res.Where(x => x.pdt >= startWith).OrderBy(x => x.pdt).ToArray();
        }

        public FileVideoMp4[] GetStructureByCameraNumber(int cameraNumber, DateTime startWith = default)
        {
            var res = GetCache().Where(x=>x.cameraNumber == cameraNumber);
            if (startWith == default)
                return res.ToArray();
            return res.Where(x => x.pdt >= startWith)
                .OrderBy(x => x.pdt).ToArray();
        }

        public FileVideoMp4[] GetFullStructureByCameraNumberAndInterval(int cameraNumber, DateTime start, DateTime end)
        {
            var res = GetCache();
            return res.Where(x => 
                x.pdt >= start && x.pdt <= end 
                               && x.cameraNumber == cameraNumber)
                .OrderBy(x => x.pdt).ToArray();
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
                                cache.Set(CacheKeys.VideoArchive, files);
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
            if (cache.TryGetValue(CacheKeys.VideoArchive, out object value))
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
                .Where(x => x.IsComplete)
                .OrderBy(x=>x.pdt);
            return files;
        }

        public string GetFullArchiveFileName(ArchiveFile file)
        {
            return Path.GetFullPath(Path.Combine(config.VideoArchivePath, file.fullArchiveName));
        }

        public bool TryGetVideoFilInfo(DateTime pdt, int camera, out ArchiveFile file)
        {
            file = GetCache().FirstOrDefault(x => x.pdt == pdt && x.cameraNumber == camera);
            return file != default;
        }
    }
}
