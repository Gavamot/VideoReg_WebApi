using System;
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

namespace WebApi.Archive
{
    public class TrendsArchiveUpCacheRep : ITrendsArchiveRep, IUpdatedCache
    {
        private readonly IArchiveConfig config;
        private readonly IFileSystemService fs;
        private readonly IMemoryCache cache;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;
        private readonly ILog log;

        static readonly object updateTaskLock = new object();
        static Task updateTask = null;

        public TrendsArchiveUpCacheRep(IMemoryCache cache,
            IBrigadeHistoryRep brigadeHistoryRep,
            IFileSystemService fs, 
            IArchiveConfig config,
            ILog log)
        {
            this.brigadeHistoryRep = brigadeHistoryRep;
            this.cache = cache;
            this.config = config;
            this.fs = fs;
            this.log = log;
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

        private FileTrendsJson[] GetCache()
        {
            if (cache.TryGetValue(CacheKeys.TrendsArchive, out object value))
            {
                return (FileTrendsJson[]) value;
            }
            return new FileTrendsJson[0];
        }

        public async Task<byte[]> GetTrendFileAsync(DateTime pdt)
        {
            var file = GetCache().FirstOrDefault(x => x.pdt == pdt);
            if (file == default)
                return null;
            string filePath = Path.Combine(config.TrendsArchivePath, file.fullArchiveName);
            return await fs.ReadFileAsync(filePath);
        }

        public FileTrendsJson[] GetFullStructure(DateTime startWith)
        {
            var res = GetCache();
            if (startWith == default)
                return res;
            return res.Where(x => x.pdt >= startWith).ToArray();
        }

        FileTrendsJson[] GetCompletedFiles()
        {
            var brigadeHistory = brigadeHistoryRep.GetBrigadeHistory();
            var fileFactory = ArchiveFileGenerator.Create(brigadeHistory, config);
            var root = config.TrendsArchivePath;
            const string pattern = "*T*_*_*_*_*_*.mp4";
            var files =
                fs.GetFiles(root, SearchOption.AllDirectories, pattern)
                    .TrySelect(fileFactory.CreteJson,
                        (file, e) =>
                            log.Error($"The file {file} has bad name. It must match to patten {pattern} [{e.Message}]"));
            return files.ToArray();
        }
    }
}
