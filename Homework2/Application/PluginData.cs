using System;

namespace Application
{
    public sealed class PluginData<T> : MarshalByRefObject
    {
        public PluginData(AppDomain domain, T plugin)
        {
            Domain = domain;
            Plugin = plugin;
        }

        public AppDomain Domain { get; }
        public T Plugin { get; }
    }
}
