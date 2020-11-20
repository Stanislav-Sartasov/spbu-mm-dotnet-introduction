using System;
using System.IO;
using System.Threading;
using Interface;

namespace Implementation
{
    public class Calculator2: MarshalByRefObject, ICalculator
    {
        public override string ToString()
        {
            return $"{Thread.GetDomain().FriendlyName} Calculator2:ICalculator impl";
        }

        public int Sum(int a, int b)
        {
            Console.Out.WriteLine($"{this} Sum({a}, {b})");
            
            try
            {
                var file = new FileStream("my_file.txt", FileMode.Create);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return a + b;
        }
    }
}