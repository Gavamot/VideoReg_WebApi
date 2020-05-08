using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.Collection;
using WebApi.Services;
using WebApi.Trends;

namespace WebApiTest
{
    //public class TestTrendsRep : ITrendsRep
    //{
    //    readonly IDateTimeService dateTimeService;
    //    readonly IFileSystemService fileSystem;
    //    public TestTrendsRep(IDateTimeService dateTimeService, IFileSystemService fileSystem)
    //    {
    //        this.dateTimeService = dateTimeService;
    //        this.fileSystem = fileSystem;
    //    }

    //    public DateTime GetLastChanged()
    //    {
    //        return dateTimeService.GetNow();
    //    }

    //    public async Task<TimestampValue<string>> GetTrendsIfChangedAsync(DateTime timestamp)
    //    {
    //        DateTime lastMod = GetLastChanged();
    //        if (lastMod > timestamp)
    //        {
    //            var f = await fileSystem.ReadFileTextAsync("../../../Test/values.json", Encoding.UTF8);
    //            return new TimestampValue<string>(lastMod, f);
    //        }
    //        return null;
    //    }

    //}
}
