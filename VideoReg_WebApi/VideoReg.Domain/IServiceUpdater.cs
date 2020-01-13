using System.Threading;

namespace VideoRegService
{
    public interface IServiceUpdater
    {
        void DoWork(CancellationToken cancellationToken);
        string Name { get; }
    }
}