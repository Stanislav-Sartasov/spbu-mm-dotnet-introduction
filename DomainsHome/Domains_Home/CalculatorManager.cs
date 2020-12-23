using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Text;
using CalculatorInterfaceLib;

namespace DomainsHome
{
    public class CalculatorManager: IDisposable
    {
        private LoaderMarshalingObject Loader;
        private string AssemblyName = typeof(CalculatorManager).Assembly.FullName;
        public CalculatorManager(string calcDllName)
        {
            AppDomainSetup domainSetup = new AppDomainSetup();
            domainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            PermissionSet permissionSet = new PermissionSet(AppDomain.CurrentDomain.PermissionSet);

            AppDomain domain = AppDomain.CreateDomain("Start domain", null, domainSetup, permissionSet);
            Loader = (LoaderMarshalingObject)domain.CreateInstanceAndUnwrap(AssemblyName, "DomainsHome.LoaderMarshalingObject");
            Loader.Load(calcDllName);
        }

        public void Dispose()
        {
            Loader.Dispose();
        }

        public List<string> GetCalculatorsNames()
        {
            return Loader.GetLoadedCalculatorsNames();
        }

        public int RunCalculator(string name, int a, int b)
        {
            return Loader.RunCalculator(name, a, b, AssemblyName);
        }
    }
}
