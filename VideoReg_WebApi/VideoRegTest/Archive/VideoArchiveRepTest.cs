using System;
using System.IO;
using System.Linq;
using FakeItEasy;
using NUnit.Framework;
using WebApi.Archive;
using WebApi.Archive.ArchiveFiles;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;
using WebApi.ValueType;

namespace WebApiTest.Archive
{
    class VideoArchiveRepTest
    {
        VideoArchiveRep rep;
        private IVideoArchiveStructureStore cache;
        private IFileSystemService fs;
        private IVideoArchiveConfig config;
        private IArchiveFileGeneratorFactory _acrhiveGenerator;

        private const string Root = "root";
        private byte[] file1 = { 1, 2 };
        private byte[] file2 = { 2, 3, 4};
        readonly string fd1 = nameof(fd1);
        readonly string fd2 = nameof(fd2);

        void CreateMp4(DateTime pdt, string fullArchiveName) => 
            new FileVideoMp4(null, pdt, new DeviceSerialNumber(), fullArchiveName, 1, 60);

        static (DateTime dt, string f) GetFileInfo(int minute)
        {
            var dt = new DateTime(2018, 1, 1, 1, minute, 0);
            var file = Path.Combine(Root, $"{dt:yyyy.MM.ddTHH.mm.ss}_N_N_N_N_60.mp4");
            return (dt, file);
        }
        
        static readonly (DateTime dt, string f)[] filesInfo =
        {
            GetFileInfo(1),
            GetFileInfo(2),
            GetFileInfo(3),
        };

        private readonly FileVideoMp4[] files = filesInfo
            .Select(x => new FileVideoMp4(null, x.dt, new DeviceSerialNumber(), x.f, 1, 60))
            .ToArray();

        [SetUp]
        public void Setup()
        {
            cache = new VideoArchiveStructureStore();
            config = A.Fake<IVideoArchiveConfig>();
            A.CallTo(() => config.VideoArchivePath)
                .Returns(Root);

            fs = A.Fake<IFileSystemService>();
            var f1 = Path.Combine(Root, fd1);

            A.CallTo(() => fs.ReadFileToMemoryAsync(A<string>.Ignored))
                .Throws(new FileNotFoundException());
          
            A.CallTo(() => fs.ReadFileToMemoryAsync(f1))
                .Returns(new MemoryStream(file1));

            var f2 = Path.Combine(Root, fd2);
            A.CallTo(() => fs.ReadFileToMemoryAsync(f2))
                .Returns(new MemoryStream(file2));

            _acrhiveGenerator = A.Fake<IArchiveFileGeneratorFactory>();

            rep = new VideoArchiveRep(cache, fs, config, _acrhiveGenerator);
        }

        [Test]
        public void GetStructure_EmptyCache()
        {
            var res = rep.GetFullStructure(DateTime.MinValue);
            Assert.AreEqual(res, new FileVideoMp4[0]);
            res = rep.GetFullStructure(DateTime.MaxValue);
            Assert.AreEqual(res, new FileVideoMp4[0]);
        }

        [Test]
        public void GetStructure_GotDataFromCache()
        {
            cache.Set(files);
            var res = rep.GetFullStructure(DateTime.MinValue);
            Assert.AreEqual(res, files);
        }

        [Test]
        public void GetStructure_GotDataFromCacheFilter()
        {
            cache.Set(files);
            for (int i = 0; i < filesInfo.Length; i++)
            {
                var dt = filesInfo.Skip(i).First().dt;
                var res = rep.GetFullStructure(dt);
                Assert.AreEqual(res, files.Skip(i));
            }
        }

        //[Test]
        //public void ReadFile_1()
        //{
        //    var res = rep.GetVideoFileStreamAsync(fd1);
        //    Assert.AreEqual(file1, res.ToArray());
        //}

        //[Test]
        //public void ReadFile_2()
        //{
        //    var res = rep.GetVideoFileStreamAsync(fd2);
        //    Assert.AreEqual(file2, res.ToArray());
        //}

        //[Test]
        //public void ReadFile_NotExist()
        //{
        //    Assert.Throws<FileNotFoundException>(() => rep.GetVideoFileStreamAsync("NotExist"));
        //}
    }
}
