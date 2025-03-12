using System;

namespace LSL.DynamicConfigFile
{
    /// <summary>
    /// The dynamic config file configuration
    /// </summary>
    public interface IDynamicConfigFileConfiguration
    {
        /// <summary>
        /// Setup the new configuration file to use
        /// </summary>
        /// <param name="configurationFileName"></param>
        /// <returns></returns>
        IDynamicConfigFileConfiguration WithConfigurationFileOf(string configurationFileName);

        /// <summary>
        /// The <c>AppDomain</c> whose config file should be overriden
        /// </summary>
        /// <param name="appDomain"></param>
        /// <returns></returns>
        IDynamicConfigFileConfiguration WithAppDomainOf(AppDomain appDomain);

        /// <summary>
        /// The <c>AppDomain</c> whose config file should be overriden
        /// </summary>
        /// <value></value>
        AppDomain AppDomain { get; }

        /// <summary>
        /// The new configuration file path
        /// </summary>
        /// <value></value>
        string ConfigurationFileName { get; }
    }
}