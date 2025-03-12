using System;
using System.IO;
using System.Xml.Linq;
using LSL.DynamicConfigFile.Xml;

namespace LSL.DynamicConfigFile
{
    /// <summary>
    /// IDynamicConfigFileFactoryExtensions
    /// </summary>
    public static class IDynamicConfigFileFactoryExtensions
    {
        private static readonly Action<IDynamicConfigFileConfiguration> NoOpConfigurator = configuration => { };

        /// <summary>
        /// Create a dynamic config file for the current <c>AppDomain</c>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="file">The file to use for configuration settings</param>
        /// <param name="configurator">A delegate to configure the dynamic config file</param>
        /// <returns></returns>
        public static IDynamicConfigFile Create(
            this IDynamicConfigFileFactory source,
            string file,
            Action<IDynamicConfigFileConfiguration> configurator = null) => 
                source.Create(AppDomain.CurrentDomain, file, configurator);

        private static IDynamicConfigFile CreateWithConfigurator(
            IDynamicConfigFileFactory source,
            Action<IDynamicConfigFileConfiguration> configurator,
            Action<IDynamicConfigFileConfiguration> extensionConfigurator) => 
                source.Create(cfg =>
                {
                    (configurator ?? NoOpConfigurator)(cfg);

                    extensionConfigurator(cfg);
                });

        /// <summary>
        /// Create a dynamic config file for the given <c>AppDomain</c>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="appDomain">The <c>AppDomain</c> to target</param>
        /// <param name="file">The file to use for configuration settings</param>
        /// <param name="configurator">A delegate to configure the dynamic config file</param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a dynamic config file from the given string content
        /// </summary>
        /// <param name="source"></param>
        /// <param name="stringContent">The content of the dynamic config fie</param>
        /// <param name="configurator">A delegate to configure the dynamic config file</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a dynamic config file based on thge <c>AppDomain</c>'s
        /// current configuration file
        /// </summary>
        /// <param name="source"></param>
        /// <param name="existingConfigurationConfigurator">
        /// A configurator to modify the <c>XDocument</c> representation of a clone
        /// of the current configuration file
        /// </param>
        /// <param name="configurator">The </param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a dynamic config file from the given file path and allows for XDocument configuration.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filePath">The file to use as a new config file</param>
        /// <param name="fileConfigurationConfigurator">An XDocument configurator</param>
        /// <param name="configurator">An optional dynamic config file configurator</param>
        /// <returns></returns>
        public static IDynamicConfigFile CreateFromFileAsXDocument(
            this IDynamicConfigFileFactory source,
            string filePath,
            Action<XDocument> fileConfigurationConfigurator,
            Action<IDynamicConfigFileConfiguration> configurator = null)
        {
            return CreateWithConfigurator(
                source,
                configurator,
                cfg =>
                {                    
                    var xml = XDocument.Load(filePath);
                    fileConfigurationConfigurator(xml);
                    xml.Save(cfg.ConfigurationFileName);
                });            
        }

        /// <summary>
        /// Creates a dynamic config file from an XDocument that is setup as a configuration file
        /// </summary>
        /// <param name="source"></param>
        /// <param name="fileConfigurationConfigurator">An XDocument configurator</param>
        /// <param name="configurator">An optional dynamic config file configurator</param>
        /// <returns></returns>
        public static IDynamicConfigFile CreateFromXDocument(
            this IDynamicConfigFileFactory source,
            Action<XDocument> fileConfigurationConfigurator,
            Action<IDynamicConfigFileConfiguration> configurator = null)
        {
            return CreateWithConfigurator(
                source,
                configurator,
                cfg =>
                {
                    var xml = XDocument.Parse($"<{ConfigFileXElementExtensions._configurationKey}/>");
                    fileConfigurationConfigurator(xml);
                    xml.Save(cfg.ConfigurationFileName);
                });            
        }        
    }
}