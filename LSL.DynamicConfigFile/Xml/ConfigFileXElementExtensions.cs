using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;

namespace LSL.DynamicConfigFile.Xml
{
    public static class ConfigFileXElementExtensions
    {
        private const string AppSettingsKey = "appSettings";

        public static XElement GetSection(this XNode source, string sectionName)
        {
            return source.XPathSelectElement(
                string.Format(
                    "/configuration/{0}",
                    sectionName
                )
            );
        }

        public static XElement GetAppSettings(this XNode source)
        {
            return source.GetSection(AppSettingsKey);
        }

        public static XElement GetConnectionStrings(this XNode source)
        {
            return source.GetSection("connectionStrings");
        }

        public static XNode SetAppSettings(this XNode source, IEnumerable<KeyValuePair<string, string>> appSettings)
        {
            return source.SetKeyValueSection(AppSettingsKey, appSettings);
        }

        public static XNode SetKeyValueSection(
            this XNode source, 
            string sectionName,
            IEnumerable<KeyValuePair<string, string>> settings)
        {
            var sectionNode = source.GetSection(sectionName);

            foreach (var setting in settings)
            {
                sectionNode.SetKeyValueElement(setting.Key, setting.Value);
            }

            return source;
        }

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

        public static XElement SetConnectionString(this XElement connectionStringNode, string key, string value)
        {
            var settingNode = connectionStringNode.XPathSelectElement(
                string.Format(
                    "add[name='{0}']",
                    key));

            if (settingNode == null)
            {
                settingNode = new XElement("add");
                connectionStringNode.Add(settingNode);
            }

            settingNode.Add(new XAttribute("name", key));
            settingNode.Add(new XAttribute("connectionString", value));

            return connectionStringNode;
        }
    }
}