using System;
using System.Threading;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using VideoReg.Domain.Archive;
using VideoReg.Domain.Archive.ArchiveFiles;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.ValueType;
using VideoReg.Infra.Services;
using VideoReg.Infra.Test;
using VideoRegService.Core.Archive;


namespace VideoReg.Domain.Test.Archive
{
    class VideoArchiveUpdateServiceTest
    {
        private IMemoryCache cache;
        private VideoArchiveUpdateHostedService _updateHostedService;
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
            cache = new CacheTest();
            rep = A.Fake<IVideoArchiveSource>();
            A.CallTo(()=>rep.GetCompletedVideoFiles(""))
                .WithAnyArguments()
                .ReturnsNextFromSequence(files);
            _updateHostedService = new VideoArchiveUpdateHostedService(log, config, cache, rep);
        }

        [Test]
        public void Empty_Rep()
        {
            var f = cache.GetVideoArchiveCache();
            Assert.IsEmpty(f);
        }

        [Test]
        public void HandleUpdates_Rep()
        {
            foreach (var f in files)
            {
                _updateHostedService.DoWork(new CancellationToken());
                Assert.AreEqual(cache.GetVideoArchiveCache(), f);
            }
        }
    }
}
