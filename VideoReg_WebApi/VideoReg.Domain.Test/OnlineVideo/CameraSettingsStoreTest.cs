using System;
using System.Linq;
using NUnit.Framework;
using VideoReg.Domain.Contract;
using VideoReg.Domain.OnlineVideo.Store;

namespace VideoReg.Domain.Test.OnlineVideo
{
    class CameraSettingsStoreTest
    {
        private ICameraSettingsStore store;
        private readonly ImageSettings DefaultSettings = CameraSettingsStore.DefaultSettings;
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
                var res = store.GetOrDefault(camera);
                Assert.AreEqual(res, DefaultSettings);
            });
        }

        [TestCase(-1433)]
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(7867)]
        public void GetOrDefault_WrongCamera(int camera)
        {
            Assert.Throws<ArgumentException>(() => store.GetOrDefault(camera),
                $"The camera number has value [{camera}], but it must be in the interval [{FirstCamera}-{LastCamera}]");
        }

        [Test]
        public void GetOrDefault_SettingsNotSet()
        {
            LoopByAllCameras((camera) =>
            {
                var res = store.GetOrDefault(camera);
                Assert.AreNotEqual(res, Settings);
            });
        }

        [TestCase(-1433)]
        [TestCase(0)]
        [TestCase(10)]
        [TestCase(7867)]
        public void Set_WrongCamera(int camera)
        {
            store.Set(new CameraSettings(camera, Settings));
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
            Assert.AreEqual(store.GetOrDefault(1), Settings);
            Assert.AreEqual(store.GetOrDefault(3), Settings2);
        }

        private void LoopBySetSettings_AssertAreEqual(ImageSettings settings) => LoopByAllCameras(camera =>
        {
            store.Set(new CameraSettings(camera, settings));
            Assert.AreEqual(store.GetOrDefault(camera), settings);
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
