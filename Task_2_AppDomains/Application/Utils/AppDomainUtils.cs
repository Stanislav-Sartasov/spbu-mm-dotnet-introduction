using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppDomains_Bashkirov.Utils
{
public static class AppDomainUtils
{
    public static T CreateInstance<T>(AppDomain domain) where T : MarshalByRefObject =>
        (T) domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
}

public class DomainAssembliesInfoExtractor : MarshalByRefObject
{
    public string[] AssemblyShortNames =>
        AppDomain.CurrentDomain.GetAssemblies().Select(it => new AssemblyName(it.FullName).Name).ToArray();
}

public class ExtensionsHolder<T> : IDisposable
{
    public List<(T, AppDomain)> Extensions { get; }

    public ExtensionsHolder(List<(T, AppDomain)> extensions)
    {
        Extensions = extensions;
    }

    public void Dispose()
    {
        foreach (var (_, domain) in Extensions)
        {
            AppDomain.Unload(domain);
        }
    }
}
}