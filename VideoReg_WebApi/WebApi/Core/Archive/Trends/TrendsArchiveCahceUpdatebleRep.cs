using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;
using WebApi.Core;
using WebApi.Core.Archive;
using WebApi.Ext;
using WebApi.Services;

namespace WebApi.Archive
{
    public class TrendsArchiveCahceUpdatebleRep : ITrendsArchiveRep
    {
        private readonly IArchiveConfig config;
        private readonly IFileSystemService fs;
        private readonly IMemoryCache cache;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;
        private readonly ILogger<TrendsArchiveCahceUpdatebleRep> log;
        private const string TrendsArchive = nameof(TrendsArchive);

        public TrendsArchiveCahceUpdatebleRep(IMemoryCache cache,
            IBrigadeHistoryRep brigadeHistoryRep,
            IFileSystemService fs, 
            IArchiveConfig config,
            ILogger<TrendsArchiveCahceUpdatebleRep> log)
        {
            this.brigadeHistoryRep = brigadeHistoryRep;
            this.cache = cache;
            this.config = config;
            this.fs = fs;
            this.log = log;
            BeginUpdate();
        }

        /// <summary>
        ///  Don't call me often
        /// </summary>
        public void BeginUpdate()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        var files = GetCompletedFiles();
                        cache.Set(TrendsArchive, files);
                    }
                    catch (Exception e)
                    {
                        log.LogError($"Can not update cache in TrendsArchiveCahceUpdatebleRep. [{e.Message}]");
                    }
                    await Task.Delay(config.TrendsArchiveUpdateTimeMs);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private FileTrendsJson[] GetCache()
        {
            if (cache.TryGetValue(TrendsArchive, out object value))
            {
                return (FileTrendsJson[]) value;
            }
            return new FileTrendsJson[0];
        }

        private async Task<ArchiveFileData> GetFileAsync(Func<FileTrendsJson, bool> selector)
        {
            var file = GetCache().FirstOrDefault(selector);
            if (file == default)
                return null;
            string filePath = Path.Combine(config.TrendsArchivePath, file.fullArchiveName);
            try
            {
                var data = await fs.ReadFileAsync(filePath);
                return new ArchiveFileData()
                {
                    File = file,
                    Data = data
                };
            }
            catch (IOException e)
            {
                log.LogWarning(e.Message);
                return null;
            }
        }

        public async Task<ArchiveFileData> GetNearestFrontTrendFileAsync(DateTime pdt)
        {
            return await GetFileAsync(x => x.pdt >= pdt);
        }

        public async Task<ArchiveFileData> GetTrendFileAsync(DateTime pdt)
        {
            return await GetFileAsync(x => x.pdt == pdt);
        }


        public FileTrendsJson[] GetFullStructure(DateTime startWith)
        {
            var res = GetCache();
            if (startWith == default)
                return res;
            return res.Where(x => x.pdt >= startWith)
                .OrderBy(x => x.pdt).ToArray();
        }

        public FileTrendsJson[] GetFullStructureByInterval(DateTime start, DateTime end)
        {
            var res = GetCache();
            return res.Where(x => x.pdt >= start && x.pdt <= end)
                .OrderBy(x=>x.pdt).ToArray();
        }

        FileTrendsJson[] GetCompletedFiles()
        {
            var brigadeHistory = brigadeHistoryRep.GetBrigadeHistory();
            var fileFactory = ArchiveFileGenerator.Create(brigadeHistory, config);
            var root = config.TrendsArchivePath;
            const string pattern = "*T*_*_*_*_*.json";
            var files = fs.GetFiles(root, SearchOption.AllDirectories, pattern);
            var res = files
                    .TrySelect(fileFactory.CreteJson,
                        (file, e) =>log.LogError($"The file {file} has bad name. It must match to patten {pattern} [{e.Message}]"))
                   .OrderBy(x => x.pdt)
                   .ToArray();
            return res;
        }
    }
}
