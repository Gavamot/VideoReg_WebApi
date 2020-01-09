using System.IO;
using System.Text;

namespace VideoReg.Infra.Services
{
    public interface IFileSystemService
    {
        byte[] ReadFile(string file);
        string[] GetFiles(string directory, string pattern = "");
        string[] GetDirectories(string directory);
        long GetFileLengthBytes(string file);
        string GetFullFileName(string file);
        string GetDirName(string dir);
        string ReadFileText(string file, Encoding encoding);
    }
}
