using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FakeItEasy;
using NUnit.Framework;
using WebApi.Collection;
using WebApi.Services;

namespace WebApiTest
{
    class TimestampConcurrentDictionaryTest
    {
        private TimestampConcurrentDictionary<int, string> collection;
        private const int key1 = 0;
        private const int key2 = 1;

        private const string value1 = "v1";
        private const string value2 = "v2";
        private readonly string[] Values =
        {
            value1,
            value2
        };

        readonly DateTime[] Dates =
        {
            new DateTime(2000, 1,1,1,1,1,100),
            new DateTime(2000, 1,1,1,1,1,300),
            new DateTime(2000, 1,1,1,1,1,700),
            new DateTime(2000, 1,1,1,1,1,900),
        };

        private IDateTimeService dateTimeService;
        TimestampKeyValuePair<int, string> GetKeyValuePair(int dateIndex, int key, string value ) => new TimestampKeyValuePair<int, string>(Dates[dateIndex], key, value);
        TimestampKeyValuePair<int, string> GetKeyValuePair(int index) => new TimestampKeyValuePair<int, string>(Dates[index], index, Values[index]);
        TimestampValue<string> GeTimestampValue(int dateIndex, string value) => new TimestampValue<string>(Dates[dateIndex], value);

        [SetUp]
        public void Init()
        {
            dateTimeService = A.Fake<IDateTimeService>();
            A.CallTo(() => dateTimeService.GetNow()).ReturnsNextFromSequence(Dates);
            collection = new TimestampConcurrentDictionary<int, string>(dateTimeService);
        }

        [Test]
        public void TryGet_Empty()
        {
            var actual = collection.TryGet(key1, out var value);
            Assert.IsFalse(actual);
        }

        [Test]
        public void TryGet_AfterAdd()
        {
            collection.AddOrUpdate(key1, value1);
            if (!collection.TryGet(key1, out var value))
            {
                Assert.Fail("TryGet must return an added item");
            }
            var actual = GeTimestampValue(key1, value1);
            Assert.AreEqual(value, actual);
        }

        [Test]
        public void TryGet_AfterUpdate()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key1, value2);
            var res = collection.TryGet(key1, out var value);
            Assert.True(res);
            Assert.AreEqual(value, GeTimestampValue(1, value2));
        }

        [Test]
        public void TryGet_AfterUpdateRepeat()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key1, value2);
            collection.AddOrUpdate(key1, value2);
            var res = collection.TryGet(key1, out var value);
            Assert.True(res);
            Assert.AreEqual(value, GeTimestampValue(2, value2));
        }

        [Test]
        public void AddUpdate_Add()
        {
            var res = collection.AddOrUpdate(key1, value1);
            Assert.IsTrue(res);
        }


        [Test]
        public void AddUpdate_Update()
        {
            collection.AddOrUpdate(key1, value1);
            var res = collection.AddOrUpdate(key1, value2);
            Assert.IsFalse(res);
        }

        [Test]
        public void AddUpdate_UpdateRepeat()
        {
            collection.AddOrUpdate(key1, value1);
            var res = collection.AddOrUpdate(key1, value2);
            res = collection.AddOrUpdate(key1, value2);
            Assert.IsFalse(res);

        }

        [Test]
        public void GetAll_Empty()
        {
            var res = collection.GetAll();
            Assert.IsEmpty(res);
        }

        [Test]
        public void GetAll_Case1()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key2, value2);
            collection.AddOrUpdate(key1, value2);
            var res = collection.GetAll().ToArray();
            Assert.AreEqual(res.Length, 2);
            Assert.Contains(GetKeyValuePair(2, key1, value2), res);
        }

        [Test]
        public void TryGetActual_Empty()
        {
            if (collection.TryGetActual(key1, TimeSpan.FromMilliseconds(1000), out var value))
            {
                Assert.Fail("The collection must have no any elements");
            }
        }


        [Test]
        public void TryGetActual_Simple()
        {
            collection.AddOrUpdate(key1, value1);
            if (!collection.TryGetActual(key1, TimeSpan.FromMilliseconds(1000), out var value))
            {
                Assert.Fail("The collection must have no any elements");
            }
            Assert.AreEqual(value, GeTimestampValue(0, value1));
        }

        [Test]
        public void TryGetActual_Updated()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key1, value2);
            if (!collection.TryGetActual(key1, TimeSpan.FromMilliseconds(1000), out var value))
            {
                Assert.Fail("The collection must have no any elements");
            }
            Assert.AreEqual(value, GeTimestampValue(1, value2));
        }

        [Test]
        public void TryGetActual_FalseCase()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key1, value2);
            var lessTime = (Dates[1] - Dates[0]) - TimeSpan.FromMilliseconds(1);
            if (collection.TryGetActual(key1, lessTime, out var value))
            {
                Assert.Fail("The item must be deprecated");
            }
            Assert.IsTrue(value == null);
        }

        [Test]
        public void GetAllActual_Empty()
        {
            var res = collection.GetAllActual(TimeSpan.MaxValue);
            Assert.IsEmpty(res);
        }

        [Test]
        public void GetAllActual_Simple()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key2, value2);
            var res = collection.GetAllActual(TimeSpan.MaxValue);
            Assert.IsTrue(res.Count() == 2);
        }

        [Test]
        public void GetAllActual_Repeat()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key2, value2);
            collection.AddOrUpdate(key1, value2);
            var res = collection.GetAllActual(TimeSpan.MaxValue);
            Assert.IsTrue(res.Count() == 2);
        }

        [Test]
        public void GetAllActual_OneDeprecated()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key2, value2);
            var res = collection.GetAllActual(TimeSpan.FromMilliseconds(400));
            Assert.IsTrue(res.Count() == 1);

        }

        [Test]
        public void TryGetValue_Simple()
        {
            collection.AddOrUpdate(key1, value1);
            if (!collection.TryGetValue(key1, out var value))
            {
                Assert.Fail();
               
            }
            Assert.AreEqual(value, value1);
        }

        [Test]
        public void TryGetActualValue_Simple()
        {
            collection.AddOrUpdate(key1, value1);
            var lessTime = (Dates[1] - Dates[0]) + TimeSpan.FromMilliseconds(1);
            if (!collection.TryGetActualValue(key1, lessTime, out var value))
            {
                Assert.Fail();
               
            }
            Assert.AreEqual(value, value1);
        }

        [Test]
        public void GetKeys_Simple()
        {
            collection.AddOrUpdate(key1, value1);
            var res = collection.GetKeys().ToArray();
            Assert.IsTrue(res.Length == 1);
            Assert.Contains(key1, res);
        }

        [Test]
        public void GetKeys_Repeatable()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key2, value1);
            collection.AddOrUpdate(key1, value2);
            var res = collection.GetKeys().ToArray();
            Assert.IsTrue(res.Length == 2);
            Assert.Contains(key1, res);
            Assert.Contains(key2, res);
        }

        [Test]
        public void GetActualKeys_Simple()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key2, value1);
            var res = collection.GetActualKeys(TimeSpan.MaxValue).ToArray();
            Assert.IsTrue(res.Length == 2);
            Assert.Contains(key1, res);
        }

        [Test]
        public void GetActualKeys_Repeatable()
        {
            collection.AddOrUpdate(key1, value1);
            collection.AddOrUpdate(key2, value1);
            collection.AddOrUpdate(key1, value2);
            var lessTime = TimeSpan.FromMilliseconds(201);
            var res = collection.GetActualKeys(lessTime).ToArray();
            Assert.IsTrue(res.Length == 1);
            Assert.Contains(key1, res);
        }
    }
}
