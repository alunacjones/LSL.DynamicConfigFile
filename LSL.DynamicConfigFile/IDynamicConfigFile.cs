using System;
using System.Security.Cryptography.X509Certificates;

namespace LSL.DynamicConfigFile
{
    public interface IDynamicConfigFile : IDisposable
    {
        AppDomain AppDomain { get; }
        string ConfigFile { get; }
    }
}