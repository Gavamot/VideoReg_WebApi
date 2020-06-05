using System;
using System.Text;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using WebApi.Archive.BrigadeHistory;
using WebApi.Services;

namespace WebApiTest
{
    public class BrigadeHistoryTest
    {
        private BrigadeHistory Empty;
        private BrigadeHistory OneRow;
        private BrigadeHistory TwoRows;
        private BrigadeHistory MultiRows;
        private BrigadeHistory WithHole;

        private readonly DateTime dt_start = new DateTime(2019, 12, 17,16,43,5);
        private readonly DateTime dt_1 = new DateTime(2019, 12, 18, 11, 24, 22);
        private readonly DateTime dt_2 = new DateTime(2019, 12, 19, 11, 21, 22);
        private readonly DateTime dt_last = new DateTime(2020, 3, 28, 11, 0, 12);
        private readonly ILogger log = A.Fake<ILogger>();

        private BrigadeHistory Bg(string text) =>
            new BrigadeHistory(text, new DateTimeService(), log);

        private string Text(params string[] str)
        {
            var res = new StringBuilder();
            foreach (var s in str)
            {
                res.Append(s);
                res.Append(Environment.NewLine);
            }
            return res.ToString();
        }

        private string GenerateLine(DateTime start, DateTime? end, int brigade)
        {
            string endStr = end.HasValue ? end.Value.ToString("yyyy-MM-ddTHH:mm:ss") : "NULL";
            return $"{start:yyyy-MM-ddTHH:mm:ss} {endStr} {brigade}{Environment.NewLine}";
        }

        [SetUp]
        public void Setup()
        {
            Empty = new BrigadeHistory("", new DateTimeService(), log);
            OneRow = Bg(Text(
                GenerateLine(dt_start, null, 1)
                ));
            TwoRows = Bg(Text(
                GenerateLine(dt_start, dt_1, 1),
                GenerateLine(dt_1, null, 2)
            ));
            MultiRows = Bg(Text(
                GenerateLine(dt_start, dt_1, 1),
                GenerateLine(dt_1, dt_2, 2),
                GenerateLine(dt_2, null, 1)
            ));
            WithHole = Bg(Text(
                GenerateLine(dt_start, dt_1, 1),
                GenerateLine(dt_2, null, 2)
            ));
        }

        [Test]
        public void EmptyTest()
        {
            Assert.AreEqual(Empty.GetBrigadeCode(dt_start), BrigadeHistory.EmptyBrigadeCode);
            Assert.AreEqual(Empty.GetBrigadeCode(dt_1), BrigadeHistory.EmptyBrigadeCode);
            Assert.AreEqual(Empty.GetBrigadeCode(dt_2), BrigadeHistory.EmptyBrigadeCode);
            Assert.AreEqual(Empty.GetBrigadeCode(dt_last), BrigadeHistory.EmptyBrigadeCode);
            Assert.AreEqual(Empty.GetBrigadeCode(DateTime.MinValue), BrigadeHistory.EmptyBrigadeCode);
            Assert.AreEqual(Empty.GetBrigadeCode(DateTime.MaxValue), BrigadeHistory.EmptyBrigadeCode);
            Assert.AreEqual(Empty.GetBrigadeCode(DateTime.Now), BrigadeHistory.EmptyBrigadeCode);
        }

        [Test]
        public void OneRowTest_Less()
        {
            var dt = dt_start - new TimeSpan(1);
            int? brigade = OneRow.GetBrigadeCode(dt);
            Assert.AreEqual(brigade, BrigadeHistory.EmptyBrigadeCode);
        }

        [Test]
        public void OneRowTest_More()
        {
            int? brigade = OneRow.GetBrigadeCode(DateTime.MaxValue);
            Assert.AreEqual(brigade, 1);
        }

        [Test]
        public void OneRowTest_Hit()
        {
            int? brigade = OneRow.GetBrigadeCode(dt_start);
            Assert.AreEqual(brigade, 1);
            brigade = OneRow.GetBrigadeCode(dt_start + TimeSpan.FromSeconds(1));
            Assert.AreEqual(brigade, 1);
        }

        [Test]
        public void TwoRowsTest_Hit()
        {
            int? brigade = TwoRows.GetBrigadeCode(dt_start);
            Assert.AreEqual(brigade, 1);
            brigade = TwoRows.GetBrigadeCode(dt_2);
            Assert.AreEqual(brigade, 2);
        }

        [Test]
        public void MultiRowsTest_Hit()
        {
            int? brigade = MultiRows.GetBrigadeCode(dt_start);
            Assert.AreEqual(brigade, 1);
            brigade = MultiRows.GetBrigadeCode(dt_2);
            Assert.AreEqual(brigade, 2);
            brigade = MultiRows.GetBrigadeCode(dt_start);
            Assert.AreEqual(brigade, 1);
        }

        [Test]
        public void WithHoleTest_Hit()
        {
            int? brigade = WithHole.GetBrigadeCode(dt_start);
            Assert.AreEqual(brigade, 1);
            brigade = WithHole.GetBrigadeCode(dt_1);
            Assert.AreEqual(brigade, 1);
            brigade = WithHole.GetBrigadeCode(dt_2);
            Assert.AreEqual(brigade, 2);
            brigade = WithHole.GetBrigadeCode(dt_1 + TimeSpan.FromSeconds(1));
            Assert.AreEqual(brigade, BrigadeHistory.EmptyBrigadeCode);
        }
    }
}