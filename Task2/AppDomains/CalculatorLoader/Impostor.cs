using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using InterfaceLib;

namespace CalculatorLoader
{
    public class Impostor: IDisposable
    {
        private readonly InternalLoader _internalLoader;
        private readonly AppDomain _loaderDomain;
        
        public List<ICalculator> Calculators { get; private set; }

        public Impostor(string implementationAssemblyPath)
        {
            AppDomainSetup loaderDomainSetup = new AppDomainSetup
            {
                ApplicationBase = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            };
            
            _loaderDomain = AppDomain.CreateDomain("LoaderAppDomain", null, loaderDomainSetup);
            
            _internalLoader =
                (InternalLoader) _loaderDomain.CreateInstanceAndUnwrap(typeof(InternalLoader).Assembly.FullName,
                    typeof(InternalLoader).FullName);

            Calculators = _internalLoader.DetectAndLoad(implementationAssemblyPath);
        }

        public void Dispose()
        {
            _internalLoader.Unload();
            AppDomain.Unload(_loaderDomain);
        }
        private class InternalLoader : MarshalByRefObject
        {
            private readonly List<AppDomain> _domains = new List<AppDomain>();
        
            public List<ICalculator> DetectAndLoad(string assemblyPath)
            {
                var calculators = new List<ICalculator>();
                var assembly = Assembly.LoadFrom(assemblyPath);
            
                foreach (var typeInfo in assembly.DefinedTypes)
                {
                    if (!typeInfo.GetInterfaces().Contains(typeof(ICalculator))) continue;
                
                    var evidence = new Evidence();
                    var permissionSet = new PermissionSet(PermissionState.None);
                    permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                    var appDomainSetup = new AppDomainSetup { ApplicationBase = Path.GetFullPath(Path.GetDirectoryName(assemblyPath)) };

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
    }
}