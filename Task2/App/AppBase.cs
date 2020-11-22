using System;
using System.Security;

namespace App
{
    public class AppBase
    {
        public void Start(string assemblyName)
        {
            Console.WriteLine($"Main domain: {AppDomain.CurrentDomain.FriendlyName}");

            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            PermissionSet permissionSet = new PermissionSet(AppDomain.CurrentDomain.PermissionSet);

            // create app domain to prevent loading calculator implementation assembly to the main domain
            AppDomain baseDomain = AppDomain.CreateDomain("BaseDomain", null, domainSetup, permissionSet);

            CalcDomains calcDomains = (CalcDomains)baseDomain.
                CreateInstanceAndUnwrap(typeof(CalcDomains).Assembly.FullName, typeof(CalcDomains).FullName);

            calcDomains.InvokeSumMethods(assemblyName);
        }
    }
}
