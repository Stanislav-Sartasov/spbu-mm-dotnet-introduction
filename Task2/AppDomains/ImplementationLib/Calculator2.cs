using System;
using InterfaceLib;

namespace ImplementationLib
{
    public class Calculator2: MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            return GenericCalculator<Calculator2>.Sum(a, b);
        }
    }
}