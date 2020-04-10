using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace WebApi
{
    public interface IServiceUpdater : IHostedService
    {
        public object Context { get; }
        public Task<bool> BeforeStart(object context, CancellationToken cancellationToken);
        Task DoWorkAsync(object context, CancellationToken cancellationToken);
        string Name { get; }
    }
}