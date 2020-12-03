using CalculatorInterface;
using System;

namespace CalculatorImplementation
{
    public class NormalCalculator : MarshalByRefObject, ICalculator
    {
        public string CalculatorName => "Regular Sum Calculator";

        public int Sum(int a, int b)
        {
            return a + b;
        }
    }
}
