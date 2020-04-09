using System;
using System.IO;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using WebApi.Archive.ArchiveFiles;
using WebApi.Archive.BrigadeHistory;
using WebApi.Configuration;
using WebApi.ValueType;


namespace WebApiTest.Test
{
    public class ArchiveFileFactoryTest
    {
        private ArchiveFileGenerator _generator;
        private readonly int?[] brigadeSequence = {2, null, 1};
        private static readonly DateTime dt1 = new DateTime(2018, 12, 11, 2, 3, 4);
        private readonly string dt1Str = dt1.ToString(ArchiveFileGenerator.FileNameDateFormat);
        private const string acrJson = "arc/json";
        private const string acrVideo = "arc/video";
        private readonly DeviceSerialNumber serial = new DeviceSerialNumber(new ushort[] { 1, 2, 3, 4 });
        private IBrigadeHistory brigadeHistory;

        string JsonFile(string date, DeviceSerialNumber? serial = default)
        {
            string res = serial == null
                ? FileChannelJson.GetEmptySerialFile(date)
                : FileChannelJson.GetStringFile(date, serial.Value);
            return Path.Combine(acrJson, res);
        }

        string VideoFile(string date, int? duration, DeviceSerialNumber? serial = null)
        {
            string res = serial == null
                ? FileVideoMp4.GetEmptySerialFile(date, duration)
                : FileVideoMp4.GetStringFile(date, serial.Value, duration);
            return Path.Combine(acrVideo, res);
        }

        private IArchiveConfig GetFakeConfig()
        {
            var config = A.Fake<IArchiveConfig>();
            A.CallTo(() => config.ChannelArchivePath)
                .Returns(acrJson);
            A.CallTo(() => config.VideoArchivePath)
                .Returns(acrVideo);
            return config;
        }

        private IBrigadeHistoryRep GetFakeBrigadeHistoryRep()
        {
            brigadeHistory = A.Fake<IBrigadeHistory>();
            A.CallTo(() => brigadeHistory.GetBrigadeCode(DateTime.MinValue))
                .WithAnyArguments()
                .ReturnsNextFromSequence(brigadeSequence);
            var brigHistoryRep = A.Fake<IBrigadeHistoryRep>();
            A.CallTo(() => brigHistoryRep.GetBrigadeHistory())
                .Returns(brigadeHistory);
            return brigHistoryRep;
        }

        [SetUp]
        public void Setup()
        {
            var config = GetFakeConfig();
            var brigadeHistoryRep = GetFakeBrigadeHistoryRep();
            _generator = ArchiveFileGenerator.Create(brigadeHistory, config);
        }

        #region Json

        [Test]
        public void JsonCorrect_SimpleSequence()
        {
            var file = JsonFile(dt1Str, serial);
            foreach (var brig in brigadeSequence)
            {
                var f = _generator.CreteJson(file);
                Assert.AreEqual(f.pdt, dt1);
                Assert.AreEqual(f.serialNumber, serial);
                Assert.AreEqual(f.fullArchiveName, file);
                Assert.AreEqual(f.brigade, brig);
            }
        }

        [Test]
        public void JsonCorrect_EmptyDevice()
        {
            DeviceSerialNumber emptySerial = default;
            var file = Path.Combine(acrJson, JsonFile(dt1Str));
            var f = _generator.CreteJson(file);
            Assert.AreEqual(f.pdt, dt1);
            Assert.AreEqual(f.serialNumber, emptySerial);
            Assert.AreEqual(f.fullArchiveName, file);
            Assert.AreEqual(f.brigade, brigadeSequence.First());
        }

        [Test]
        public void JsonIncorrect_Empty()
        {
            Assert.Throws<FormatException>(() => _generator.CreteJson(string.Empty));
        }

        [Test]
        public void JsonIncorrect_OnlyExtension()
        {
            Assert.Throws<FormatException>(() => _generator.CreteJson(FileChannelJson.Extension));
        }

        [Test]
        public void JsonIncorrect_BadDateTime()
        {
            // The format must be "yyyy.M.ddTHH.mm.ss"
            var file = JsonFile("14.12.2018T22.14.18", serial);
            Assert.Throws<FormatException>(() => _generator.CreteJson(file));
        }
        #endregion

        #region Video

        [Test]
        public void VideoCorrect_SimpleSequence()
        {
            int duration = 60;
            int cameraNumber = 1;
            var file = VideoFile(dt1Str, duration, serial);
            foreach (var brig in brigadeSequence)
            {
                var f = _generator.CreateVideoMp4(file, cameraNumber);
                Assert.AreEqual(f.pdt, dt1);
                Assert.AreEqual(f.serialNumber, serial);
                Assert.AreEqual(f.fullArchiveName, file);
                Assert.AreEqual(f.brigade, brig);
                Assert.AreEqual(f.durationSeconds, duration);
                Assert.AreEqual(f.cameraNumber, cameraNumber);
            }
        }

        [Test]
        public void VideoCorrect_EmptyDevice()
        {
            int duration = 60;
            int cameraNumber = 1;
            var file = VideoFile(dt1Str, duration);
            var f = _generator.CreateVideoMp4(file, cameraNumber);
            Assert.AreEqual(f.pdt, dt1);
            Assert.AreEqual(f.serialNumber, default(DeviceSerialNumber));
            Assert.AreEqual(f.fullArchiveName, file);
            Assert.AreEqual(f.durationSeconds, duration);
            Assert.AreEqual(f.cameraNumber, cameraNumber);
            Assert.AreEqual(f.brigade, brigadeSequence.First());
        }

        [Test]
        public void VideoCorrect_UncompletedEmptyDevice()
        {
            int cameraNumber = 1;
            DeviceSerialNumber? serial = null;
            var file = VideoFile(dt1Str, null);

            var f = _generator.CreateVideoMp4(file, cameraNumber);
            Assert.AreEqual(f.pdt, dt1);
            Assert.AreEqual(f.serialNumber, default(DeviceSerialNumber));
            Assert.AreEqual(f.fullArchiveName, file);
            Assert.AreEqual(f.durationSeconds, null);
            Assert.AreEqual(f.cameraNumber, cameraNumber);
            Assert.AreEqual(f.brigade, brigadeSequence.First());

            file = VideoFile($"{dt1Str}", 1);
            f = _generator.CreateVideoMp4(file, cameraNumber);
            Assert.AreEqual(f.pdt, dt1);
            Assert.AreEqual(f.serialNumber, default(DeviceSerialNumber));
            Assert.AreEqual(f.fullArchiveName, file);
            Assert.AreEqual(f.durationSeconds, 1);
            Assert.AreEqual(f.cameraNumber, cameraNumber);
            Assert.AreEqual(f.brigade, brigadeSequence[1]);
        }

        [Test]
        public void VideoCorrect_Uncompleted()
        {
            int cameraNumber = 1;
            var file = VideoFile(dt1Str, null, serial);
            var f = _generator.CreateVideoMp4(file, cameraNumber);
            Assert.AreEqual(f.pdt, dt1);
            Assert.AreEqual(f.serialNumber, serial);
            Assert.AreEqual(f.fullArchiveName, file);
            Assert.AreEqual(f.durationSeconds, null);
            Assert.AreEqual(f.cameraNumber, cameraNumber);
            Assert.AreEqual(f.brigade, brigadeSequence.First());
        }

        [Test]
        public void VideoIncorrect_Empty()
        {
            Assert.Throws<FormatException>(() => _generator.CreateVideoMp4(string.Empty, 1));
        }

        [Test]
        public void VideoIncorrect_OnlyExtension()
        {
            Assert.Throws<FormatException>(() => _generator.CreateVideoMp4(FileVideoMp4.Extension, 1));
        }

        [Test]
        public void VideoIncorrect_BadDateTime()
        {
            // The format must be "yyyy.M.ddTHH.mm.ss"
            var file = VideoFile("14.12.2018T22.14.18", 60, serial);
            Assert.Throws<FormatException>(() => _generator.CreateVideoMp4(file, 1));
        }

        #endregion
    }
}