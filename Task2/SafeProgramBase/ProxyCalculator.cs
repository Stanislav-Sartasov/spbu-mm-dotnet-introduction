using ICalculatorLibrary;
using System;
using System.Reflection;

namespace SafeProgram
{
    public class ProxyCalculator : MarshalByRefObject, ICalculator
    {
        public string MyAssemblyName { get; set; }
        public string MyTypeName { get; set; }
        public AppDomain MyDomain => AppDomain.CurrentDomain;

        public int Sum(int a, int b)
        {
            Console.WriteLine($"ProxyCalculator.Sum with assembly {MyAssemblyName}," +
                $" type {MyTypeName} and domain {AppDomain.CurrentDomain.FriendlyName}");
            var type = Assembly.Load(MyAssemblyName).GetType(MyTypeName);
            var constructor = type.GetConstructor(new Type[] { });
            var calculator = (ICalculator)constructor.Invoke(new object[] { });
            return calculator.Sum(a, b);
        }
    }
}
