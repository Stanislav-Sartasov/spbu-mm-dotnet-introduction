using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using Api;
using System.Security.Permissions;
using System.Security;
using System.Security.Policy;

namespace Main
{
    public class CalcLoader: MarshalByRefObject
    {
        public class CalcDomainHolder : MarshalByRefObject, IDisposable
        {
            private ICalculator calc;
            private AppDomain domain;

            public ICalculator Calc
            {
                get => calc;
            }

            public CalcDomainHolder(ICalculator calc, AppDomain domain)
            {
                this.calc = calc;
                this.domain = domain;
            }

            public void Dispose()
            {
                AppDomain.Unload(domain);
            }
        }

        public List<CalcDomainHolder> Load()
        {
            var assemblyPath = AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\Implementation\bin\Debug\Implementation.dll";

            var holders = new List<CalcDomainHolder>();

            Assembly assembly = Assembly.LoadFrom(assemblyPath);

            Console.WriteLine($"{AppDomain.CurrentDomain.FriendlyName} assemblies:");
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                Console.WriteLine(a);
            Console.WriteLine();

            foreach (var t in assembly.GetTypes())
            {
                if (t.GetInterfaces().Contains(typeof(ICalculator)))
                {
                    var evidence = new Evidence();
                    var permissionSet = new PermissionSet(PermissionState.None);
                    permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    var setup = new AppDomainSetup {
                        ApplicationBase = AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\Implementation\bin\Debug"
                    };

                    var domain = AppDomain.CreateDomain($"Domain of {t}", evidence, setup, permissionSet);
                    var calc = (ICalculator) domain.CreateInstanceAndUnwrap(assembly.FullName, t.FullName);
                    var holder = new CalcDomainHolder(calc, domain);

                    holders.Add(holder);


                }
            }

            return holders;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var sandbox = AppDomain.CreateDomain("SandBox");
            var calcLoader = (CalcLoader) sandbox.CreateInstanceAndUnwrap(typeof(CalcLoader).Assembly.FullName, typeof(CalcLoader).FullName);

            Console.WriteLine("Default app domain assemblies:");
            foreach (var a in Thread.GetDomain().GetAssemblies())
                Console.WriteLine(a);
            Console.WriteLine();

            var holders = calcLoader.Load();

            foreach (var holder in holders)
            {
                var calc = holder.Calc;

                Console.WriteLine($"Sum(2,2) = {calc.Sum(2, 2)}");
                
                holder.Dispose();
            }

            AppDomain.Unload(sandbox);
            
            Console.ReadKey();
        }
    }
}