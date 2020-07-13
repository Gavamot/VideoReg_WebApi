using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebApi.CoreService.Core;
using WebApi.OnlineVideo;

namespace WebApi.Core.OnlineVideo
{
    public class RedisSnapshotRep : ISnapshotRep
    {
        IRedisRep redis;
        public RedisSnapshotRep(IRedisRep redis)
        {
            this.redis = redis;
        }

        public Snapshot[] GetAll()
        {
            var res = new Snapshot[9];
            var tasks = new Task[9];
            for (int i = 0; i < 9; i++)
            {
                int camera = i + 1;
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        res[camera - 1] = await redis.GetObject<Snapshot>($"cam_{camera}");
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                });
            }
            Task.WaitAll(tasks);
            return res;
        }
    }
}
