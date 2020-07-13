using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApi.Services;

namespace WebApi
{
    public interface IServiceUpdater : IHostedService
    {
        public object Context { get; }
        public Task<bool> BeforeStart(object context, CancellationToken cancellationToken);
        Task DoWorkAsync(object context, CancellationToken cancellationToken);
        public void Continue();
        public void Pause();
        string Name { get; }
    }

    public abstract class ServiceUpdater : IServiceUpdater, IHostedService
    {
        protected readonly int updateTimeMs;
        protected readonly ILogger<ServiceUpdater> log;
        public abstract object Context { get; protected set; }

        private volatile object isPause = false;
         
        protected ServiceUpdater(
            int updateTimeMs,
            ILogger<ServiceUpdater> log)
        {
            this.updateTimeMs = GetCorrectUpdateTimeMs(updateTimeMs);
            this.log = log;

        }

        public void Pause()
        {
            isPause = true;
        }

        public void Continue()
        {
            isPause = false;
        }

        public abstract string Name { get; }
        public string ServiceName => $"Service {Name}";
        /// <summary>
        /// Происходит перед вызовом основного цикла
        /// </summary>
        /// <returns>true - запустить цикл постоянного обновления, false - не запускать  цикл постоянного обновления</returns>
        public abstract Task<bool> BeforeStart(object context, CancellationToken cancellationToken);

        public abstract Task DoWorkAsync(object context, CancellationToken cancellationToken);
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var isStart = await BeforeStart(Context, cancellationToken);
            if (!isStart) return;
            new Task(action: async () =>
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if ((bool) isPause)
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    var stopwatch = Stopwatch.StartNew();

                    try
                    {
                        await DoWorkAsync(Context, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        //break;
                    }
                    catch (Exception e)
                    {
                        log.LogError($"{ServiceName} has error. ({e.Message})");
                    }

                    stopwatch.Stop();
                    await SleepIfNeedMsAsync(stopwatch.ElapsedMilliseconds, cancellationToken);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning).Start();
            log.LogInformation($"{ServiceName} is started");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            log.LogInformation($"{ServiceName} is stopped");
            return Task.CompletedTask;
        }

        protected int GetSleepTimeMs(long elapsedMilliseconds)
        {
            int sleepMs = (int)(updateTimeMs - elapsedMilliseconds);
            if (sleepMs > 0 && sleepMs <= updateTimeMs)
                return sleepMs;
            return 1;
        }

        protected async Task SleepIfNeedMsAsync(long elapsedMilliseconds) => await SleepIfNeedMsAsync(elapsedMilliseconds, CancellationToken.None);

        protected async Task SleepIfNeedMsAsync(long elapsedMilliseconds, CancellationToken token)
        {
            int sleepMs = GetSleepTimeMs(elapsedMilliseconds);
            log.LogDebug($"{ServiceName} update operation is completed. Execution time = {elapsedMilliseconds} ms"); 
            await Task.Delay(sleepMs, token);
        }

        private int GetCorrectUpdateTimeMs(int updateTimeMs)
        {
            if (updateTimeMs <= 0)
            {
                log.LogWarning($"{Name} - updateTimeMs have value={updateTimeMs} this value was changed to {1} (Cause for performance reasons shout have interaction between loop operations)");
                updateTimeMs = 1;
            }
            return updateTimeMs;
        }
    }
}