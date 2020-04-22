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
        private readonly ImageSettings DefaultSettings = ImageSettings.DefaultSettings;
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
                Assert.AreEqual(res, DefaultSettings);
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
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(7867)]
        public void Set_WrongCamera(int camera)
        {
            store.Set(new CameraSettings(camera, true, Settings));
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
            var settingsArr = Enumerable.Range(0, 20).Select(x => new CameraSettings
            {
                Camera = x,
                Settings = DefaultSettings
            }) .ToArray();
            settingsArr[1].Settings = Settings; 
            settingsArr[3].Settings = Settings2;
            store.SetAll(settingsArr);
            Assert.AreEqual(store.Get(1), Settings);
            Assert.AreEqual(store.Get(3), Settings2);
        }

        private void LoopBySetSettings_AssertAreEqual(ImageSettings settings) => LoopByAllCameras(camera =>
        {
            store.Set(new CameraSettings(camera, true, settings));
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
