using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace LSL.DynamicConfigFile
{
    public class DynamicConfigFileFactory : IDynamicConfigFileFactory
    {
        public IDynamicConfigFile Create(Action<IDynamicConfigFileConfiguration> configurator)
        {
            var cfg = new DynamicConfigFileConfiguration();
            configurator(cfg);

            return new DynamicConfigFile(cfg.AppDomain, cfg.ConfigurationFileName);
        }

        private class DynamicConfigFile : IDynamicConfigFile
        {
            private const string ConfigFileKey = "APP_CONFIG_FILE";

            private readonly object _originalAppConfig;

            public DynamicConfigFile(AppDomain appDomain, string configFile)
            {
                _originalAppConfig = appDomain.GetData(ConfigFileKey).ToString();
                ConfigFile = configFile;
                AppDomain = appDomain;
                AppDomain.SetData(ConfigFileKey, configFile);
                ResetConfiguration();
            }

            public void Dispose()
            {                
            }

            public void DisposeManaged()
            {
                AppDomain.SetData(ConfigFileKey, _originalAppConfig);
                ResetConfiguration();
            }

            private static void ResetConfiguration()
            {
                var configurationManager = typeof(ConfigurationManager);

                SetStaticValue(configurationManager, "s_initState", 0);
                SetStaticValue(configurationManager, "s_configSystem", null);                

                SetStaticValue(configurationManager
                    .Assembly
                    .GetType("System.Configuration.ClientConfigPaths"),
                    "s_current",
                    null);
            }

            private static void SetStaticValue(IReflect type, string fieldName, object value)
            {
                type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic) 
                    .SetValue(null, value);
            }

            public AppDomain AppDomain { get; private set; }
            public string ConfigFile { get; private set; }
        }
    }
}