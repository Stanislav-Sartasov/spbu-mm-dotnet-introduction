using System;
using System.IO;
using CalculatorInterface;

namespace CalculatorImpl
{
    public class CalculatorFile : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Called from class: {GetType()}. Domain: {AppDomain.CurrentDomain.FriendlyName}");

            try
            {
                using (FileStream fs = File.Create("CalculatorFile.txt"))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine("Sum");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in Sum(..): {e.Message}");
            }

            return a + b;
        }
    }
}
