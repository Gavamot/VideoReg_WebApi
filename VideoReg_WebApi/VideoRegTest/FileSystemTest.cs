using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApiTest
{
    public class FileSystemTest : IFileSystemService
    {
        private readonly IEnumerable<FileTest> files;
        public FileSystemTest(IEnumerable<FileTest> files)
        {
            this.files = files;
        }

        /// <summary>
        /// ReadFile
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        public byte[] ReadFile(string file)
        {
            return files.First(x => x.fullName == file).body;
        }

        public Task<byte[]> ReadFileAsync(string file)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// GetFiles
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="options"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        /// <exception cref="RegexMatchTimeoutException">Ignore.</exception>
        public string[] GetFiles(string directory, SearchOption options = SearchOption.AllDirectories, string pattern = "*")
        {
            var regex = new Regex(pattern);
            return files.Where(x => x.IsInDirectory(directory))
                .Where(x => regex.IsMatch(x.name))
                .Select(x => x.fullName)
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// GetDirectories
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
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

        /// <summary>
        /// GetFileLengthBytes
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        public long GetFileLengthBytes(string file)
        {
            return files.First(x => x.fullName == file).size;
        }

        public string GetFullFileName(string file)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetDirName
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        public string GetDirName(string dir)
        {
            return dir.Split("\\").Last();
        }

        /// <summary>
        /// ReadFileText
        /// </summary>
        /// <param name="file"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
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

        /// <summary>
        /// ReadFileToMemoryAsync
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        public MemoryStream ReadFileToMemoryAsync(string file)
        {
            var f = files.First(x => x.fullName == file);
            return new MemoryStream(f.body);
        }

        public MemoryStream ReadFileToMemory(string file)
        {
            throw new NotImplementedException();
        }

        Task<MemoryStream> IFileSystemService.ReadFileToMemoryAsync(string file)
        {
            throw new NotImplementedException();
        }
    }
}
