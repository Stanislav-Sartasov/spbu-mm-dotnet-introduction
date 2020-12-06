using System;
using InterfaceLib;

namespace ImplementationLib
{
    public class Calculator: MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            return GenericCalculator<Calculator>.Sum(a, b, -3);
        }
    }
}