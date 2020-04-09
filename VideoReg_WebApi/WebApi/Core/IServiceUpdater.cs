using System.Threading;
using System.Threading.Tasks;

namespace WebApi
{
    public interface IServiceUpdater
    {
        Task DoWorkAsync(CancellationToken cancellationToken);
        string Name { get; }
    }
}