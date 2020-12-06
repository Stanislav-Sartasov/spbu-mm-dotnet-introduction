using System;
using Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AccessorGeneratorTest
{
    [TestClass]
    public class AccessorGeneratorTestClass
    {
        [TestMethod]
        public void PathContainsOnePropertyTest()
        {
            string pathToName = "Test.Message";
            Func<TestClass, string> accessor = AccessorGenerator.GenerateAccessor<TestClass, string>(pathToName);

            TestClass test1 = new TestClass() { Message = "Test Message 1" };
            TestClass test2 = new TestClass() { Message = "Test Message 2" };

            string message1 = accessor(test1);
            string message2 = accessor(test2);

            Assert.AreEqual("Test Message 1", message1);
            Assert.AreEqual("Test Message 2", message2);
        }

        [TestMethod]
        public void PathContainsMultiplePropertiesTest()
        {
            string pathToName = "Test.A.B.C.Message";
            Func<TestClass, string> accessor = AccessorGenerator.GenerateAccessor<TestClass, string>(pathToName);

            TestClass test1 = new TestClass()
            {
                A = new A()
                {
                    B = new B()
                    {
                        C = new C()
                        {
                            Message = "We need to go deeper!"
                        }
                    }
                }
            };

            TestClass test2 = new TestClass()
            {
                A = new A()
                {
                    B = new B()
                    {
                        C = new C()
                    }
                }
            };

            TestClass test3 = new TestClass();

            string message1 = accessor(test1);
            string message2 = accessor(test2);
            string message3 = accessor(test3);

            Assert.AreEqual("We need to go deeper!", message1);
            Assert.AreEqual(null, message2);
            Assert.AreEqual(null, message3);
        }

        [TestMethod]
        public void PathContainsNoPropertiesTest()
        {
            string pathToName = "Test";
            Assert.ThrowsException<ArgumentException>(() => AccessorGenerator.GenerateAccessor<TestClass, string>(pathToName));

            pathToName = string.Empty;
            Assert.ThrowsException<ArgumentException>(() => AccessorGenerator.GenerateAccessor<TestClass, string>(pathToName));
        }

        [TestMethod]
        public void PathContainsInvalidPropertiesTest()
        {
            string pathToName = "Test.A.D.C";
            Assert.ThrowsException<ArgumentException>(() => AccessorGenerator.GenerateAccessor<TestClass, string>(pathToName));
        }

        private class TestClass
        {
            public A A { get; set; }
            public string Message { get; set; }
        }

        private class A
        {
            public B B { get; set; }
            public string Message { get; set; }
        }

        private class B
        {
            public C C { get; set; }
            public string Message { get; set; }
        }

        private class C
        {
            public string Message { get; set; }
        }
    }
}
