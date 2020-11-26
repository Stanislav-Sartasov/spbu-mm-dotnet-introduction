using ICalculatorLibrary;
using System;
using System.IO;

namespace CalculatorLibrary
{
    public class TrueCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }
    }

    public class PhilosophyCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            return 42;
        }
    }

    public class UnsafeCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            File.WriteAllText(Path.Combine("..", "..", "unsafe.txt"), "42");
            Console.WriteLine("Unsafe Sum");
            return a - b;
        }
    }
}
