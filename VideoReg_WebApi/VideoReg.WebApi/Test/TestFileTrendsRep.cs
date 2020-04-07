using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VideoReg.Domain.OnlineVideo;

namespace VideoReg.WebApi.Test
{
    public class TestFileTrendsRep : ITrendsRep
    {
        public async Task<string> GetTrendsIfChangedCAsync(DateTime timestamp)
        {
            return await File.ReadAllTextAsync("..\\..\\..\\Test\\values.json", Encoding.UTF8);
        }
    }
}
