using System;
using System.Linq;
using NUnit.Framework;
using WebApi.Contract;
using WebApi.OnlineVideo.Store;

namespace WebApiTest.OnlineVideo
{
    class CameraSettingsStoreTest
    {
        private ICameraSettingsStore store;
        private int FirstCamera = 1;
        private int LastCamera = 9;

        

        private ImageSettings Settings => new ImageSettings
        {
            Width = 300,
            Height = 200,
            Quality = 65
        };

        private ImageSettings Settings2 => new ImageSettings
        {
            Width = 200,
            Height = 100,
            Quality = 49
        };

        [SetUp]
        public void Setup()
        {
            store = new CameraSettingsStore();
        }

        [Test]
        public void GetOrDefault_EmptyCache()
        {
            LoopByAllCameras(camera =>
            {
                var res = store.Get(camera);
                Assert.AreEqual(res, CameraSettings.GetDefault(camera));
            });
        }

        [TestCase(-1433)]
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(7867)]
        public void GetOrDefault_WrongCamera(int camera)
        {
            Assert.Throws<ArgumentException>(() => store.Get(camera),
                $"The camera number has value [{camera}], but it must be in the interval [{FirstCamera}-{LastCamera}]");
        }

        [Test]
        public void GetOrDefault_SettingsNotSet()
        {
            LoopByAllCameras((camera) =>
            {
                var res = store.Get(camera);
                Assert.AreNotEqual(res, Settings);
            });
        }

        [TestCase(-1433)]
        [TestCase(10)]
        [TestCase(7867)]
        public void Set_WrongCameraThrowException(int camera)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                store.Set(CameraSettings.GetDefault(camera));
            });
            GetOrDefault_SettingsNotSet();
        }

        [Test]
        public void Set_Each()
        {
            LoopBySetSettings_AssertAreEqual(Settings);
        }

        [Test]
        public void Set_EachRepeat()
        {
            LoopBySetSettings_AssertAreEqual(Settings);
            LoopBySetSettings_AssertAreEqual(Settings2);
        }

        [Test]
        public void SetAll()
        {
            var settings = new CameraSettings()
            {
                Camera = 0,
                Settings = Settings2,
                EnableConversion = true,
                Enabled = true
            };

            store.Set(settings);
            LoopByAllCameras((camera) =>
            {
                var cameraSettings = store.Get(camera);
                settings.Camera = camera;
                Assert.AreEqual(cameraSettings, settings);
            });
        }

        private void LoopBySetSettings_AssertAreEqual(ImageSettings imageSettings) => LoopByAllCameras(camera =>
        {
            var settings = new CameraSettings(camera, true, false, imageSettings);
            store.Set(settings);
            Assert.AreEqual(store.Get(camera), settings);
        });

        private void LoopByAllCameras(Action<int> action)
        {
            for (int camera = FirstCamera; camera <= LastCamera; camera++)
            {
                action(camera);
            }
        }
    }
}
