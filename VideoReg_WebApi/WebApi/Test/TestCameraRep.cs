using System.Linq;
using System.Threading.Tasks;
using WebApi.OnlineVideo;

namespace WebApiTest
{
    public class TestCameraRep : ICameraSourceRep
    {
        readonly CameraSourceSettings[] settings = {
            //new CameraSourceSettings(1, "http://192.168.88.10/tmpfs/auto.jpg"),
            //new CameraSourceSettings(2, "http://192.168.88.242/webcapture.jpg?command=snap&amp;channel=1"),
            //new CameraSourceSettings(3, "http://192.168.88.82/ISAPI/Streaming/channels/101/picture?snapShotImageType=JPEG"),
            //new CameraSourceSettings (4, "http://192.168.88.242/webcapture.jpg?command=snap&channel=0"),
            //new CameraSourceSettings (5, "http://192.168.20.31/webcapture.jpg?command=snap&channel=0")
            new CameraSourceSettings(1, "http://192.168.20.125:8080/Snapshot/GetSnapshot"),
            new CameraSourceSettings(3, "http://192.168.20.125:80/Snapshot/GetSnapshot"),
        };

        public async Task<CameraSourceSettings> Get(int cameraNumber) => settings.FirstOrDefault(x => x.number == cameraNumber);
        public async Task<CameraSourceSettings[]> GetAll() => settings;
    }
}
