using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace LSL.DynamicConfigFile
{
    public static class IDynamicConfigFileFactoryExtensions
    {
        private static readonly Action<IDynamicConfigFileConfiguration> NoOpConfigurator = configuration => { };

        public static IDynamicConfigFile Create(
            this IDynamicConfigFileFactory source, 
            string file, 
            Action<IDynamicConfigFileConfiguration> configurator = null)
        {
            return source.Create(AppDomain.CurrentDomain, file, configurator);
        }

        private static IDynamicConfigFile CreateWithConfigurator(
            IDynamicConfigFileFactory source,
            Action<IDynamicConfigFileConfiguration> configurator,
            Action<IDynamicConfigFileConfiguration> extensionConfigurator)
        {
            return source.Create(cfg =>
            {
                (configurator ?? NoOpConfigurator)(cfg);

                extensionConfigurator(cfg);
            });
        }

        public static IDynamicConfigFile Create(
            this IDynamicConfigFileFactory source, 
            AppDomain appDomain, 
            string file,
            Action<IDynamicConfigFileConfiguration> configurator = null)
        {
            return CreateWithConfigurator(
                source,
                configurator,
                cfg =>
                {
                    cfg.WithAppDomainOf(appDomain)
                        .WithConfigurationFileOf(file);
                });
        }

        public static IDynamicConfigFile CreateFromStringContent(
            this IDynamicConfigFileFactory source, 
            string stringContent,
            Action<IDynamicConfigFileConfiguration> configurator = null)
        {
            return CreateWithConfigurator(
               source,
               configurator,
               cfg =>
               { 
                    File.WriteAllText(
                        cfg.ConfigurationFileName,
                        stringContent);                    
                });
        }

        public static IDynamicConfigFile CreateFromExistingFileAsXDocument(
            this IDynamicConfigFileFactory source,
            Action<XDocument> existingConfigurationConfigurator,
            Action<IDynamicConfigFileConfiguration> configurator = null)
        {
            return CreateWithConfigurator(
                source,
                configurator,
                cfg =>
                {
                    var xml = XDocument.Load(cfg.AppDomain.SetupInformation.ConfigurationFile);
                    existingConfigurationConfigurator(xml);
                    xml.Save(cfg.ConfigurationFileName);
                });
        }

        public static IDynamicConfigFile CreateFromExistingFileAsString(
            this IDynamicConfigFileFactory source,
            Func<string, string> existingConfigurationConfigurator,
            Action<IDynamicConfigFileConfiguration> configurator = null)
        {
            return CreateWithConfigurator(
                source,
                configurator,
                cfg =>
                {
                    var existingFileContents = File.ReadAllText(
                        cfg.AppDomain
                            .SetupInformation
                            .ConfigurationFile);

                    File.WriteAllText(
                        cfg.ConfigurationFileName,
                        existingConfigurationConfigurator(
                            existingFileContents));                    
                });
        }
    }

}