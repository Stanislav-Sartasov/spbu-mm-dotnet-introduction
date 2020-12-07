using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Reflection;
using Api;
using System.Security.Permissions;
using System.Security;
using System.Security.Policy;
using System.IO;

namespace CalcLoader
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

    public class CalcLoader: MarshalByRefObject
    {
        public List<CalcDomainHolder> Load(string assemblyPath)
        {
            Console.WriteLine(assemblyPath);

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
                        ApplicationBase = Path.GetFullPath(Path.GetDirectoryName(assemblyPath))
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
}