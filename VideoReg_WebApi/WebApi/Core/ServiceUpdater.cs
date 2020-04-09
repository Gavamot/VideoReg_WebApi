using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebApi.Services;

namespace WebApi
{
    public abstract class ServiceUpdater : IHostedService, IServiceUpdater
    {
        protected readonly int updateTimeMs;
        protected readonly ILog log;

        protected ServiceUpdater(
            int updateTimeMs,
            ILog log)
        {
            if (updateTimeMs <= 0)
                updateTimeMs = 1;
            this.updateTimeMs = updateTimeMs;
            this.log = log;
        }

        public abstract string Name { get; }
        public string ServiceName => $"Service {Name}";

        protected Action BeforeStart;

        protected int GetSleepTimeMs(long elapsedMilliseconds)
        {
            int sleepMs = (int)(updateTimeMs - elapsedMilliseconds);
            if (sleepMs > 0 && sleepMs <= updateTimeMs)
                return sleepMs;
            return 0;
        }

        protected async Task SleepIfNeedMsAsync(long elapsedMilliseconds, CancellationToken token)
        {
            int sleepMs = GetSleepTimeMs(elapsedMilliseconds);
            log.Debug($"{ServiceName} update operation is completed. Execution time = {elapsedMilliseconds} ms");
            if (sleepMs > 0)
                await Task.Delay(sleepMs, token);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            BeforeStart?.Invoke();
            new Task(action: async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        await DoWorkAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        log.Error($"{ServiceName} has error. ({e.Message})", e);
                    }
                    stopwatch.Stop();
                    await SleepIfNeedMsAsync(stopwatch.ElapsedMilliseconds, cancellationToken);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning).Start();
            log.Info($"{ServiceName} is started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            log.Info($"{ServiceName} is stopped");
            return Task.CompletedTask;
        }

        public abstract Task DoWorkAsync(CancellationToken cancellationToken);
    }
}