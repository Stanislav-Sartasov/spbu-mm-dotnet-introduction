using CalculatorInterface;
using System;

namespace CalculatorImplementation
{
    public class ModuloCalculator : MarshalByRefObject, ICalculator
    {
        public string CalculatorName => "Sum Modulo 42 Calculator";

        public int Sum(int a, int b)
        {
            return (a + b) % 42;
        }
    }
}
