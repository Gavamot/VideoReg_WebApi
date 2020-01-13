using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace VideoReg.Infra.Test
{
    public struct FileTest
    {
        public readonly string fullName;
        public readonly string name;
        public readonly int size;
        public readonly byte[] body;
        public readonly string extension;
        public bool IsInDirectory(string dir) => fullName.StartsWith(dir); 

        public FileTest(string name, byte[] body = default)
        {
            this.body = body ?? new byte[0];
            this.fullName = name.Trim().Replace("/", "\\");
            this.name = fullName.Split("\\").Last();
            this.size = this.body.Length;
            this.extension = name.Split(".").Last();
            if (extension == name) 
                extension = string.Empty;
        }
    }
}