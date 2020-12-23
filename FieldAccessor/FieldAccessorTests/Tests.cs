using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using FieldAccessor;

namespace FieldAccessorTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void FieldAccessorTest()
        {
            var func = FieldAccessor.FieldAccessor.GetSaveField();

            var strangeStruct = new Dictionary<int, List<Queue<int>>>();
            Assert.IsNull(func(strangeStruct, 0, 0));

            strangeStruct.Add(0, new List<Queue<int>>());
            Assert.IsNull(func(strangeStruct, 0, 0));

            strangeStruct[0].Add(new Queue<int>());
            Assert.IsNull(func(strangeStruct, 0, 0));

            strangeStruct[0][0].Enqueue(2015);
            Assert.AreEqual(2015, func(strangeStruct, 0, 0));

            Assert.IsNull(func(strangeStruct, 0, 0));
        }
    }
}
