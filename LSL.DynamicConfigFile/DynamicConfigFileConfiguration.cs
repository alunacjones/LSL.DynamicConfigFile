using System;

namespace LSL.DynamicConfigFile
{
    internal class DynamicConfigFileConfiguration : IDynamicConfigFileConfiguration
    {
        public AppDomain AppDomain { get; private set; }
        public string ConfigurationFileName { get; private set; }

        public DynamicConfigFileConfiguration()
        {
            AppDomain = AppDomain.CurrentDomain;
            ConfigurationFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile + ".dynamic.config";
        }

        public IDynamicConfigFileConfiguration WithConfigurationFileOf(string configurationFileName)
        {
            ConfigurationFileName = configurationFileName;
            return this;
        }

        public IDynamicConfigFileConfiguration WithAppDomainOf(AppDomain appDomain)
        {
            AppDomain = appDomain;
            return this;
        }
    }
}