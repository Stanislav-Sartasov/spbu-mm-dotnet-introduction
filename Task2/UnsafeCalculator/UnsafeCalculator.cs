using ICalculatorLibrary;
using System;
using System.IO;

namespace UnsafeCalculator
{
    public class UnsafeCalculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Method is called from domain - {AppDomain.CurrentDomain.FriendlyName}");

            // there can be any malicious action
            FileStream fileStream = File.Create("ResultUnsafeCalculator.txt");
            StreamWriter streamWriter = new StreamWriter(fileStream);
            streamWriter.WriteLine("Welcome to unsafe calculator heh :)");
            streamWriter.Dispose();
            fileStream.Dispose();

            return a + b;
        }
    }
}
