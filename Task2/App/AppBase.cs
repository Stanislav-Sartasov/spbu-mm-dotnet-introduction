using System;
using System.Collections.Generic;
using System.Security;
using CalculatorInterface;

namespace App
{
    public class AppBase : IDisposable
    {
        CalcDomains calcDomains;

        public void Dispose()
        {
            calcDomains?.Dispose();
        }

        public List<ICalculator> Implementations => calcDomains?.Implementations;

        public void Start(string assemblyName)
        {
            if (calcDomains != null)
            {
                throw new Exception("App domains for AppBase were already created");
            }

            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            PermissionSet permissionSet = new PermissionSet(AppDomain.CurrentDomain.PermissionSet);

            // create app domain to prevent loading calculator implementation assembly to the main domain
            AppDomain baseDomain = AppDomain.CreateDomain("BaseDomain", null, domainSetup, permissionSet);

            calcDomains = (CalcDomains)baseDomain.
                CreateInstanceAndUnwrap(typeof(CalcDomains).Assembly.FullName, typeof(CalcDomains).FullName);

            calcDomains.FindCalculatorImplementations(assemblyName);
        }
    }
}
