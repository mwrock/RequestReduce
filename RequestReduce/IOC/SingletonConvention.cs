using System;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace RequestReduce.IOC
{
    public class SingletonConvention : DefaultConventionScanner
    {
        public override void Process(Type type, Registry registry)
        {
            base.Process(type, registry);
            var pluginType = FindPluginType(type);
            if ((pluginType != null))
            {
                registry.For(pluginType).Singleton();
            }
        }
    }
}