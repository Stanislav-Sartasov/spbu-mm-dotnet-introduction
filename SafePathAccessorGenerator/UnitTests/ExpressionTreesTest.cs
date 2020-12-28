using ExpressionTrees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    internal class TestClassA<T>
    {
        public int number = 0;
        public string str = "String";
        public T[] list;

        public TestClassA(T[] list)
        {
            this.list = list;
        }
    }

    internal class TestClassB<T>
    {
        public int integer = 0;
        public string sample_string = "String";
        public T[] listOfItems;

        public TestClassB(T[] list)
        {
            this.listOfItems = list;
        }
    }




    [TestClass]
    public class ExpressionTreesTest
    {
        [TestMethod]
        public void TestOutOfRangeNull()
        {
            var nestedArrays = new TestClassA<TestClassB<int?>>(new[] { new TestClassB<int?>(new int?[] { 1, 2, null }), new TestClassB<int?>(new int?[] { 11, 22, 3 }), null });
            var accessGenerator = new AccessorGenerator<TestClassA<TestClassB<int?>>, int?>();

            var OutOfRangeAccessFunction1 = accessGenerator.generateAccessor(nestedArrays, "list[12].listOfItems[0]");
            var OutOfRangeAccessFunction2 = accessGenerator.generateAccessor(nestedArrays, "list[0].listOfItems[12]");

            Assert.AreEqual(null, OutOfRangeAccessFunction1(nestedArrays));
            Assert.AreEqual(null, OutOfRangeAccessFunction2(nestedArrays));


        }

        [TestMethod]
        public void TestCorrectPath()
        {
            var nestedArrays = new TestClassA<TestClassB<int?>>(new[] { new TestClassB<int?>(new int?[] { 1, 2, null }), new TestClassB<int?>(new int?[] { 11, 22, 3 }), null });
            var accessGenerator = new AccessorGenerator<TestClassA<TestClassB<int?>>, int?>();

            var SuccessfulAccessFunction1 = accessGenerator.generateAccessor(nestedArrays, "list[0].listOfItems[0]");
            var SuccessfulAccessFunction2 = accessGenerator.generateAccessor(nestedArrays, "list[1].listOfItems[2]");

            Assert.AreEqual(1, SuccessfulAccessFunction1(nestedArrays));
            Assert.AreEqual(3, SuccessfulAccessFunction2(nestedArrays));

        }

        [TestMethod]
        public void TestNullHandling()
        {
            var nestedArrays = new TestClassA<TestClassB<int?>>(new[] { new TestClassB<int?>(new int?[] { 1, 2, null }), new TestClassB<int?>(new int?[] { 11, 22, 3 }), null });
            var accessGenerator = new AccessorGenerator<TestClassA<TestClassB<int?>>, int?>();

            var NullFindingAccessFunction1 = accessGenerator.generateAccessor(nestedArrays, "list[2].listOfItems[0]");
            var NullFindingAccessFunction2 = accessGenerator.generateAccessor(nestedArrays, "list[0].listOfItems[2]");

            Assert.AreEqual(null, NullFindingAccessFunction1(nestedArrays));
            Assert.AreEqual(null, NullFindingAccessFunction2(nestedArrays));

        }

        [TestMethod]
        public void TestWrongPath()
        {
            var nestedArrays = new TestClassA<TestClassB<int?>>(new[] { new TestClassB<int?>(new int?[] { 1, 2, null }), new TestClassB<int?>(new int?[] { 11, 22, 3 }), null });
            var accessGenerator = new AccessorGenerator<TestClassA<TestClassB<int?>>, int?>();

            Assert.ThrowsException<KeyNotFoundException>(() => accessGenerator.generateAccessor(nestedArrays, "wrong_path[1]"));
            Assert.ThrowsException<KeyNotFoundException>(() => accessGenerator.generateAccessor(nestedArrays, "list[1].wrong_path[2]"));
            Assert.ThrowsException<KeyNotFoundException>(() => accessGenerator.generateAccessor(nestedArrays, "list.wrong_path[2]"));
            Assert.ThrowsException<KeyNotFoundException>(() => accessGenerator.generateAccessor(nestedArrays, "list.wrong_path[2]"));
            Assert.ThrowsException<KeyNotFoundException>(() => accessGenerator.generateAccessor(nestedArrays, ""));
        }
    }
}
