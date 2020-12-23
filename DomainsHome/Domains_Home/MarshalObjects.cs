using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using CalculatorInterfaceLib;
using System.Threading;

namespace DomainsHome
{
    class CalculatorProxy: MarshalByRefObject
    {
        public int RunCalculator(Type type, int a, int b)
        {
            MethodInfo myMethod = type.GetMethod("Sum");
            object obj = Activator.CreateInstance(type);

            return (int) myMethod.Invoke(obj, new object[] { a, b });
        }
    }

    class LoaderMarshalingObject: MarshalByRefObject, IDisposable
    {
        private List<Type> calculators = new List<Type>();

        public void Load(string assemblyName)
        {
            Console.WriteLine("Loading domain - {0}", Thread.GetDomain().FriendlyName);
            Assembly calcDll = Assembly.Load(assemblyName);

            foreach (Type type in calcDll.GetTypes())
            {
                
               if (!type.IsAbstract && !type.IsInterface && typeof(ICalculator).IsAssignableFrom(type))
               {
                    calculators.Add(type);
               } 
            }
        }

        public int RunCalculator(string name, int a, int b, string assemblyName)
        {
            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            PermissionSet permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissionSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));

            var type = FindCalculatorByName(name);

            if (type == null)
            {
                Console.WriteLine("Calculator not found");
                return -1;
            }
            AppDomain domain = AppDomain.CreateDomain("Domain - " + type.FullName, null, domainSetup, permissionSet);
            var calc = (CalculatorProxy) domain.CreateInstanceAndUnwrap(assemblyName, "DomainsHome.CalculatorProxy");
            var result = calc.RunCalculator(type, a, b);
            AppDomain.Unload(domain);
            return result;
        }

        private Type FindCalculatorByName(string name)
        {
            var list = calculators.Where(type => type.Name == name);

            if (list.Count() > 0)
            {
                return list.First();
            }

            return null;
        }

        public List<string> GetLoadedCalculatorsNames()
        {
            return calculators.ConvertAll(type => type.Name);
        }

        public void Dispose()
        {
            calculators.Clear();
        }
    }
}
