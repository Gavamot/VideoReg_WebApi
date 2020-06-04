using System;
using System.Threading.Tasks;
using WebApi.Archive;

namespace WebApi.Core.SignalR
{

    public interface IArchiveTransmitter
    {
        public Task UploadCameraFileAsync(DateTime pdt, DateTime end, int camera);
        public Task UploadTrendsFileAsync(DateTime pdt, DateTime end);
    }

    public class ArchiveTransmitter : IArchiveTransmitter
    {
        private readonly IRegInfoRep regInfoRep;
        private readonly ICameraArchiveRep cameraArchiveRep;
        private readonly ITrendsArchiveRep trendsArchiveRep;
        private readonly AscHttpClient httpAsc;

        public ArchiveTransmitter(
            IRegInfoRep regInfoRep,
            ICameraArchiveRep cameraArchiveRep,
            ITrendsArchiveRep trendsArchiveRep,
            AscHttpClient httpAsc)
        {
            this.regInfoRep = regInfoRep;
            this.cameraArchiveRep = cameraArchiveRep;
            this.trendsArchiveRep = trendsArchiveRep;
            this.httpAsc = httpAsc;
        }

        public async Task UploadCameraFileAsync(DateTime pdt, DateTime end, int camera)
        {
            var file = await cameraArchiveRep.TryGetNearestFrontVideoFileAsync(pdt, camera);
            await httpAsc.UploadCameraFileAsync(regInfoRep.Vpn, file, end, camera);
        }

        public async Task UploadTrendsFileAsync(DateTime pdt, DateTime end)
        {
            var file = await trendsArchiveRep.GetNearestFrontTrendFileAsync(pdt);
            await httpAsc.UploadTrendsFileAsync(regInfoRep.Vpn, file, end);
        }
    }
}
