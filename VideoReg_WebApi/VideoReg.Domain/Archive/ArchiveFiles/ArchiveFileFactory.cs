using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.ValueType;

namespace VideoReg.Domain.Archive.ArchiveFiles
{
    public class ArchiveFileFactory : IArchiveFileFactory
    {
        protected class ArchiveFileData
        {
            public int? brigade;
            public DateTime pdt;
            public DeviceSerialNumber serialNumber;
            public string fullArchiveName;
            public string[] filePats;
        }

        
        public const string FileNameDateFormat = "yyyy.M.ddTHH.mm.ss";
        public const string EmptyDevice = "N";

        private readonly IBrigadeHistoryRep brigadeHistoryRep;
        private readonly IArchiveConfig config;

        public ArchiveFileFactory(IBrigadeHistoryRep brigadeHistoryRep, IArchiveConfig config)
        {
            this.brigadeHistoryRep = brigadeHistoryRep;
            this.config = config;
        }

        public FileChannelJson CreteJson(string fileFullName)
        {
            var data = GetArchiveFileData(fileFullName, config.ChannelArchivePath, FileChannelJson.Extension);
            return new FileChannelJson(data.brigade, data.pdt, data.serialNumber, data.fullArchiveName);
        }

        public FileVideoMp4 CreateVideoMp4(string fileFullName, int cameraNumber)
        {
            var data = GetArchiveFileData(fileFullName, config.VideoArchivePath, FileVideoMp4.Extension);
            int? duration = ParseDuration(data.filePats);
            return new FileVideoMp4(data.brigade, data.pdt, data.serialNumber, data.fullArchiveName, cameraNumber, duration);
        }

        protected void CheckFileExtension(string file, string ext)
        {
            var fe = Path.GetExtension(file);
            if (fe != ext)
                throw new FormatException($"file[{file}] haves ext={fe} but must haves ext={ext}");
        }

        protected ArchiveFileData GetArchiveFileData(string fileFullName, string acrFolder, string ext)
        {
            CheckFileExtension(fileFullName, ext);
            var filePats = GetFilePats(fileFullName);
            var pdt = ParsePdt(filePats);
            var brigadeHistory = brigadeHistoryRep.GetBrigadeHistory();
            var brigade = brigadeHistory.GetBrigadeCode(pdt);
            var serialNumber = ParseSerialNumber(filePats);
            var arcFullName = GetFullArcName(fileFullName, acrFolder);
            var res = new ArchiveFileData
            {
                pdt = pdt,
                brigade = brigade,
                fullArchiveName = arcFullName,
                serialNumber = serialNumber,
                filePats = filePats
            };
            return res;
        }

        protected DateTime ParsePdt(IEnumerable<string> filesPats)
        {
            var ptd = filesPats.First();
            return DateTime.ParseExact(ptd, FileNameDateFormat, CultureInfo.CurrentCulture);
        }

        protected DeviceSerialNumber ParseSerialNumber(IEnumerable<string> fileNamePats)
        {
            fileNamePats = fileNamePats.Skip(1) // DateTime
                .Take(DeviceSerialNumber.ArgsCount)
                .ToArray();
           
            if (fileNamePats.Contains(EmptyDevice))
                return default;
            var args = fileNamePats.Select(ushort.Parse);
            return new DeviceSerialNumber(args);
        }

        protected int? ParseDuration(IEnumerable<string> fileNamePats)
        {
            string dur = fileNamePats.Skip(5).FirstOrDefault();
            if (string.IsNullOrEmpty(dur))
                return default;
            return int.Parse(dur);
        }

        protected string GetFullArcName(string fileFullName, string acrFolder)
        {
            if (!acrFolder.EndsWith(Path.DirectorySeparatorChar))
                acrFolder += Path.DirectorySeparatorChar;
            var arcFullName = fileFullName.Replace(acrFolder + Path.DirectorySeparatorChar, string.Empty);
            return arcFullName;
        }

        protected string[] GetFilePats(string file)
        {
            // Pat(часть) означают части имени файла разделенные символом _
            // 0 - дата время
            // 1-4 серийный номер устройства
            const int minCountOfFileNamePats = 5;
            var fileName = Path.GetFileNameWithoutExtension(file);
            var filePats = fileName.Split(ArchiveFile.PatsSeparator);
            if (filePats.Length < minCountOfFileNamePats)
                throw new FormatException($"file({fileName}) has bad format");
            return filePats;
        }
    }
}