using System;

namespace LSL.DynamicConfigFile
{
    public interface IDynamicConfigFileConfiguration
    {
        IDynamicConfigFileConfiguration WithConfigurationFileOf(string configurationFileName);
        IDynamicConfigFileConfiguration WithAppDomainOf(AppDomain appDomain);

        AppDomain AppDomain { get; }
        string ConfigurationFileName { get; }
    }
}