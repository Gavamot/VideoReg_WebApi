using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VideoReg.Infra.Services
{
    public class FileSystemService : IFileSystemService
    {
        public byte[] ReadFile(string file) 
            => File.ReadAllBytes(file);

        public string[] GetFiles(string directory, string pattern = "*") 
            => Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);

        public string[] GetDirectories(string directory) 
            => Directory.GetDirectories(directory);

        public long GetFileLengthBytes(string file)
            => new FileInfo(file).Length;
        
        public string GetFullFileName(string file)
            => new FileInfo(file).FullName;

        public string GetDirName(string dir)
            => new DirectoryInfo(dir).Name;
       
        public string ReadFileText(string file, Encoding encoding)
            => File.ReadAllText(file, encoding);
    }
}
