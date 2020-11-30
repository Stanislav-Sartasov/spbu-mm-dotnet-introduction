using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using InterfaceLib;

namespace CalculatorLoader
{
    internal static class Loader
    {
        private class Impostor : MarshalByRefObject
        {
            private readonly List<AppDomain> _domains = new List<AppDomain>();
            
            public List<ICalculator> DetectAndLoad(string assemblyName)
            {
                var calculators = new List<ICalculator>();
                var assembly = Assembly.Load(assemblyName);
                
                foreach (var typeInfo in assembly.DefinedTypes)
                {
                    if (!typeInfo.GetInterfaces().Contains(typeof(ICalculator))) continue;
                    
                    var evidence = new Evidence();
                    var permissionSet = new PermissionSet(PermissionState.None);
                    permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    var appDomainSetup = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory};

                    var appDomain = AppDomain.CreateDomain($"DomainFor{typeInfo.Name}", evidence, appDomainSetup, permissionSet);

                    var calculatorInstance =
                        appDomain.CreateInstanceAndUnwrap(assembly.FullName, typeInfo.FullName);
                        
                    calculators.Add((ICalculator) calculatorInstance);
                    _domains.Add(appDomain);
                }
                
                return calculators;
            }

            public void Unload()
            {
                foreach (var domain in _domains)
                {
                    AppDomain.Unload(domain);
                }
            }
        }
        
        
        public static void Main(string[] args)
        {
            var loaderDomain = AppDomain.CreateDomain("LoaderAppDomain");
            var impostor =
                (Impostor) loaderDomain.CreateInstanceAndUnwrap(typeof(Impostor).Assembly.FullName,
                    typeof(Impostor).FullName);

            var calculators = impostor.DetectAndLoad("ImplementationLib");
            
            Console.WriteLine("--- All loaded assemblies in default domain:");
            foreach (var assembly in Thread.GetDomain().GetAssemblies())
            {
                Console.WriteLine(assembly.FullName);
            }

            Console.WriteLine("\n--- Calling Sum:");
            foreach (var calculator in calculators)
            {
                calculator.Sum(42, 3);
            }
            
            impostor.Unload();
            AppDomain.Unload(loaderDomain);
        }
    }
}