using ICalculatorLibrary;
using System;

namespace CalculatorLibrary
{
    public class Calculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            return a + b;
        }
    }
}
