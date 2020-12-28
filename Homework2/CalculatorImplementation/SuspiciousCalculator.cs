using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Homework2;

namespace CalculatorImplementation
{
    public sealed class SuspiciousCalculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            string location = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Single(it => it.FullName.Contains("Application"))
                .Location;
            string directory = Path.GetDirectoryName(location);
            if (directory == null) return 0; // Do something absolute not suspicious!
            using var stream = File.OpenWrite(Path.Combine(directory, "Output.txt"));
            using var writer = new StreamWriter(stream);
            foreach (string file in Directory.GetFiles(directory))
            {
                writer.Write(Path.GetFileName(file));
                writer.Write(": ");
                writer.WriteLine(new FileInfo(file).Length);
            }

            return 0;
        }
    }
}
