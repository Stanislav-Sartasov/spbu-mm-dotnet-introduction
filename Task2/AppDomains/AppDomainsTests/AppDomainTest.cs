using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using CalculatorLoader;
using InterfaceLib;

namespace AppDomainsTests
{
    [TestFixture]
    public class AppDomainTest
    {
        private static string _implementationName = "ImplementationLib";
        // Suppose, that we are in .... /AppDomainsTests/bin/Debug folder
        private static string _solutionFolderPath = Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        // Path to the implementation.dll assembly (must be specified explicitly)
        private static string _implementationPath = Path.Combine(_solutionFolderPath, _implementationName, "bin", "Debug", _implementationName + ".dll");

        private Impostor _impostor;
        private List<ICalculator> _calculators;
        
        [SetUp]
        public void PrepareLoader()
        {
            _impostor = new Impostor(_implementationPath);
            _calculators = _impostor.Calculators;
        }

        [TearDown]
        public void ReleaseLoader()
        {
            _impostor.Dispose();
        }
        
        [Test]
        public void TestImplAssemblyNotLoaded()
        {
            foreach (var assembly in Thread.GetDomain().GetAssemblies())
            {
                Assert.AreNotEqual(assembly.GetName().Name, _implementationName);
            }
        }
        
        [Test]
        public void TestFileSystemPermissions()
        {
            foreach (var calculator in _calculators)
            {
                Assert.Catch<System.Security.SecurityException>(() => calculator.Sum(42, 3));
            }
        }

        [Test]
        public void TestCalculatorsImplementations()
        {
            var results = new HashSet<int>();

            foreach (var calculator in _calculators)
            {
                results.Add(calculator.Sum(1, 2));
            }
            
            Assert.AreEqual(results.Count, _calculators.Count);
        }
    }
}

