using FieldNullability;
using System.Collections.Generic;
using Xunit;
using System;

namespace Testing {

    public class Tests
    {

        public class A 
        {
            public B B;

            public A(B B)
            {
                this.B = B;
            }
        } 

        public class B 
        {
            public int C;

            public B(int C)
            {
                this.C = C;
            }
        }

        static List<string> path = new List<string>{ "B", "C" };
        static Func<A, int?> getter = DelegateFactory.createSafeGetterDelegate<A, int?>(path);

        [Fact]
        public void SuccessTest()
        {
            var b = new B(2);
            var a = new A(b);
            var result = getter.Invoke(a);
            Assert.Equal(2, result);
        }

        [Fact]
        public void FailureTest1() 
        {
            B b = null;
            var a = new A(b);
            var result = getter.Invoke(a);
            Assert.Null(result);
        }

        [Fact]
        public void FailureTest2() 
        {
            B b = new B(2);
            A a = null;
            var result = getter.Invoke(a);
            Assert.Null(result);
        }
    }
}