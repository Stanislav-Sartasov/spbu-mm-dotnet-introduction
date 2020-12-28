using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace Application
{
    public sealed class PluginLoader : MarshalByRefObject
    {
        public static List<PluginData<TPlugin>> LoadPlugins<TPlugin>(
            string assemblyPath,
            Evidence domainsSecurityInfo
        )
        {
            var loaderDomain = AppDomain.CreateDomain(
                "Domain that loads plugins",
                new Evidence(),
                AppDomain.CurrentDomain.SetupInformation
            );
            var loader = (PluginLoader) loaderDomain.CreateInstanceAndUnwrap(
                typeof(PluginLoader).Assembly.FullName,
                typeof(PluginLoader).FullName!
            );
            var result = loader.Load<TPlugin>(
                assemblyPath,
                domainsSecurityInfo,
                AppDomain.CurrentDomain.SetupInformation,
                CreatePluginPermissions()
            );
            return result;
        }

        private static PermissionSet CreatePluginPermissions()
        {
            var permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
            return permissions;
        }

        private const string AssemblyNameKey = "Assembly that contains a plugin";
        private const string AssemblyBytesKey = "Bytes of an assembly that contains a plugin";

        public List<PluginData<TPlugin>> Load<TPlugin>(
            string assemblyPath,
            Evidence domainsSecurityInfo,
            AppDomainSetup domainsSetup,
            PermissionSet domainsPermissions
        )
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);

            return assembly.GetTypes()
                .Where(it => typeof(TPlugin).IsAssignableFrom(it))
                .Select(type =>
                {
                    string typeName = type.FullName ?? throw new ArgumentException();
                    var domain = AppDomain.CreateDomain(
                        $"Domain that loads plugin: {typeName}", domainsSecurityInfo, domainsSetup, domainsPermissions);
                    domain.SetData(AssemblyNameKey, assembly.FullName);
                    domain.SetData(AssemblyBytesKey, assemblyBytes);
                    domain.AssemblyResolve += ResolveHandler;
                    var instance = (TPlugin) domain.CreateInstanceAndUnwrap(assembly.FullName, typeName);
                    return new PluginData<TPlugin>(domain, instance);
                })
                .ToList();
        }

        private static Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            var domain = AppDomain.CurrentDomain;
            if (args.Name != (string) domain.GetData(AssemblyNameKey)) return null;
            return Assembly.Load((byte[]) domain.GetData(AssemblyBytesKey));
        }
    }
}
