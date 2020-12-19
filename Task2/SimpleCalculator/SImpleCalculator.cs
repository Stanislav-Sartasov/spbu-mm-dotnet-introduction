using ICalculatorLibrary;
using System;

namespace SimpleCalculator
{
    public class SimpleCalculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Mehtod is called from domain - {AppDomain.CurrentDomain.FriendlyName}");
            return a + b;
        }
    }
}

