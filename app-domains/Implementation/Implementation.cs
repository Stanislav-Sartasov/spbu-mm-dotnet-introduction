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
            if (a + b == 666)
            {
                FileIOPermissionChecker.Check();
            }
            return a + b;
        }
    }

    public class YetAnotherCalculator : MarshalByRefObject, ICalculator
    {
        public int Sum(int a, int b)
        {
            Console.WriteLine($"Method Sum: {Thread.GetDomain().FriendlyName}");
            if (a + b == 666)
            {
                FileIOPermissionChecker.Check();
            }
            return a + b;
        }
    }

    public class FileIOPermissionChecker
    {
        public static void Check()
        {
            var file = new FileStream("hi.txt", FileMode.Create);
        }
    }
}
