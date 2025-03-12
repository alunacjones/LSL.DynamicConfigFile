using System;
using System.Configuration;

namespace LSL.DynamicConfigFile
{
    /// <inheritdoc/>
    public class DynamicConfigFileFactory : IDynamicConfigFileFactory
    {
        /// <inheritdoc/>
        public IDynamicConfigFile Create(Action<IDynamicConfigFileConfiguration> configurator)
        {
            var cfg = new DynamicConfigFileConfiguration();
            configurator(cfg);

            return new DynamicConfigFile(cfg.AppDomain, cfg.ConfigurationFileName);
        }

        private class DynamicConfigFile : IDynamicConfigFile
        {
            private const string _configFileKey = "APP_CONFIG_FILE";

            private readonly object _originalAppConfig;
            private bool _disposedValue;

            public DynamicConfigFile(AppDomain appDomain, string configFile)
            {
                _originalAppConfig = appDomain.GetData(_configFileKey).ToString();
                ConfigFile = configFile;
                AppDomain = appDomain;
                AppDomain.SetData(_configFileKey, configFile);
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

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    if (disposing)
                    {
                        AppDomain.SetData(_configFileKey, _originalAppConfig);
                        ResetConfiguration();
                    }

                    _disposedValue = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}