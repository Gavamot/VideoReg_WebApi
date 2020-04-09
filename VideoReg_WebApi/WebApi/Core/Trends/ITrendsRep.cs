using System;
using System.Threading.Tasks;
using WebApi.Collection;

namespace WebApi.Trends
{
    public interface ITrendsRep
    {
        DateTime GetLastChanged();
        Task<TimestampValue<string>> GetTrendsIfChangedAsync(DateTime timestamp);
    }
}
