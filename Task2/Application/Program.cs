using ICalculatorLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public class Program
    {
        private static readonly string _libraryPath = Path.Combine("..", "..", "..", "CalculatorLib", "bin", "Debug");
        private static readonly string _libraryName = "CalculatorLib.dll";
        private static readonly string _assemblyName = "CalculatorLib";

        public static void Main()
        {
            AppDomainSetup domainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            };
            var newDomain = AppDomain.CreateDomain("App", null, domainSetup);
            GetProxyCalculator(newDomain).SumAll(typeof(ICalculator), _assemblyName, Path.Combine(_libraryPath, _libraryName), 12, 13);
            Console.ReadKey();
        }

        public static AppDomain CreatAppDomainRestriced(string path, string newDomainTitle)
        {
            PermissionSet ps = new PermissionSet(PermissionState.None);
            ps.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            AppDomainSetup ads = new AppDomainSetup
            {
                ApplicationBase = path
            };
            return AppDomain.CreateDomain(newDomainTitle, null, ads, ps);
        }

        public static int SumInAppDomain(AppDomain appDomain, string assemblyName, string typeName, int a, int b)
        {
            return GetProxyCalculator(appDomain).Sum(assemblyName, typeName, a, b);
        }

        public static ProxyObjectCalculator GetProxyCalculator(AppDomain appDomain)
        {
            ProxyObjectCalculator proxyCalculator = (ProxyObjectCalculator)Activator.CreateInstanceFrom(appDomain, 
                                                                           typeof(ProxyObjectCalculator).Assembly.ManifestModule.FullyQualifiedName,
                                                                           typeof(ProxyObjectCalculator).FullName).Unwrap();
            return proxyCalculator;
        }

        public class ProxyObjectCalculator : MarshalByRefObject
        {
            public int Sum(string an, string tn, int a, int b)
            {
                var calc = (ICalculator)Activator.CreateInstance(an, tn).Unwrap();
                return calc.Sum(a, b);
            }

            public int SumInner(string an, Type t, int a, int b)
            {
                Assembly.Load(an);
                Console.WriteLine($"Actual Sum() called in {AppDomain.CurrentDomain.FriendlyName} domain");
                var calc = (ICalculator)Activator.CreateInstance(t);
                return calc.Sum(a, b);
            }

            public void SumAll(Type iType, string an, string ap, int a, int b)
            {
                Assembly.LoadFrom(ap);
                Console.WriteLine($"SumAll() called in {AppDomain.CurrentDomain.FriendlyName} domain");
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => iType.IsAssignableFrom(p) && p.IsClass);
                foreach (var type in types)
                {
                    var ad = CreatAppDomainRestriced(_libraryPath, $"type {type}");
                    var calc = GetProxyCalculator(ad);
                    Console.WriteLine($"Calculator {type}:");
                    try
                    {
                        Console.WriteLine($"Result is {calc.SumInner(an, type, a, b)}");
                    } catch (Exception e)
                    {
                        Console.WriteLine($"Resul is {e.Message}");
                    }
                }
            }
        }
    }
}
