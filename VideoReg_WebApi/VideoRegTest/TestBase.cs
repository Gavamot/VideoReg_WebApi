using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;

namespace WebApiTest
{
    class TestBase
    {
        public V GetNonPublicValue<T, V>(T obj, string filedName)
        {
            FieldInfo field = obj.GetType().GetField(filedName, BindingFlags.Instance | BindingFlags.NonPublic);
            return (V)field.GetValue(obj);
        }

        /// <summary>
        /// SetNonPublicValue
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filedName"></param>
        /// <param name="value"></param>
        /// <exception cref="FieldAccessException"></exception>
        /// <exception cref="TargetException"></exception>
        public void SetNonPublicValue<T>(T obj, string filedName, object value)
        {
            FieldInfo field = obj.GetType().GetField(filedName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(obj, value);
        }

        public string ReadTestFile(string file)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, file);
            string res = File.ReadAllText(path);
            return res;
        }

        /// <summary>
        /// ReadTestFileBytes
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public byte[] ReadTestFileBytes(string file)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, file);
            var res = File.ReadAllBytes(path);
            return res;
        }

        public void AssertEqualJson<T1, T2>(T1 obj1, T2 obj2)
        {
            if (obj1 == null || obj2 == null)
            {
                Assert.AreEqual(obj1, obj2);
            }
            Assert.AreEqual(JsonConvert.SerializeObject(obj1), JsonConvert.SerializeObject(obj2));
        }
    }
}
