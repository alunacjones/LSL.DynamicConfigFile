using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LSL.DynamicConfigFile.Xml
{
    /// <summary>
    /// ConfigFileXElementExtensions
    /// </summary>
    public static class ConfigFileXElementExtensions
    {
        private const string _appSettingsKey = "appSettings";

        /// <summary>
        /// Retrieve a configuration section by name from an <c>XNode</c>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sectionName">The section name to retrieve</param>
        /// <param name="createIfMissing">Will create the section if it doesn't exist</param>
        /// <returns></returns>
        public static XElement GetSection(this XNode source, string sectionName, bool createIfMissing = false)
        {
            var configurationElement = source.GetConfigurationElement();
            var result = configurationElement.XPathSelectElement(sectionName);

            if (result == null && createIfMissing)
            {
                return configurationElement.AddElement(new XElement(sectionName));
            }

            return result;
        }

        internal const string _configurationKey = "configuration";
        internal const string _configurationSelector = $"/{_configurationKey}";

        internal static XElement GetConfigurationElement(this XNode source) => 
            source.XPathSelectElement(_configurationSelector)
                ?? throw new ArgumentException("Provided node must have a root of 'configuration'");

        /// <summary>
        /// Adds an element to an <c>XNode</c>
        /// </summary>
        /// <remarks>
        /// Will only accept an <c>XDocument</c> or an <c>XElement</c> otherwise it throws an exception
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="element">The element to add</param>
        /// <returns></returns>
        public static XElement AddElement(this XNode source, XElement element)
        {
            if (source is XDocument doc)
            {
                doc.Root.Add(element);
                return element;
            }

            if (source is XElement el)
            {
                el.Add(element);
                return element;
            }

            throw new ArgumentException($"Can only add elements to an XDocument or XElement. Provided type: {source.GetType().FullName}");
        }

        /// <summary>
        /// Retrieve the <c>appSettings</c> section from the <c>XNode</c>
        /// </summary>
        /// <param name="source"></param>
        /// <returns>The <c>appSettings</c> section</returns>
        public static XElement GetAppSettings(this XNode source) => source.GetSection(_appSettingsKey, true);

        /// <summary>
        /// Get the <c>connectionStrings</c> section from the <c>XNode</c>
        /// </summary>
        /// <param name="source"></param>
        /// <returns>The <c>connectionStrings</c> section</returns>
        public static XElement GetConnectionStrings(this XNode source) => source.GetSection("connectionStrings", true);

        /// <summary>
        /// Set the <c>appSettings</c> section from the given values
        /// </summary>
        /// <param name="source"></param>
        /// <param name="appSettings">The values to use for <c>appSettings</c></param>
        /// <returns>The original <c>XNode</c></returns>
        public static XNode SetAppSettings(this XNode source, IEnumerable<KeyValuePair<string, string>> appSettings)
        {
            return source.SetKeyValueSection(_appSettingsKey, appSettings);
        }

        /// <summary>
        /// Sets key/value pairs for a given section name
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sectionName">The section name to provide key/value pairs for</param>
        /// <param name="settings">the key/value pairs</param>
        /// <returns>The original <c>XNode</c></returns>
        public static XNode SetKeyValueSection(
            this XNode source, 
            string sectionName,
            IEnumerable<KeyValuePair<string, string>> settings)
        {
            var sectionNode = source.GetSection(sectionName, true);

            foreach (var setting in settings)
            {
                sectionNode.SetKeyValueElement(setting.Key, setting.Value);
            }

            return source;
        }

        /// <summary>
        /// Sets a key/value pair for a given <c>XElement</c> parent node
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="key">The key of the element to set</param>
        /// <param name="value">The value to set</param>
        /// <remarks>
        /// If an element with the given key doesn't exist then it is added as a child element
        /// with <c>key</c> and <c>value</c> attributes being set to the given values.
        /// </remarks>
        /// <returns>The original <c>XElement</c></returns>
        public static XElement SetKeyValueElement(this XElement parentNode, string key, string value)
        {
            var settingNode = parentNode.XPathSelectElement(
                string.Format(
                    "add[key='{0}']",
                    key));

            if (settingNode == null)
            {
                settingNode = new XElement("add");
                parentNode.Add(settingNode);
            }

            settingNode.Add(new XAttribute("key", key));
            settingNode.Add(new XAttribute("value", value));

            return parentNode;      
        }

        /// <summary>
        /// Sets the values for the <c>connectionStrings</c> section
        /// </summary>
        /// <param name="source"></param>
        /// <param name="connectionStrings">The connection string values</param>
        /// <returns>The original <c>XNode</c></returns>
        public static XNode SetConnectionStrings(this XNode source,
            IEnumerable<KeyValuePair<string, string>> connectionStrings)
        {
            var connectionStringsNode = source.GetConnectionStrings();

            foreach (var connectionString in connectionStrings)
            {
                connectionStringsNode.SetConnectionString(connectionString.Key, connectionString.Value);
            }

            return source;
        }

        /// <summary>
        /// Sets a <c>connectionString</c>s
        /// </summary>
        /// <param name="connectionStringNode"></param>
        /// <param name="name">The connection string name</param>
        /// <param name="connectionString">The connection string value</param>
        /// <returns></returns>
        public static XElement SetConnectionString(this XElement connectionStringNode, string name, string connectionString)
        {
            var settingNode = connectionStringNode.XPathSelectElement(
                string.Format(
                    "add[name='{0}']",
                    name));

            if (settingNode == null)
            {
                settingNode = new XElement("add");
                connectionStringNode.Add(settingNode);
            }

            settingNode.Add(new XAttribute("name", name));
            settingNode.Add(new XAttribute("connectionString", connectionString));

            return connectionStringNode;
        }
    }
}