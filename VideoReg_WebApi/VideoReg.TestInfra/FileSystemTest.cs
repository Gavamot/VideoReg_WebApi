using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoReg.Infra.Services;

namespace VideoReg.Infra.Test
{
    public class FileSystemTest : IFileSystemService
    {
        private readonly IEnumerable<FileTest> files;
        public FileSystemTest(IEnumerable<FileTest> files)
        {
            this.files = files;
        }

        public byte[] ReadFile(string file)
        {
            return files.First(x => x.fullName == file).body;
        }

        public Task<byte[]> ReadFileAsync(string file)
        {
            throw new NotImplementedException();
        }

        public string[] GetFiles(string directory, string pattern = "")
        {
            var regex = new Regex(pattern);
            return files.Where(x => x.IsInDirectory(directory))
                .Where(x => regex.IsMatch(x.name))
                .Select(x=>x.fullName)
                .Distinct()
                .ToArray();
        }

        public string[] GetDirectories(string directory)
        {
            var res = files.Where(x => x.fullName.StartsWith(directory))
                .Select(x => x.fullName.Replace(directory, string.Empty))
                .Where(x => x.Split("\\").Count(x => x != string.Empty) > 1)
                .Select(x => x.Split("\\").First(x => x != string.Empty))
                .Distinct()
                .Select(x=> Path.Combine(directory, x))
                .ToArray();
            return res;
        }

        public long GetFileLengthBytes(string file)
        {
            return files.First(x => x.fullName == file).size;
        }

        public string GetFullFileName(string file)
        {
            throw new NotImplementedException();
        }

        public string GetDirName(string dir)
        {
            return dir.Split("\\").Last();
        }

        public string ReadFileText(string file, Encoding encoding)
        {
            return encoding.GetString(files.First(x => x.fullName == file).body); 
        }

        public Task<string> ReadFileTextAsync(string file, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastModification(string file)
        {
            throw new NotImplementedException();
        }

        public MemoryStream ReadFileToMemory(string file)
        {
            var f = files.First(x => x.fullName == file);
            return new MemoryStream(f.body);
        }
    }
}
