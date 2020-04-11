using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using WebApi.Archive;
using WebApi.Archive.ArchiveFiles;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;
using WebApi.ValueType;


namespace WebApiTest.Archive
{
    class VideoArchiveUpdateServiceTest
    {
        private IVideoArchiveStructureStore cache;
        private VideoArchiveUpdaterHostedService _updaterHostedService;
        private ILog log;
        private IVideoArchiveConfig config;
        private IVideoArchiveSource rep;
        private const string ArcPath = "root";

         static Func<string, FileVideoMp4> Mp4 = 
            path => new FileVideoMp4(1, new DateTime(), new DeviceSerialNumber(), path, 1, 60);

        private readonly FileVideoMp4[][] files = 
        {
            new []{ Mp4("file_1"), Mp4("file_2"), Mp4("file_3") },
            new []{ Mp4("file_2"), Mp4("file_3"), Mp4("file_4") },
            new []{ Mp4("file_3"), Mp4("file_4"), Mp4("file_5") }
        };

        [SetUp]
        public void Setup()
        {
            log = A.Fake<ILog>();
            config = A.Fake<IVideoArchiveConfig>();
            A.CallTo(() => config.VideoArchiveUpdateTimeMs)
                .Returns(1000);
            A.CallTo(() => config.VideoArchivePath)
                .Returns(ArcPath);
            cache = new VideoArchiveStructureStore();
            rep = A.Fake<IVideoArchiveSource>();
            A.CallTo(()=>rep.GetCompletedFiles())
                .ReturnsNextFromSequence(files);
            _updaterHostedService = new VideoArchiveUpdaterHostedService(log, config, cache, rep);
        }

        [Test]
        public void Empty_Rep()
        {
            var f = cache.GetAll();
            Assert.IsEmpty(f);
        }

        [Test]
        public async Task HandleUpdates_Rep()
        {
            foreach (var f in files)
            {
                await _updaterHostedService.DoWorkAsync(null, CancellationToken.None);
                Assert.AreEqual(cache.GetAll(), f);
            }
        }
    }
}
