using System;
using InterfaceLib;

namespace ImplementationLib
{
    public class Calculator1: MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            return GenericCalculator<Calculator1>.Sum(a, b);
        }
    }
}