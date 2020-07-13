using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebApi.OnlineVideo;
using WebApi.Services;

namespace WebApiTest
{
    public class TestCameraRep : ICameraHttpSourceRep
    {
        private readonly IFileSystemService fs;
        public TestCameraRep(IFileSystemService fs)
        {
            this.fs = fs;
        }

        //readonly CameraSourceSettings[] Settings = {
        //    //new CameraSourceSettings(1, "http://192.168.88.10/tmpfs/auto.jpg"),
        //    //new CameraSourceSettings(2, "http://192.168.88.242/webcapture.jpg?command=snap&amp;channel=1"),
        //    //new CameraSourceSettings(3, "http://192.168.88.82/ISAPI/Streaming/channels/101/picture?snapShotImageType=JPEG"),
        //    //new CameraSourceSettings (4, "http://192.168.88.242/webcapture.jpg?command=snap&channel=0"),
        //    //new CameraSourceSettings (5, "http://192.168.20.31/webcapture.jpg?command=snap&channel=0")
        //    new CameraSourceSettings(1, "http://192.168.20.125:8080/Snapshot/GetSnapshot"),
        //    new CameraSourceSettings(3, "http://192.168.20.125:80/Snapshot/GetSnapshot"),
        //};

        public async Task<CameraSourceHttpSettings> Get(int cameraNumber) => (await GetAll()).FirstOrDefault(x => x.number == cameraNumber);

        public async Task<CameraSourceHttpSettings[]> GetAll()
        {
            var text = await fs.ReadFileTextAsync("Test/camera_source.json", Encoding.UTF8);
            return JsonConvert.DeserializeObject<CameraSourceHttpSettings[]>(text);
        }
    }
}
