using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Services
{

    public class FileSystemService : IFileSystemService
    {
        public byte[] ReadFile(string file) 
            => File.ReadAllBytes(file);

        public async Task<byte[]> ReadFileAsync(string file)
            => await File.ReadAllBytesAsync(file);

        public string[] GetFiles(string directory, SearchOption options = SearchOption.AllDirectories, string pattern = "*") 
            => Directory.GetFiles(directory, pattern, options);

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

        public async Task<string> ReadFileTextAsync(string file, Encoding encoding)
            => await File.ReadAllTextAsync(file);

        public DateTime GetLastModification(string file) =>
            File.GetLastWriteTime(file);

        public async Task<MemoryStream> ReadFileToMemoryAsync(string file)
        {
            var ms = new MemoryStream();
            await using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.Asynchronous);
            await fs.CopyToAsync(ms);
            return ms;
        }

        public async Task<MemoryStream> ReadFileToMemory(string file)
        {
            var ms = new MemoryStream();
            await using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.Asynchronous);
            await fs.CopyToAsync(ms);
            return ms;
        }

        MemoryStream IFileSystemService.ReadFileToMemory(string file)
        {
            var ms = new MemoryStream();
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            fs.CopyTo(ms);
            return ms;
        }
    }
}
