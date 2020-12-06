using System;
using System.IO;
using System.Threading;

namespace ImplementationLib
{
    public static class GenericCalculator<T>
    {
        private static int MagicNum = 45;
        public static int Sum(int a, int b, int permutation)
        {
            Console.WriteLine($"Calling Sum of {typeof(T)} from {Thread.GetDomain().FriendlyName}");

            if (a + b == MagicNum)
            {
                using (var _ = File.Create("file.txt")) {}   
            }

            return a + b * permutation;
        }
    }
}