using System.Linq;
using VideoReg.Domain.OnlineVideo;

namespace VideoReg.WebApi.Test
{
    public class TestCameraRep : ICameraSourceRep
    {
        readonly CameraSourceSettings[] settings = {
            new CameraSourceSettings (1, "http://192.168.88.242/webcapture.jpg?command=snap&channel=0"),
            new CameraSourceSettings (2, "http://192.168.20.31/webcapture.jpg?command=snap&channel=0")
        };

        public CameraSourceSettings Get(int cameraNumber) => settings.FirstOrDefault(x => x.number == cameraNumber);
        public CameraSourceSettings[] GetAll() => settings;
    }
}
