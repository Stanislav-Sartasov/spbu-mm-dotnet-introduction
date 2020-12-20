using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NullSafety;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        private class X
        {
            public Y YProperty { get; set; }
        }

        private class Y
        {
            public Z ZProperty { get; set; }
        }

        private class Z
        {
            public string ZString { get; set; }
        }


        [TestMethod]
        public void AllPropertyNotNull()
        {
            X MyClass = new X()
            {
                YProperty = new Y()
                {
                    ZProperty = new Z()
                    {
                        ZString = "This is my property"
                    }
                }
            };

            Func<X, string> function = NullSafety.NullSafety.SafeGetProperty<X, string>(new List<string> {"MyClass", "YProperty", "ZProperty", "ZString" });
            string result = function(MyClass);
            Assert.AreEqual(result, "This is my property");
        }

        [TestMethod]
        public void MiddlePropertyIsNull()
        {
            X MyClass = new X()
            {
                YProperty = null
            };

            Func<X, string> function = NullSafety.NullSafety.SafeGetProperty<X, string>(new List<string> { "MyClass", "YProperty", "ZProperty", "ZString" });
            string result = function(MyClass);
            Assert.AreEqual(result, null);
        }
    }
}
