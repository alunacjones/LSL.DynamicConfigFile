using System;

namespace LSL.DynamicConfigFile
{
    /// <summary>
    /// The dynamic config file
    /// </summary>
    public interface IDynamicConfigFile : IDisposable
    {
        /// <summary>
        /// The <c>AppDomain</c> whose config file is being overriden
        /// </summary>
        /// <value></value>
        AppDomain AppDomain { get; }

        /// <summary>
        /// The path to the dynamic config file
        /// </summary>
        /// <value></value>
        string ConfigFile { get; }
    }
}