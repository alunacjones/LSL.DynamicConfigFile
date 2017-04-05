using System;

namespace LSL.DynamicConfigFile
{
    public interface IDynamicConfigFileFactory
    {
        IDynamicConfigFile Create(Action<IDynamicConfigFileConfiguration> configurator);
    }
}