using System;
using System.Collections.Generic;
using System.IO;
using VideoReg.Domain.ValueType;
using System.Linq;

namespace VideoReg.Domain.Archive.ArchiveFiles
{
    public abstract class ArchiveFile
    {
        protected ArchiveFile(int? brigade, DateTime pdt, DeviceSerialNumber serialNumber, string fullArchiveName)
        {
            this.brigade = brigade;
            this.pdt = pdt;
            this.serialNumber = serialNumber;
            this.fullArchiveName = fullArchiveName;
        }
        public readonly int? brigade;
        public readonly DateTime pdt;
        public readonly DeviceSerialNumber serialNumber;

        /// <summary>
        /// Полное имя файла относительно папки с архивами
        /// </summary>
        public readonly string fullArchiveName;
        public const string PatsSeparator = "_";
        public const string EmptySerialLetter = "N";
        public const string EmptySerial = "N_N_N_N";
        public override string ToString()
        {
            return $"{pdt:yyyy.M.ddTHH.mm.ss}{PatsSeparator}{serialNumber}";
        }

        protected static string GetStringFile(params object[] args)
        {
            IEnumerable<object> arguments = args;
            if (args.Any(x => x is DeviceSerialNumber))
            {
                var parameters = new List<object>();
                foreach (var arg in args)
                {
                    if (arg is DeviceSerialNumber s)
                    {
                        parameters.AddRange(s.Values.Select(x=>(object)x));
                    }
                    else
                    {
                        parameters.Add(arg);
                    }
                }
                arguments = parameters;
            }
            return string.Join(PatsSeparator, arguments);
        }

        protected static string GetEmptySerialFile(string date) => 
            GetStringFile(date, EmptySerialLetter, EmptySerialLetter, EmptySerialLetter, EmptySerialLetter);

        #region Compare

        protected bool Equals(ArchiveFile other)
        {
            return brigade == other.brigade 
                   && pdt.Equals(other.pdt) 
                   && serialNumber.Equals(other.serialNumber) 
                   && fullArchiveName == other.fullArchiveName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ArchiveFile)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = brigade.GetHashCode();
                hashCode = (hashCode * 397) ^ pdt.GetHashCode();
                hashCode = (hashCode * 397) ^ serialNumber.GetHashCode();
                hashCode = (hashCode * 397) ^ (fullArchiveName != null ? fullArchiveName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}
