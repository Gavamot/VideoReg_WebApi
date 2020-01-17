using System;
using System.IO;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using VideoReg.Domain.Archive;
using VideoReg.Domain.Archive.ArchiveFiles;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Domain.Archive.Config;
using VideoReg.Infra.Services;
using VideoReg.Infra.Test;

namespace VideoReg.Domain.Test.Archive
{
    class VideoArchiveSourceFSTest
    {
        private VideoArchiveSourceFS videoArchiveSourceFs;

        private string file1 = $"2016.12.11T13.30.23_1_1_1_1_50.{FileVideoMp4.Extension}";

        private static readonly DateTime dt1 = new DateTime(2018, 12, 11, 2, 3, 4);
        private readonly string dt1Str = dt1.ToString(ArchiveFileFactory.FileNameDateFormat);

        private static readonly DateTime dt2 = new DateTime(2018, 12, 11, 2, 4, 4);
        private readonly string dt2Str = dt2.ToString(ArchiveFileFactory.FileNameDateFormat);

        private static readonly DateTime dt3 = new DateTime(2018, 12, 11, 2, 5, 4);
        private readonly string dt3Str = dt3.ToString(ArchiveFileFactory.FileNameDateFormat);

        private readonly int duration = 50;
        public VideoArchiveSourceFSTest()
        {
            string[] Files(string dir, string[] f) => f.Select(x => Path.Combine(dir, x)).ToArray();
            f1 = Files(dir1, new[]
            {
                FileVideoMp4.GetEmptySerialFile(dt1Str, duration),
                FileVideoMp4.GetEmptySerialFile(dt2Str, duration),
                FileVideoMp4.GetEmptySerialFile(dt3Str, null),
                "arc/1/34324.mp4",
                FileChannelJson.GetEmptySerialFile(dt1Str)
            });
            f2 = Files(dir2, new[]
            {
                FileVideoMp4.GetEmptySerialFile(dt1Str, duration),
                FileVideoMp4.GetEmptySerialFile(dt2Str, null),
                FileVideoMp4.GetEmptySerialFile(dt3Str, duration),
                "arc/2/34324.mp4",
                FileChannelJson.GetEmptySerialFile(dt1Str)
            });

            var files = f1.Concat(f2)
                .Select(x=>new FileTest(x))
                .ToArray();
            fs = new FileSystemTest(files);
        }

        private string[] f1;
        private string[] f2;
        const string root = "arc";
        string dir1 = Path.Combine(root, "1");
        string dir2 = Path.Combine(root, "2");
        private string[] dirs => new[] { dir1, dir2, string.Empty, "qwe", "123" };
        private IArchiveConfig config;
        private ILog log;
        private IBrigadeHistory brigadeHistory;
        private IBrigadeHistoryRep brigadeHistoryRep;
        private IFileSystemService fs;
        private const string Pattern = "^.*T.*(_.*){5}\\.mp4$";

        [SetUp]
        public void Setup()
        {
            config = A.Fake<IArchiveConfig>();
            log = A.Fake<ILog>();
            A.CallTo(() => config.VideoArchivePath)
                .Returns(root);

            brigadeHistory = A.Fake<IBrigadeHistory>();
            A.CallTo(() => brigadeHistory.GetBrigadeCode(DateTime.MinValue))
                .WithAnyArguments()
                .Returns(1);

            brigadeHistoryRep = A.Fake<IBrigadeHistoryRep>();
            A.CallTo(() => brigadeHistoryRep.GetBrigadeHistory())
                .Returns(brigadeHistory);

            videoArchiveSourceFs = new VideoArchiveSourceFS(log, config, brigadeHistoryRep, fs);
        }

        [Test]
        public void Correct_CheckCalls()
        {
            var files = videoArchiveSourceFs.GetCompletedVideoFiles(Pattern).ToArray();

            A.CallTo(() => config.VideoArchivePath)
                .MustHaveHappened(1, Times.OrMore);

            A.CallTo(() => brigadeHistory.GetBrigadeCode(A<DateTime>.Ignored))
                .MustHaveHappened(files.Length, Times.OrMore);

            A.CallTo(() => brigadeHistoryRep.GetBrigadeHistory())
                .MustHaveHappened(1, Times.Exactly);
        }

        [Test]
        public void Correct_SkipWithOutDuration()
        {
            var files = videoArchiveSourceFs.GetCompletedVideoFiles(Pattern);
            Assert.IsTrue(files.All(x => x.IsComplete));
        }

        [Test]
        public void Correct_CorrectResult()
        {
            FileVideoMp4 Mp4(DateTime dt, string path, int cam) => 
                new FileVideoMp4(1, dt, default, 
                    path.Replace(root + "\\", ""), cam, duration);
            var files = videoArchiveSourceFs.GetCompletedVideoFiles(Pattern);
            var actual = new[]
            {
                Mp4(dt1, f1[0], 1),
                Mp4(dt2, f1[1], 1),
                Mp4(dt1, f2[0], 2),
                Mp4(dt3, f2[2], 2)
            };
            Assert.AreEqual(files, actual);
        }
    }
}
