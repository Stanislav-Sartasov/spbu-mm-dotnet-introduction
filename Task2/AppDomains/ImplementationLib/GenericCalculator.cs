using System;
using System.IO;
using System.Threading;

namespace ImplementationLib
{
    public static class GenericCalculator<T>
    {
        public static int Sum(int a, int b)
        {
            Console.WriteLine($"Calling Sum of {typeof(T)} from {Thread.GetDomain().FriendlyName}");
            
            try
            {
                using (var file = File.Create("file.txt")) {}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return a + b;
        }
    }
}