using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VideoReg.Domain.ValueType;

namespace VideoReg.Domain.Archive.ArchiveFiles
{
    public class FileChannelJson : ArchiveFile
    {
        public const string Extension = ".json";
        public bool IsComplete => true;
        public override string ToString()
        {
            return $"{base.ToString()}{Extension}";
        }

        public FileChannelJson(int? brigade, DateTime pdt, DeviceSerialNumber serialNumber, string fullArchiveName) 
            : base(brigade, pdt, serialNumber, fullArchiveName)
        {

        }

        public static string GetStringFile(string date, DeviceSerialNumber serial) 
            => ArchiveFile.GetStringFile(date, serial) + Extension;

        public new static string GetEmptySerialFile(string date)
            => ArchiveFile.GetEmptySerialFile(date) + Extension;
    }
}
