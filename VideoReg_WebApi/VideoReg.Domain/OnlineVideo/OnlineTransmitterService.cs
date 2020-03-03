using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using VideoReg.Domain.Store;

namespace VideoReg.Domain.OnlineVideo
{
    public class OnlineTransmitterService : IHostedService
    {
        private ICameraStore cameraStore;
        public OnlineTransmitterService(ICameraStore cameraStore)
        {
            this.cameraStore = cameraStore;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
