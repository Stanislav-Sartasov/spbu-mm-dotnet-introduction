using ICalc;
using ConsumerN;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace ConsumerUnitTest
{
    [TestClass]
    public class ConsumerUnitTestClass
    {
        [TestMethod]
        public void CheckLoadUnload()
        {
            var initial_path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent;

            string dll_path = initial_path.FullName + "\\Implementation\\bin\\Debug\\Implementation.dll";

            Assert.IsTrue(File.Exists(dll_path), $"Nothing found in path: {dll_path}");

            var consumer = new Consumer();


            consumer.AddImplementations(dll_path);

            var implementations = consumer.Implementations;

            var values = implementations.Select(i => i.calculator.Sum(1, 2));
            Assert.IsTrue(values.Contains(3) & values.Contains(1337), "Some calculators are not loaded");

            var domains = implementations.Select(i => i.domain);
            Assert.IsTrue(domains.Count() == domains.Distinct().Count(), "Some calculators are loaded in the same domain");

            var implemenation = consumer.Implementations.First();
            consumer.UnloadImplementation(implemenation);
            Assert.ThrowsException<System.AppDomainUnloadedException>(
                () => implemenation.calculator.Sum(1, 2),
                "Unloading calculators does not work");
        }

    }
}

