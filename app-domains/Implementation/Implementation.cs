using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api;

namespace Implementation
{
    public class Calculator: MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Method Sum: {Thread.GetDomain().FriendlyName}");
            FileIOPermissionChecker.check();
            return a + b;
        }
    }

    public class YetAnotherCalculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Method Sum: {Thread.GetDomain().FriendlyName}");
            FileIOPermissionChecker.check();
            return a + b;
        }
    }

    public class FileIOPermissionChecker
    {
        public static void check()
        {
            try
            {
                var file = new FileStream("hi.txt", FileMode.Create);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't read a file: {e.GetType()}");
            }
        }
    }
}
