using System;
using App;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new AppBase();
            app.Start("CalculatorImpl");
        }
    }
}
