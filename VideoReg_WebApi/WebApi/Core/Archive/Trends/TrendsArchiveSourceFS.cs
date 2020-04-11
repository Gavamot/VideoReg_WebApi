﻿using System;
using System.Collections.Generic;
using System.Linq;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;
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

        public FileTrendsJson[] GetCompletedFiles(string pattern = Pattern)
        {
            var brigadeHistory = brigadeHistoryRep.GetBrigadeHistory();
            var fileFactory = ArchiveFileGenerator.Create(brigadeHistory, config);
            var res = new List<FileVideoMp4>();
            var directories = fileSystem.GetDirectories(config.VideoArchivePath);
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
            var dirName = fileSystem.GetDirName(dir);
            if (int.TryParse(dirName, out res))
                return true;
            log.Error($"The directory {dir} must be {dirType}-directory. It must have the int type.");
            return false;
        }

        private List<FileVideoMp4> GetFilesForAdd(ArchiveFileGenerator fileGenerator, string camDir, int cameraNumber, string pattern)
        {
            var res = new List<FileVideoMp4>();
            string cameraNumberStr = cameraNumber.ToString();
            var files = fileSystem.GetFiles(camDir, pattern)
                .Select(x => x.Replace(camDir, cameraNumberStr))
                .ToArray();

            foreach (var file in files)
            {
                try
                {
                    var vf = fileGenerator.CreateVideoMp4(file, cameraNumber);
                    if (vf.IsComplete)
                        res.Add(vf);
                }
                catch (Exception e)
                {
                    log.Error($"The file {file} has bad name. It must match to patten {pattern} [{e.Message}]");
                }
            }
            return res;
        }
    }
}
