using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using VideoReg.Infra.Services;

namespace VideoRegService
{
    public abstract class ServiceUpdater : IHostedService, IDisposable, IServiceUpdater
    {
        protected Timer _timer;
        protected readonly int updateTimeMs;
        protected readonly ILog log;
        protected volatile bool isUpdating = true;

        protected ServiceUpdater(
            int updateTimeMs,
            ILog log)
        {
            this.updateTimeMs = updateTimeMs;
            this.log = log;
        }

        public abstract string Name { get; }
        public string ServiceName => $"Service {Name}";
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Update, cancellationToken, TimeSpan.Zero, TimeSpan.FromMilliseconds(updateTimeMs));
            Update(cancellationToken);
            log.Info($"{ServiceName} is started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            log.Info($"{ServiceName} is stopped");
            return Task.CompletedTask;
        }

        private void Update(object state)
        {
            if(!isUpdating) return;
            isUpdating = false;
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                DoWork((CancellationToken) state);
                stopwatch.Stop();
                log.Info($"{ServiceName} update operation is completed. Execution time = { stopwatch.ElapsedMilliseconds } ms");
            }
            catch(Exception e)
            {
                log.Error($"{ServiceName} has error. ({e.Message})", e);
            }
            isUpdating = true;
        }

        public abstract void DoWork(CancellationToken cancellationToken);

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}