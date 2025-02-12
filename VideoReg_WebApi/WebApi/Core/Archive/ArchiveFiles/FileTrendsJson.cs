﻿using System;
using WebApi.ValueType;

namespace WebApi.Archive.ArchiveFiles
{
    public class FileTrendsJson : ArchiveFile
    {
        public const string Extension = ".json";
        public bool IsComplete => true;
        public const string EmptyFileName = "e"; 

        public override string ToString()
        {
            return $"{base.ToString()}{Extension}";
        }

        public static FileTrendsJson EmptyFile => new FileTrendsJson();
        
        public FileTrendsJson() : this(0, DateTime.MaxValue, new DeviceSerialNumber(), EmptyFileName)
        {
            
        } 

        public FileTrendsJson(int brigade, DateTime pdt, DeviceSerialNumber serialNumber, string fullArchiveName) 
            : base(brigade, pdt, serialNumber, fullArchiveName)
        {

        }

        public static string GetStringFile(string date, DeviceSerialNumber serial) 
            => ArchiveFile.GetStringFile(date, serial) + Extension;

        public new static string GetEmptySerialFile(string date)
            => ArchiveFile.GetEmptySerialFile(date) + Extension;
    }
}
