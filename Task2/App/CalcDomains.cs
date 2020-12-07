using System;
using System.Security;
using System.Security.Permissions;
using System.Reflection;
using System.Collections.Generic;
using CalculatorInterface;

namespace App
{
    class CalcDomains : MarshalByRefObject, IDisposable
    {
        List<AppDomain> domains;
        List<ICalculator> impls;

        public CalcDomains()
        {
            domains = new List<AppDomain>();
            impls = new List<ICalculator>();
        }

        public List<ICalculator> Implementations => impls;

        public void Dispose()
        {
            foreach (var dom in domains)
            {
                AppDomain.Unload(dom);
            }

            domains.Clear();
            impls.Clear();
        }

        public void FindCalculatorImplementations(string assemblyName)
        {
            if (domains.Count > 0 || impls.Count > 0)
            {
                throw new Exception("Some assembly is already loaded");
            }

            Console.WriteLine($"InvokeSumMethods(..) domain: {AppDomain.CurrentDomain.FriendlyName}{Environment.NewLine}");

            Assembly asmb = Assembly.Load(assemblyName);
  
            foreach (Type t in asmb.GetTypes())
            {
                if (t.IsAssignableFrom(typeof(ICalculator)) 
                    && !t.IsAbstract && !t.IsInterface)
                {
                    continue;
                }

                AppDomainSetup domainSetup = new AppDomainSetup();
                domainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

                PermissionSet permissionSet = new PermissionSet(PermissionState.None);
                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

                AppDomain implDomain = AppDomain.CreateDomain("Domain" + t.Name, null, domainSetup, permissionSet);
                ICalculator impl = (ICalculator)implDomain.CreateInstanceAndUnwrap(asmb.FullName, t.FullName);

                domains.Add(implDomain);
                impls.Add(impl);
            }
        }
    }
}
