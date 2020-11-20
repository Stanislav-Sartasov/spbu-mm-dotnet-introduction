using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Interface;

namespace Loader
{
    public class ImplementationLoader: IDisposable
    {
        private readonly Internal _internal;
        private readonly AppDomain _internalDomain;
        public List<ICalculator> Instances { get; }

        public ImplementationLoader(string assemblyPath)
        {
            var assemblyName = typeof(Internal).Assembly.FullName ?? "";
            var typeName = typeof(Internal).FullName ?? ""; 
            
            _internalDomain = AppDomain.CreateDomain("AppDomain:Internal");
            _internal = (Internal) _internalDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
            Instances = _internal.Load(assemblyPath);
        }

        public void Dispose()
        {
            _internal.Dispose();
            AppDomain.Unload(_internalDomain);
        }
        
        private class Internal : MarshalByRefObject, IDisposable
        {
            private List<ICalculator> _instances;
            private List<AppDomain> _domains;

            public Internal()
            {
                _instances = new List<ICalculator>();
                _domains = new List<AppDomain>();
            }
            public List<ICalculator> Load(string assemblyPath)
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(ICalculator).IsAssignableFrom(type))
                    {
                        string assemblyName = assembly.FullName;
                        string typeName = type.FullName;
                        
                        var evidence = new Evidence();
                        var permissionSet = new PermissionSet(PermissionState.None);
                        permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                        var setup = new AppDomainSetup { ApplicationBase = Path.GetFullPath(Environment.CurrentDirectory) };
                
                        var domain = AppDomain.CreateDomain($"Domain:{typeName}", evidence, setup, permissionSet);
                        var instance = (ICalculator) domain.CreateInstanceAndUnwrap(assemblyName, typeName);
                        
                        _domains.Add(domain);
                        _instances.Add(instance);
                    }
                }

                return _instances;
            }

            public void Dispose()
            {
                foreach (var domain in _domains)
                {
                    AppDomain.Unload(domain);
                }
            }
        }

    }
}