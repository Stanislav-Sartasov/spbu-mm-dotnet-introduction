using System;
using System.Collections.Generic;
using System.Reflection;
using AppDomains;

namespace Loader
{
    public class ImplementationLoader<TBaseClass>
    {
        private readonly List<TBaseClass> _instances;
        private readonly string _assemblyPath;
        
        public List<TBaseClass> Instances  => _instances;
        
        public ImplementationLoader(string assemblyPath)
        {
            _assemblyPath = assemblyPath;

            List<string> implementationsNames = GetImplementationsNames();

            foreach (var name in implementationsNames)
            {
                Console.Out.WriteLine(name);
            }
        }

        private List<string> GetImplementationsNames()
        {
            var currentAssemblyName = Assembly.GetAssembly(typeof(ImplementationsFinder))?.GetName().Name ?? "";
            var objectName = typeof(ImplementationsFinder).AssemblyQualifiedName ?? "";
            var implementationsDomain = AppDomain.CreateDomain("AppDomain:ReflectionInfoMining");
            
            var finder = (ImplementationsFinder)
                implementationsDomain.CreateInstanceAndUnwrap(currentAssemblyName, objectName,
                    new object[] {_assemblyPath, typeof(TBaseClass)});

            var implementationsList = finder.ImplementationsNames;
            
            AppDomain.Unload(implementationsDomain);

            return implementationsList;
        }

        private class ImplementationsFinder : MarshalByRefObject
        {
            private readonly string _assemblyPath;
            private readonly Type _baseType;
            private readonly List<string> _implementationsNames;

            public List<string> ImplementationsNames => _implementationsNames;

            public ImplementationsFinder(string assemblyPath, Type baseType)
            {
                _assemblyPath = assemblyPath;
                _baseType = baseType;
                _implementationsNames = Find();
            }

            private List<string> Find()
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(_assemblyPath);
                var found = new List<string>();    
                
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(_baseType))
                        found.Add(type.AssemblyQualifiedName);
                }

                return found;
            }
        }
    }
}