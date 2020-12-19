using ICalculatorLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace SafeProgram
{
    public sealed class SafeDomain : MarshalByRefObject
    {
        // execute any action in safe environment
        public static void Execute(string name, Action<SafeDomain> action)
        {
            Console.WriteLine($"Create restricted domain '{name}'");
            AppDomain domain = null;
            try
            {
                var setup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                };
                domain = AppDomain.CreateDomain(name, null, setup);
                var guardian = (SafeDomain)domain.CreateInstanceFromAndUnwrap(typeof(SafeDomain).Assembly.Location, typeof(SafeDomain).FullName);

                Console.WriteLine("Run execution in safe environment");
                action(guardian);
            }
            finally
            {
                if (domain != null)
                    AppDomain.Unload(domain);
            }
        }

        public IEnumerable<ProxyCalculator> LoadFromAssembly(string path)
        {
            Console.WriteLine($"Current domain - {AppDomain.CurrentDomain.FriendlyName}");
            Console.WriteLine($"ProxyCalculator.LoadFromAssembly - {path}");
            var calculators = new List<ProxyCalculator>();
            var assembly = Assembly.LoadFrom(path);
            foreach (var type in assembly.DefinedTypes)
            {
                if (type.ImplementedInterfaces.Contains(typeof(ICalculator)))
                {
                    Console.WriteLine($"Loading calculator {type.FullName}");
                    var calculator = BuildInstanceInDomain<ProxyCalculator>(type.FullName, Path.GetDirectoryName(path));
                    calculator.MyTypeName = type.FullName;
                    calculator.MyAssemblyName = assembly.FullName;
                    calculators.Add(calculator);
                }
            }

            return calculators.ToArray();
        }

        public T BuildInstanceInDomain<T>(string name, string path)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = path
            };
           
            var permission = new PermissionSet(PermissionState.None);
            permission.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            var domain = AppDomain.CreateDomain(name, null, setup, permission);
            var instance = (T)Activator.CreateInstanceFrom(domain, typeof(T).Assembly.Location, typeof(T).FullName).Unwrap();
            return instance;
        }

        public static string BuildPathToAssembly(string assembly, string root)
        {
            var bin = Path.Combine(root, assembly, "bin");
            var conf = Directory.Exists(Path.Combine(bin, "Debug")) ? "Debug" : "Release";
            var pathToBase = Path.GetFullPath(Path.Combine(bin, conf));

            return Path.Combine(pathToBase, assembly + ".dll");
        }
    }
}
