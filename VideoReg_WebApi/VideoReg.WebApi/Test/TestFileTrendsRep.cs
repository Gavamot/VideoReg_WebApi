using System.IO;
using System.Threading.Tasks;
using VideoReg.Domain.OnlineVideo;

namespace VideoReg.WebApi.Test
{
    public class TestFileTrendsRep : ITrendsRep
    {

        public async Task<byte[]> GetThrendsAsync()
        {
            return await File.ReadAllBytesAsync("..\\..\\..\\Test\\values.json");
        }
    }
}
