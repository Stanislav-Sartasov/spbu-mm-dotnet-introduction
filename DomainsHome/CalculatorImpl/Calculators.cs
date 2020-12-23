using System;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using CalculatorInterfaceLib;

namespace CalculatorImpl
{
    public class BaseCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine("Running Base calculator");
            Console.WriteLine(Thread.GetDomain().FriendlyName);
            return a + b;
        }
    }

    class MaxCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine("Running Max calculator");
            Console.WriteLine(Thread.GetDomain().FriendlyName);
            return Math.Max(a, b);
        }
    }

    class FileCalculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine("Running File calculator");
            Console.WriteLine(Thread.GetDomain().FriendlyName);

            try
            {
                var input = File.ReadAllText("somefile.txt");
            } 
            catch (FileNotFoundException)
            {
                Console.WriteLine("file not found");
                return 0;
            }

            return a + b;
        }
    }
}

