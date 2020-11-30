using NUnit.Framework;
using SafePropertyAccess;

namespace SafePropertyAccessTests {
    public class SafePropertyAccessTest {
        private class X {
            private Y _y;

            public Y Y {
                get => _y;
                set => _y = value;
            }
        }

        private class Y {
            private int _z;

            public int Z {
                get => _z;
                set => _z = value;
            }
        }


        [Test]
        public void TestXYZ() {
            Y y = new Y {Z = 3};
            X x = new X {Y = y};

            var getProperty = PropertyAccessGenerator.Generate<X, int?>(new[] {"Y", "Z"});

            Assert.AreEqual(x.Y.Z, getProperty(x));
        }

        [Test]
        public void TestXYZWithNullY() {
            X x = new X {Y = null};

            var getProperty = PropertyAccessGenerator.Generate<X, int?>(new[] {"Y", "Z"});

            Assert.AreEqual(null, getProperty(x));
        }

        [Test]
        public void TestXYZWithNullX() {
            X x = null;

            var getProperty = PropertyAccessGenerator.Generate<X, int?>(new[] {"Y", "Z"});

            Assert.AreEqual(null, getProperty(x));
        }
    }
}