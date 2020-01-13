using System;
using System.Collections.Generic;
using VideoReg.Domain.Archive.ArchiveFiles;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Domain.Archive.Config;
using VideoReg.Infra.Services;

namespace VideoReg.Domain.Archive
{
    public class VideoArchiveSourceFS : IVideoArchiveSource
    {
        public const string Pattern = "*T*_*_*_*_*_*.mp4";
        private readonly IArchiveConfig config;
        private readonly ILog log;
        private readonly IFileSystemService fileSystem;
        private readonly IBrigadeHistoryRep brigadeHistoryRep;

        readonly ArchiveFileFactory fileFactory;

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
            this.fileFactory = new ArchiveFileFactory(brigadeHistoryRep, config);
        }

        public FileVideoMp4[] GetCompletedVideoFiles(string pattern = Pattern)
        {
            var res = new List<FileVideoMp4>();
            var directories = fileSystem.GetDirectories(config.VideoArchivePath);
            foreach (var camDir in directories)
            {
                if (!TryParseIntFolder(camDir, "camera", out var cameraNumber)) continue;
                var files = GetFilesForAdd(camDir, cameraNumber, pattern);
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

        private List<FileVideoMp4> GetFilesForAdd(string camDir, int cameraNumber, string pattern)
        {
            var res = new List<FileVideoMp4>();
            foreach (var file in fileSystem.GetFiles(camDir, pattern))
            {
                try
                {
                    var vf = fileFactory.CreateVideoMp4(file, cameraNumber);
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
