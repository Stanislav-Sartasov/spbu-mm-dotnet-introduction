using System;
using CalculatorInterface;

namespace CalculatorImpl
{
    public class Calculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Called from class: {GetType()}. Domain: {AppDomain.CurrentDomain.FriendlyName}");

            return a + b;
        }
    }
}
