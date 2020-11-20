using System;
using System.Threading;
using AppDomains;

namespace Implementation
{
    public class Calculator1: MarshalByRefObject, ICalculator
    {
        public override string ToString()
        {
            return $"{Thread.GetDomain().FriendlyName} Calculator1:ICalculator impl";
        }

        public int Sum(int a, int b)
        {
            Console.Out.WriteLine($"{this} Sum({a}, {b})");
            return a + b;
        }
    }
}