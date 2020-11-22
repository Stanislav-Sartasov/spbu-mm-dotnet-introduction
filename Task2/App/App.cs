using System;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.IO;
using CalculatorInterface;
using System.Reflection;
using System.Collections.Generic;

namespace App
{
    public class App
    {

        public static void InvokeSumMethods(string assemblyName)
        {
            Assembly asmb = Assembly.Load(assemblyName);
            var domains = new List<AppDomain>();
            var impls = new List<ICalculator>();
  
            foreach (Type t in asmb.GetTypes())
            {
                if (t.IsAssignableFrom(typeof(ICalculator)) 
                    && !t.IsAbstract && !t.IsInterface)
                {
                    continue;
                }

                AppDomainSetup domainSetup = new AppDomainSetup();
                domainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

                PermissionSet permissionSet = new PermissionSet(AppDomain.CurrentDomain.PermissionSet);
                permissionSet.RemovePermission(typeof(FileIOPermission));

                AppDomain implDomain = AppDomain.CreateDomain("Domain" + t.Name, null, domainSetup, permissionSet);
                ICalculator impl = (ICalculator)implDomain.CreateInstanceAndUnwrap(asmb.FullName, t.FullName);

                domains.Add(implDomain);
                impls.Add(impl);
            }

            foreach (var impl in impls)
            {
                int r = impl.Sum(1, 2);
                Console.WriteLine($"Result: {r}{Environment.NewLine}");
            }

            foreach (var dom in domains)
            {
                AppDomain.Unload(dom);
            }
        }
    }
}
