using System;
using Homework2;

namespace CalculatorImplementation
{
    public class Calculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b) => a + b;
    }
}
