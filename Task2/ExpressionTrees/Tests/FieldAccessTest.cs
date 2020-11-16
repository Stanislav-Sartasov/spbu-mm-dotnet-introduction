using System;
using System.Collections.Generic;
using ExpressionTrees;
using NUnit.Framework;

namespace Tests
{
    public class FieldAccessTest
    {
        private static readonly int TestValue = 0x03432;
        
        private readonly TestStructN0 _testStructN0;
        
        private readonly TestClassN0 _testClassN0;
        private readonly TestClassN1 _testClassN1;
        private readonly TestClassN2 _testClassN2;

        private readonly TestClassN2 _testClassN2WithNullTestClassN0;
        private readonly TestClassN2 _testClassN2WithNullTestClassN1;

        private readonly FieldAccess<TestStructN0, int> _accessIntFromTestStructN0;
        private readonly FieldAccess<TestClassN0, int> _accessIntFromTestClassN0; 
        private readonly FieldAccess<TestClassN1, int> _accessIntFromTestClassN1; 
        private readonly FieldAccess<TestClassN2, int> _accessIntFromTestClassN2; 
        
        private readonly FieldAccess<TestClassN1, TestClassN0> _accessTestClassN0FromTestClassN1;
        private readonly FieldAccess<TestClassN2, TestClassN0> _accessTestClassN0FromTestClassN2;
        private readonly FieldAccess<TestClassN2, TestStructN0> _accessTestStructN0FromTestClassN2;

        public FieldAccessTest()
        {
            _testStructN0 = new TestStructN0(TestValue);
            _testClassN0 = new TestClassN0(TestValue);
            _testClassN1 = new TestClassN1(_testClassN0, _testStructN0);
            _testClassN2 = new TestClassN2(_testClassN1);
            
            _testClassN2WithNullTestClassN0 = new TestClassN2(new TestClassN1(null, _testStructN0));
            _testClassN2WithNullTestClassN1 = new TestClassN2(null);
            
            _accessIntFromTestStructN0 = new FieldAccess<TestStructN0, int>(new [] { "_intField" });
            _accessIntFromTestClassN0 = new FieldAccess<TestClassN0, int>(new [] { "_intField" });
            _accessIntFromTestClassN1 = new FieldAccess<TestClassN1, int>( new [] { "_TestClassN0", "_intField" } );
            _accessIntFromTestClassN2 = new FieldAccess<TestClassN2, int>( new [] { "_TestClassN1", "_TestClassN0", "_intField" } );
            
            _accessTestClassN0FromTestClassN1 = new FieldAccess<TestClassN1, TestClassN0>( new [] { "_TestClassN0" });
            _accessTestClassN0FromTestClassN2 = new FieldAccess<TestClassN2, TestClassN0>( new [] { "_TestClassN1", "_TestClassN0" });
            _accessTestStructN0FromTestClassN2 = new FieldAccess<TestClassN2, TestStructN0>( new [] { "_TestClassN1", "_TestStructN0" });
        }

        private struct TestStructN0
        {
            public int _intField;

            public TestStructN0(int intField)
            {
                _intField = intField;
            }
        }
        
        private class TestClassN0
        {
            public int _intField;

            public TestClassN0(int intField)
            {
                _intField = intField;
            }
        }
        
        private class TestClassN1
        {
            public TestClassN0 _TestClassN0;
            public TestStructN0 _TestStructN0;

            public TestClassN1(TestClassN0 testClassN0, TestStructN0 testStructN0)
            {
                _TestClassN0 = testClassN0;
                _TestStructN0 = testStructN0;
            }
        }

        private class TestClassN2
        {
            public TestClassN1 _TestClassN1;
            public TestClassN1 _NullTestClassN1 = null; 
            
            public TestClassN2(TestClassN1 testClassN1)
            {
                _TestClassN1 = testClassN1;
            }
        }

        [Test]
        public void TestStructN0Access()
        {
            Assert.AreEqual(TestValue, _accessIntFromTestStructN0.Access(_testStructN0));    
        }
        
        [Test]
        public void TestClassN0Access()
        {
            Assert.AreEqual(TestValue, _accessIntFromTestClassN0.Access(_testClassN0));
        }
        
        [Test]
        public void TestClassN1Access()
        {
            Assert.AreEqual(TestValue, _accessIntFromTestClassN1.Access(_testClassN1));
            Assert.AreEqual(_testClassN0, _accessTestClassN0FromTestClassN1.Access(_testClassN1));
        }
        
        [Test]
        public void TestClassN2Access()
        {
            Assert.AreEqual(TestValue, _accessIntFromTestClassN2.Access(_testClassN2));
            Assert.AreEqual(_testClassN0, _accessTestClassN0FromTestClassN2.Access(_testClassN2));
            Assert.AreEqual(_testStructN0, _accessTestStructN0FromTestClassN2.Access(_testClassN2));
            
            Assert.AreEqual(0, _accessIntFromTestClassN2.Access(null));
            Assert.AreEqual(null, _accessTestClassN0FromTestClassN2.Access(_testClassN2WithNullTestClassN0));
            Assert.AreEqual(null, _accessTestClassN0FromTestClassN2.Access(_testClassN2WithNullTestClassN1));
            Assert.AreEqual(default(TestStructN0), _accessTestStructN0FromTestClassN2.Access(_testClassN2WithNullTestClassN1));
        }
        
    }
}