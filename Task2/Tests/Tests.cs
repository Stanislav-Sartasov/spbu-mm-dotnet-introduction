using System;
using System.IO;
using System.Security;
using Application;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Tests
    {
        private string _calcLibPath = Path.Combine(new[]
        {
            Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.Parent.FullName,
            "CalculatorLib", "bin", "Debug"
        });

        private readonly string _assemblyLibName = "CalculatorLib";
        private readonly string _typeNameTrueCalculator = "CalculatorLib.TrueCalculator";
        private readonly string _typeNamePhylosophyCalculator = "CalculatorLib.PhilosophyCalculator";
        private readonly string _typeNameUnsageCalculator = "CalculatorLib.UnsafeCalculator";

        [Test]
        public void AppDomainUnsafeReadFilePreventTest()
        {
            var appDomainUnsafeCalculator = Program.CreatAppDomainRestriced(_calcLibPath, "UnsafeCalculator");
            try
            {
                var ex = Program.SumInAppDomain(appDomainUnsafeCalculator, _assemblyLibName, _typeNameUnsageCalculator, 12, 13);
            } catch (SecurityException)
            {
                Assert.Pass();
            }

            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Assert.AreEqual(true, assembly.GetName().Name != _assemblyLibName);
            }
        }

        [Test]
        public void AppDomainSeveralDomainsTest()
        {
            var appDomainPhylosophyCalc = Program.CreatAppDomainRestriced(_calcLibPath, "PhylosophyCalculator");
            var appDomainTrueCalc = Program.CreatAppDomainRestriced(_calcLibPath, "TrueCalculator");
            var resPh = Program.SumInAppDomain(appDomainPhylosophyCalc, _assemblyLibName, _typeNamePhylosophyCalculator, 12, 13);
            var resTr = Program.SumInAppDomain(appDomainTrueCalc, _assemblyLibName, _typeNameTrueCalculator, 12, 13);

            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Assert.AreEqual(true, assembly.GetName().Name != _assemblyLibName);
            }

            Assert.AreEqual(resPh, 42);
            Assert.AreEqual(resTr, 25);
        }

    }
}
