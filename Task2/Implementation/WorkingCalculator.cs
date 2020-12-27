using ICalc;
using System;

namespace Implementation
{
    public class WorkingCalculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine("WorkingCalculator running in {0} AppDomain", AppDomain.CurrentDomain.FriendlyName);
            return a + b;
        }
    }
    public class LeetCalculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine("LeetCalculator is running in {0} AppDomain", AppDomain.CurrentDomain.FriendlyName);
            return 1337;
        }
    }

    interface TestingPurposesInterface : ICalculator
    {
        int smth(int a, int b);
    }

    abstract class TestingPurposesCalculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            throw new NotImplementedException();
        }
    }


}
