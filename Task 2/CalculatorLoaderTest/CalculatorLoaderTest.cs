using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalculatorInterface;
using Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculatorLoaderTest
{
    [TestClass]
    public class CalculatorLoaderTestClass
    {
        private const string CalculatorDllName = "CalculatorImplementation.dll";

        // Source: https://stackoverflow.com/questions/19001423/getting-path-to-the-parent-folder-of-the-solution-file-using-c-sharp
        private static string GetSolutionDirectoryPath()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return directory.FullName ?? throw new DirectoryNotFoundException();
        }

        private static string GetAssemblyFilePath(string directory)
        {
            IEnumerable<string> files = Directory.GetFiles(directory, CalculatorDllName, SearchOption.AllDirectories);
            return files.First() ?? throw new FileNotFoundException();
        }

        [TestMethod]
        public void CalculatorLoaderTest()
        {
            string solutionPath;
            try
            {
                solutionPath = GetSolutionDirectoryPath();
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Unable to acquire solution directory path");
                return;
            }

            string assemblyPath;
            try
            {
                assemblyPath = GetAssemblyFilePath(solutionPath);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"{CalculatorDllName} not found");
                return;
            }
            catch (Exception)
            {
                Console.WriteLine($"Unexpected error occured during {CalculatorDllName} search");
                return;
            }

            using CalculatorLoader loader = new CalculatorLoader();
            IEnumerable<ICalculator> calculators = loader.LoadCalculators(new[] { assemblyPath });

            Assert.IsTrue(calculators.Any(calc => calc.CalculatorName == "Regular Sum Calculator"));
            Assert.IsTrue(calculators.Any(calc => calc.CalculatorName == "Sum Modulo 42 Calculator"));
            Assert.IsFalse(calculators.Any(calc => calc.CalculatorName == "Fake Calculator"));

            int a = 20, b = 22;
            IEnumerable<int> sums = calculators.Select(calc => calc.Sum(a, b));

            Assert.IsTrue(sums.Contains(a + b));
            Assert.IsTrue(sums.Contains(a + b % 42));
        }
    }
}
