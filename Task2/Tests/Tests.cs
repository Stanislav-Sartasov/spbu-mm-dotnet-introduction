using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeProgram;
using System;
using System.IO;
using System.Security;

namespace Tests
{
    [TestClass]
    public class Tests
    {
        private static readonly string myRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
        private static readonly string mySimpleCalcName = "SimpleCalculator";
        private static readonly string myUnsafeCalcName = "UnsafeCalculator";

        [TestMethod]
        public void SimpleCalculatorTest()
        {
            Console.WriteLine($"Current domain - {AppDomain.CurrentDomain.FriendlyName}");
            var path = SafeDomain.BuildPathToAssembly(mySimpleCalcName, myRoot);
            SafeDomain.Execute("Safe environment", env =>
            {
                foreach (var calc in env.LoadFromAssembly(path))
                {
                    Console.WriteLine($"Calculator domain - {calc.MyDomain.FriendlyName}");
                    try
                    {
                        var result = calc.Sum(2, 3);
                        Console.WriteLine($"Result = {result}");
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Expected no exception, but got - " + ex.Message);
                    }

                    AppDomain.Unload(calc.MyDomain);
                }
            });
        }

        [TestMethod]
        public void UnsafeCalculatorTest()
        {
            Console.WriteLine($"Current domain - {AppDomain.CurrentDomain.FriendlyName}");
            var path = SafeDomain.BuildPathToAssembly(myUnsafeCalcName, myRoot);
            SafeDomain.Execute("Safe environment", env =>
            {
                foreach (var calc in env.LoadFromAssembly(path))
                {
                    Console.WriteLine($"Calculator domain - {calc.MyDomain.FriendlyName}");
                    Func<int> action = () =>
                    {
                        var result = calc.Sum(2, 3);
                        Console.WriteLine($"Result = {result}");
                        return result;
                    };

                    Assert.ThrowsException<SecurityException>(() => action());
                }
            });
        }
    }
}
