using System;
using System.Threading.Tasks;

namespace VideoReg.Domain.OnlineVideo
{
    public interface ITrendsRep
    {
        DateTime GetLastChanged();
        Task<string> TryGetTrendsIfChangedAsync(DateTime timestamp);
    }
}
