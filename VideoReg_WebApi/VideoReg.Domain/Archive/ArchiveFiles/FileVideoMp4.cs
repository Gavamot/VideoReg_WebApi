using System;
using VideoReg.Domain.ValueType;

namespace VideoReg.Domain.Archive.ArchiveFiles
{
    public class FileVideoMp4 : ArchiveFile
    {
        public const string Extension = ".mp4";
        public readonly int cameraNumber;
        public readonly int? durationSeconds;
        public bool IsComplete => durationSeconds != null;

        public FileVideoMp4(ArchiveFile file, int cameraNumber, int? durationSeconds)
            : base(file.brigade, file.pdt, file.serialNumber, file.fullArchiveName)
        {
            this.cameraNumber = cameraNumber;
            this.durationSeconds = durationSeconds;
        }
        public FileVideoMp4(int? brigade, DateTime pdt, DeviceSerialNumber serialNumber, string fullArchiveName, int cameraNumber, int? durationSeconds)
            : base(brigade, pdt, serialNumber, fullArchiveName)
        {
            this.cameraNumber = cameraNumber;
            this.durationSeconds = durationSeconds;
        }

        public static string GetStringFile(string date, DeviceSerialNumber serial, int? duration)
        {
            var res = duration == default ?
                ArchiveFile.GetStringFile(date, serial) :
                ArchiveFile.GetStringFile(date, serial, duration);
            res += Extension;
            return res;
        }

        public static string GetEmptySerialFile(string date, int? duration)
        {
            var res = ArchiveFile.GetEmptySerialFile(date);
            if (res != null)
                res += ArchiveFile.PatsSeparator + duration;
            res += Extension;
            return res;
        }

        public override string ToString()
        {
            string dur = durationSeconds == default ? "" : $"{PatsSeparator}{durationSeconds}"; 
            return $"{base.ToString()}{dur}{Extension}";
        }

        #region Compare

        protected bool Equals(FileVideoMp4 other)
        {
            return base.Equals(other) 
                   && cameraNumber == other.cameraNumber 
                   && durationSeconds == other.durationSeconds;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileVideoMp4)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ cameraNumber;
                hashCode = (hashCode * 397) ^ durationSeconds.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
