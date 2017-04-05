using System;
using System.Configuration;

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

            // ReSharper disable once UnusedMember.Local
            public void DisposeManaged()
            {
                AppDomain.SetData(ConfigFileKey, _originalAppConfig);
                ResetConfiguration();
            }

            private static void ResetConfiguration()
            {
                var configurationManager = typeof(ConfigurationManager);

                configurationManager.SetStatic("s_initState", 0)
                    .SetStatic("s_configSystem", null);

                configurationManager
                    .Assembly
                    .GetType("System.Configuration.ClientConfigPaths")
                    .SetStatic(
                        "s_current",
                        null);
            }

            public AppDomain AppDomain { get; private set; }
            public string ConfigFile { get; private set; }
        }
    }
}