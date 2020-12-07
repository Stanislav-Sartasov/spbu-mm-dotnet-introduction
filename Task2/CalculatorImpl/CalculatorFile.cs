using System;
using System.IO;
using CalculatorInterface;

namespace CalculatorImpl
{
    public class CalculatorFile : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Sum(..) is called from: {GetType()}. Domain: {AppDomain.CurrentDomain.FriendlyName}");

            using (FileStream fs = File.Create("CalculatorFile.txt"))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine("Sum");
                }
            }

            return a + b;
        }
    }
}
