using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using AppDomains_Bashkirov.Utils;


namespace AppDomains_Bashkirov
{
internal class EntryPoint
{
    public static int Main()
    {
        Console.WriteLine(@"Please run ""Application\Tests\ExtensionLoaderTests.cs"" instead");
        return -1;
    }
}

public static class ExtensionsManager
{
    public static List<(TExtensionPoint, AppDomain)> LoadExtensionsInSeparateDomainsFrom<TExtensionPoint>(
        string assemblyPath,
        AppDomainSetup loaderDomainsSetup,
        string domainsNamePrefix,
        Evidence domainsSecurityInfo,
        AppDomainSetup domainsSetup,
        PermissionSet domainsPermissions)
    {
        var loaderDomain = AppDomain.CreateDomain("ExtensionsManager#loaderDomain", new Evidence(), loaderDomainsSetup);
        try
        {
            var loader = AppDomainUtils.CreateInstance<Loader>(loaderDomain);
            var result = loader.DoLoad<TExtensionPoint>(
                assemblyPath, domainsNamePrefix, domainsSecurityInfo, domainsSetup, domainsPermissions);
            return result;
        }
        finally
        {
            AppDomain.Unload(loaderDomain);
        }
    }

    private class Loader : MarshalByRefObject
    {
        public List<(TExtensionPoint, AppDomain)> DoLoad<TExtensionPoint>(
            string assemblyPath,
            string domainsNamePrefix,
            Evidence domainsSecurityInfo,
            AppDomainSetup domainsSetup,
            PermissionSet domainsPermissions)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            var assemblyBytes = File.ReadAllBytes(assemblyPath);

            return assembly.GetTypes()
                .Where(it => typeof(TExtensionPoint).IsAssignableFrom(it)
                             && !it.IsAbstract
                             && it.GetConstructors().Length > 0)
                .Select(type =>
                {
                    var typeName = type.FullName ?? throw new ArgumentException();
                    var domain = AppDomain.CreateDomain(
                        $"{domainsNamePrefix}: {typeName}", domainsSecurityInfo, domainsSetup, domainsPermissions);
                    domain.SetData(AssemblyNameKey, assembly.FullName);
                    domain.SetData(AssemblyBytesKey, assemblyBytes);
                    domain.AssemblyResolve += AssemblyLoadedAsBytesResolver;
                    var instance = (TExtensionPoint) domain.CreateInstanceAndUnwrap(assembly.FullName, typeName);
                    return (instance, domain);
                })
                .ToList()!;
        }

        private const string AssemblyNameKey = "ExtensionsManager#Loader#assemblyName"; 
        private const string AssemblyBytesKey = "ExtensionsManager#Loader#assemblyBytes"; 
        
        private static Assembly? AssemblyLoadedAsBytesResolver(object sender, ResolveEventArgs args)
        {
            var domain = AppDomain.CurrentDomain;
            return args.Name == (string) domain.GetData(AssemblyNameKey)
                ? Assembly.Load((byte[]) domain.GetData(AssemblyBytesKey))
                : null;
        }
    }
}
}