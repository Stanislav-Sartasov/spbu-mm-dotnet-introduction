using CalculatorInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

using CalculatorLocation = System.ValueTuple<string, string>;

namespace Loader
{
    public class CalculatorLoader : IDisposable
    {
        private readonly List<AppDomain> _loadedDomains = new List<AppDomain>();

        public class CalculatorProxy : MarshalByRefObject
        {
            public CalculatorLocation[] DetectCalculatorImplementations(string[] assemblyPaths)
            {
                return assemblyPaths.SelectMany(path =>
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
                    Assembly currentAssembly = AppDomain.CurrentDomain.Load(assemblyName);

                    return currentAssembly
                        .GetExportedTypes()
                        .Where(type => !type.IsAbstract)
                        .Where(type => typeof(ICalculator).IsAssignableFrom(type))
                        .Select(type => (path, type.FullName));
                }).ToArray();
            }
        }

        private IEnumerable<CalculatorLocation> DetectCalculators(IEnumerable<string> calculatorLibPaths)
        {
            string[] pathsArray = calculatorLibPaths.ToArray();

            AppDomainSetup domainSetup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
            };

            PermissionSet permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
            permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, pathsArray));

            AppDomain loaderDomain = AppDomain.CreateDomain("Calculator loader", new Evidence(), domainSetup, permissions);

            try
            {
                string proxyAssemblyName = typeof(CalculatorProxy).Assembly.FullName;
                string proxyTypeName = typeof(CalculatorProxy).FullName;

                CalculatorProxy proxy = (CalculatorProxy)loaderDomain.CreateInstanceAndUnwrap(proxyAssemblyName, proxyTypeName);

                return proxy.DetectCalculatorImplementations(pathsArray);
            }
            finally
            {
                AppDomain.Unload(loaderDomain);
            }
        }

        public IEnumerable<ICalculator> LoadCalculators(IEnumerable<string> calculatorLibPaths)
        {
            IEnumerable<CalculatorLocation> locations = DetectCalculators(calculatorLibPaths);
            List<ICalculator> loadedCalculators = new List<ICalculator>();

            foreach (((string pathToAssembly, string typeName), int index) in locations.WithIndex())
            {
                AppDomainSetup domainSetup = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                };

                PermissionSet permissions = new PermissionSet(PermissionState.None);
                permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                permissions.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, pathToAssembly));

                AppDomain calculatorDomain = AppDomain.CreateDomain($"Calculator {index} Domain", new Evidence(), domainSetup, permissions);

                try
                {
                    ICalculator currentCalculator = (ICalculator)calculatorDomain.CreateInstanceFromAndUnwrap(pathToAssembly, typeName);

                    if (loadedCalculators.Any(calculator => calculator.CalculatorName == currentCalculator.CalculatorName))
                    {
                        AppDomain.Unload(calculatorDomain);
                        continue;
                    }

                    _loadedDomains.Add(calculatorDomain);
                    loadedCalculators.Add(currentCalculator);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unable to load calculator {index}. Cause: {e.Message}");
                    AppDomain.Unload(calculatorDomain);
                }
            }

            return loadedCalculators;
        }

        public void Dispose()
        {
            _loadedDomains.ForEach(domain => AppDomain.Unload(domain));
            _loadedDomains.Clear();
        }
    }
}
