using System;
using InterfaceLib;

namespace ImplementationLib
{
    public class YetAnotherCalculator: MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            return GenericCalculator<YetAnotherCalculator>.Sum(a, b, 2);
        }
    }
}