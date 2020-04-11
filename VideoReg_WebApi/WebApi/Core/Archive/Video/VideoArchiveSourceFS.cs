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
    public class VideoArchiveSourceFS : IVideoArchiveSource
    {
        public const string Pattern = "*T*_*_*_*_*_*.mp4";
        private readonly IArchiveConfig config;
        private readonly ILog log;
        private readonly IFileSystemService fileSystem;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;

        public VideoArchiveSourceFS(ILog log, 
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

        public FileVideoMp4[] GetCompletedFiles()
        {
            var brigadeHistory = brigadeHistoryRep.GetBrigadeHistory();
            var fileFactory = ArchiveFileGenerator.Create(brigadeHistory, config);
            var res = new List<FileVideoMp4>();
            var directories = fileSystem.GetDirectories(config.VideoArchivePath);
            foreach (var camDir in directories)
            {
                if (!TryParseIntFolder(camDir, "camera", out var cameraNumber)) continue;
                var files = GetFilesForAdd(fileFactory, camDir, cameraNumber, Pattern);
                res.AddRange(files);
            }
            return res.ToArray();
        }

        private bool TryParseIntFolder(string dir, string dirType, out int res)
        {
            var dirName = fileSystem.GetDirName(dir);
            if (int.TryParse(dirName, out res))
                return true;
            log.Error($"The directory {dir} must be {dirType}-directory. It must have the int type.");
            return false;
        }

        private IEnumerable<FileVideoMp4> GetFilesForAdd(ArchiveFileGenerator fileGenerator, string camDir, int cameraNumber, string pattern)
        {
            string cameraNumberStr = cameraNumber.ToString();
            var files = fileSystem.GetFiles(camDir, SearchOption.AllDirectories, pattern)
                .Select(x => x.Replace(camDir, cameraNumberStr))
                .TrySelect(file=> fileGenerator.CreateVideoMp4(file, cameraNumber), (file, e) =>
                {
                    log.Error($"The file {file} has bad name. It must match to patten {pattern} [{e.Message}]");
                })
                .Where(x=> x.IsComplete);
            return files;
        }
    }
}
