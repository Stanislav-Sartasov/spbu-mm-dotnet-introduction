using System;
using NUnit.Framework;
using static NullPropagating.NullPropagation;

namespace NullPropagating.Tests
{
public class NullPropagationTests
{
    [Test]
    public void TestBlankPath()
    {
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, string?>("")(null), 
            "Given path \"\" is blank");
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, string?>("  ")(null),
            "Given path \"  \" is blank");
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, string?>("")(null),
            "Given path \"  \" is blank");
    }

    [Test]
    public void TestOneComponentPath()
    {
        Assert.AreEqual("42", Wrap<C<string?>?, string?>("Item1")(C_<string?>("42")));
        Assert.AreEqual(null, Wrap<C<string?>?, string?>("Item1")(C_<string?>(null)));
        Assert.AreEqual(null, Wrap<C<string?>?, string?>("Item1")(null));
    }

    [Test]
    public void TestTwoComponentPath()
    {
        Assert.AreEqual("42", Wrap<C<C<string?>?>?, string?>("Item1.Item1")(C_(C_<string?>("42"))));
        Assert.AreEqual(null, Wrap<C<C<string?>?>?, string?>("Item1.Item1")(C_(C_<string?>(null))));
        Assert.AreEqual(null, Wrap<C<C<string?>?>?, string?>("Item1.Item1")(C_<C<string?>?>(null)));
        Assert.AreEqual(null, Wrap<C<C<string?>?>?, string?>("Item1.Item1")(null));
    }

    [Test]
    public void TestThreeComponentPath()
    {
        Assert.AreEqual("42", Wrap<C<C<C<string?>?>?>?, string?>("Item1.Item1.Item1")(C_(C_(C_<string?>("42")))));
        Assert.AreEqual(null, Wrap<C<C<C<string?>?>?>?, string?>("Item1.Item1.Item1")(C_(C_(C_<string?>(null)))));
        Assert.AreEqual(null, Wrap<C<C<C<string?>?>?>?, string?>("Item1.Item1.Item1")(C_(C_<C<string?>?>(null))));
        Assert.AreEqual(null, Wrap<C<C<C<string?>?>?>?, string?>("Item1.Item1.Item1")(C_<C<C<string?>?>?>(null)));
        Assert.AreEqual(null, Wrap<C<C<C<string?>?>?>?, string?>("Item1.Item1.Item1")(null));
    }

    [Test]
    public void TestWrongPath()
    {
        var errMsg = $"{typeof(C<string>)} does not contain field \"Item2\"";
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, string?>("Item1.Item1.Item2")(null), errMsg);
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, string?>("Item1.Item2.Item1")(null), errMsg);
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, string?>("Item2.Item1.Item1")(null), errMsg);
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, string?>("Item2.Item3.Item4")(null), errMsg);
    }

    [Test]
    public void TestWrongDestType()
    {
        var errMsg =
            $"Specified return type {typeof(int?)} is not a supertype of actual return type {typeof(string)}";
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, int?>("Item1.Item1.Item2")(null), errMsg);
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, int?>("Item1.Item2.Item1")(null), errMsg);
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, int?>("Item2.Item1.Item1")(null), errMsg);
        Assert.Throws<ArgumentException>(() => Wrap<C<C<C<string?>?>?>?, int?>("Item2.Item3.Item4")(null), errMsg);
    }

    private static C<T?>? C_<T>(T value) => new(value);

    private class C<T>
    {
        public readonly T Item1;

        public C(T item1)
        {
            Item1 = item1;
        }
    }
}
}