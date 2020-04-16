using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Contract;

namespace WebApi.Core
{
    public static class Must
    {
        public static async Task<T> Do<T>(Func<Task<T>> func, int waitIfErrorMs, CancellationToken cancellationToken, Action<Exception> OnError = null)
        {
            while (true)
            {
                try
                {
                    return await func();
                }
                catch (Exception e)
                {
                    OnError(e);
                    await Task.Delay(waitIfErrorMs, cancellationToken);
                }
            }
        }

        public static async Task Do(Func<Task> action, int waitIfErrorMs, CancellationToken cancellationToken, Action<Exception> OnError = null)
        {
            while (true)
            {
                try
                {
                    await action();
                    break;
                }
                catch (Exception e)
                {
                    OnError(e);
                    await Task.Delay(waitIfErrorMs, cancellationToken);
                }
            }
        }
    }
}
