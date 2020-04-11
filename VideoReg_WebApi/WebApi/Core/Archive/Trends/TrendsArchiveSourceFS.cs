using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;
using WebApi.Ext;
using WebApi.Services;

namespace WebApi.Archive
{
    /// <summary>
    /// Отвечает за получение структуры архива непосредственно с диска
    /// </summary>
    public class TrendsArchiveSourceFS : ITrendsArchiveSource
    {
        public const string Pattern = "*T*_*_*_*_*_*.mp4";
        private readonly IArchiveConfig config;
        private readonly ILog log;
        private readonly IFileSystemService fileSystem;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;

        public TrendsArchiveSourceFS(ILog log, 
            IArchiveConfig config, 
            IBrigadeHistoryRep brigadeHistoryRep,
            IFileSystemService fileSystem
            )
        {
            this.config = config;
            this.log = log;
            this.brigadeHistoryRep = brigadeHistoryRep;
            this.fileSystem = fileSystem;
        }

        public FileTrendsJson[] GetCompletedFiles()
        {
            var brigadeHistory = brigadeHistoryRep.GetBrigadeHistory();
            var fileFactory = ArchiveFileGenerator.Create(brigadeHistory, config);
            var root = config.TrendsArchivePath;
            var files = 
                fileSystem.GetFiles(root, SearchOption.AllDirectories, Pattern)
                    .TrySelect(fileFactory.CreteJson, 
                        (file, e) => 
                            log.Error($"The file {file} has bad name. It must match to patten {Pattern} [{e.Message}]"));
            return files.ToArray();
        }

        private bool TryParseIntFolder(string dir, string dirType, out int res)
        {
            var dirName = fileSystem.GetDirName(dir);
            if (int.TryParse(dirName, out res))
                return true;
            log.Error($"The directory {dir} must be {dirType}-directory. It must have the int type.");
            return false;
        }

    }
}
