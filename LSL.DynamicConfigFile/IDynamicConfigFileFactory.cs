using System;

namespace LSL.DynamicConfigFile
{
    /// <summary>
    /// The dynamic config file factory
    /// </summary>
    public interface IDynamicConfigFileFactory
    {
        /// <summary>
        /// Creates a dynamic config file
        /// </summary>
        /// <param name="configurator">A delegate to configure the new configuration file</param>
        /// <returns></returns>
        IDynamicConfigFile Create(Action<IDynamicConfigFileConfiguration> configurator);
    }
}