using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Services
{
    public interface IFileSystemService
    {
        byte[] ReadFile(string file);
        Task<byte[]> ReadFileAsync(string file);
        string[] GetFiles(string directory, string pattern = "");
        string[] GetDirectories(string directory);
        long GetFileLengthBytes(string file);
        string GetFullFileName(string file);
        string GetDirName(string dir);
        string ReadFileText(string file, Encoding encoding);
        Task<string> ReadFileTextAsync(string file, Encoding encoding);
        DateTime GetLastModification(string file);
        Task<MemoryStream> ReadFileToMemoryAsync(string file);
        MemoryStream ReadFileToMemory(string file);
    }
}
