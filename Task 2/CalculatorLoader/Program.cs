using CalculatorInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorLoader
{
    class Program
    {
        public const string CalculatorDllName = "CalculatorImplementation.dll";

        // Source: https://stackoverflow.com/questions/19001423/getting-path-to-the-parent-folder-of-the-solution-file-using-c-sharp
        public static string GetSolutionDirectoryPath()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }

            return directory.FullName ?? throw new DirectoryNotFoundException();
        }

        public static string GetAssemblyFilePath(string directory)
        {
            IEnumerable<string> files = Directory.GetFiles(directory, CalculatorDllName, SearchOption.AllDirectories);
            return files.First() ?? throw new FileNotFoundException();
        }


        public static void Main()
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

            int a = 20, b = 22;
            foreach ((ICalculator calculator, int index) in calculators.WithIndex())
            {
                int result = calculator.Sum(a, b);
                Console.WriteLine($"Computation result for {index} loaded calculator is {result}");
            }

            Console.ReadKey();
        }
    }
}
