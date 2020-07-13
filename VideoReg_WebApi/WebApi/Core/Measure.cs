using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebApi
{
    public static class Measure
    {
        public static T Invoke<T>(Func<T> action, out TimeSpan execTime)
        {
            var timer = Stopwatch.StartNew();
            var res = action();
            timer.Stop();
            execTime = timer.Elapsed;
            return res;
        }

        public static T Invoke<T>(Func<T> action, Action<TimeSpan> execTimeAct)
        {
            var timer = Stopwatch.StartNew();
            var res = action();
            timer.Stop();
            var execTime = timer.Elapsed;
            execTimeAct(execTime);
            return res;
        }

        public static void Invoke(Action action, out TimeSpan execTime)
        {
            var timer = Stopwatch.StartNew();
            action();
            timer.Stop();
            execTime = timer.Elapsed;
        }

        public static void Invoke(Action action, Action<TimeSpan> execTimeAct)
        {
            var timer = Stopwatch.StartNew();
            action();
            timer.Stop();
            var execTime = timer.Elapsed;
            execTimeAct(execTime);
        }

    }
}
