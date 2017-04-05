using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using FluentAssertions;
using NUnit.Framework;
using LSL.DynamicConfigFile.Xml;

namespace LSL.DynamicConfigFile.Tests.DynamicConfigFileFactoryTests
{    
    public class Tests
    {
        private static DynamicConfigFileFactory BuildSut()
        {
            return new DynamicConfigFileFactory();
        }

        [Test]
        public void DynamicConfigFileFactory_WhenSwitchingConfigFiles_ItShouldUseTheNewFile()
        {
            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
            var newConfigFile = Path.Combine(Environment.CurrentDirectory, @"TestConfigs\Test.config");

            using (var dcf = BuildSut()
                .Create(newConfigFile))
            {
                AppSetting("original.key").Should().BeNull();
                AppSetting("dynamic.key").Should().Be("dynamic.key.value");

                dcf.AppDomain.Should().Be(AppDomain.CurrentDomain);
                dcf.ConfigFile.Should().Be(newConfigFile);
            }

            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
        }

        [Test]
        public void DynamicConfigFileFactory_WhenInitialisingFromStringContent_ItShouldUseTheNewFile()
        {
            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
            var stringContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"TestConfigs\Test.config"));
            var newConfigFile = Path.GetTempFileName();
            File.WriteAllText(newConfigFile, stringContent);

            using (var dcf = BuildSut()
                .CreateFromStringContent(stringContent, cfg => cfg.WithConfigurationFileOf(newConfigFile)))
            {
                AppSetting("original.key").Should().BeNull();
                AppSetting("dynamic.key").Should().Be("dynamic.key.value");

                dcf.AppDomain.Should().Be(AppDomain.CurrentDomain);
                dcf.ConfigFile.Should().Be(newConfigFile);
            }

            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
        }

        [Test]
        public void DynamicConfigFileFactory_WhenInitialisingFromExistingConfig_ItShouldUseTheNewFile()
        {
            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
            ConnectionString("DB1").Should().Be("DB1.connection.string");
            ConnectionString("DB2").Should().Be("");
            var newConfigFile = Path.GetTempFileName();

            using (var dcf = BuildSut()
                .CreateFromExistingFileAsXDocument(
                    xml =>
                    {
                        xml
                            .SetAppSettings(new Dictionary<string, string>
                            {
                                {"dynamic.key", "dynamic.key.value"},
                                {"original.key", "original.key.updated" }
                            })
                            .SetConnectionStrings(new Dictionary<string, string>
                            {
                                {"DB2", "DB2.connection.string"}
                            });
                    },
                    cfg => cfg.WithConfigurationFileOf(newConfigFile)))
            {
                AppSetting("original.key").Should().Be("original.key.updated");
                AppSetting("dynamic.key").Should().Be("dynamic.key.value");
                ConnectionString("DB1").Should().Be("DB1.connection.string");
                ConnectionString("DB2").Should().Be("DB2.connection.string");

                dcf.AppDomain.Should().Be(AppDomain.CurrentDomain);
                dcf.ConfigFile.Should().Be(newConfigFile);
            }

            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
            ConnectionString("DB1").Should().Be("DB1.connection.string");
            ConnectionString("DB2").Should().Be("");
        }

        [Test]
        public void DynamicConfigFileFactory_WhenGettingAppSettings_ItShouldReturnTheExpectedResult()
        {
            var xml = XDocument.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var defaultSettings = xml.GetAppSettings();
            var appSettings = defaultSettings.XPathSelectElements("*[@key and @value]");

            appSettings.Count().Should().Be(1);
            appSettings.ElementAt(0).ToString().Should().Be("<add key=\"original.key\" value=\"original.key.value\" />");
        }

        [Test]
        public void DynamicConfigFileFactory_WhenSettingAnArbitraryKeyValueSection_ItShouldSetupAsExpected()
        {
            const string sectionName = "otherAppSettings";

            SectionValue(sectionName, "other.original.key").Should().Be("other.original.key.value");
            SectionValue(sectionName, "other.dynamic.key").Should().BeNull();
            var newConfigFile = Path.GetTempFileName();

            using (var dcf = BuildSut()
                .CreateFromExistingFileAsXDocument(
                    xml =>
                        xml.SetKeyValueSection(sectionName, new Dictionary<string, string>
                        {
                            {"other.dynamic.key", "dynamic.key.value"},
                            {"other.original.key", "original.key.updated"}
                        }),
                    cfg => cfg.WithConfigurationFileOf(newConfigFile)))
            {
                SectionValue(sectionName, "other.original.key").Should().Be("original.key.updated");
                SectionValue(sectionName, "other.dynamic.key").Should().Be("dynamic.key.value");

                dcf.AppDomain.Should().Be(AppDomain.CurrentDomain);
                dcf.ConfigFile.Should().Be(newConfigFile);
            }

            SectionValue(sectionName, "other.original.key").Should().Be("other.original.key.value");
            SectionValue(sectionName, "other.dynamic.key").Should().BeNull();
        }

        [Test]
        public void DynamicConfigFileFactory_WhenUpdateingAnExistingFilesText_ItShouldUseTheNewFile()
        {
            AppSetting("original.key").Should().Be("original.key.value");

            var newConfigFile = Path.GetTempFileName();

            using (var dcf = BuildSut()
                .CreateFromExistingFileAsString(source => source.Replace("original.key", "different-original.key"), 
                cfg => cfg.WithConfigurationFileOf(newConfigFile)))
            {
                AppSetting("original.key").Should().BeNull();
                AppSetting("different-original.key").Should().Be("different-original.key.value");

                dcf.AppDomain.Should().Be(AppDomain.CurrentDomain);
                dcf.ConfigFile.Should().Be(newConfigFile);
            }

            AppSetting("original.key").Should().Be("original.key.value");
        }

        private static string AppSetting(string appSettingKey)
        {
            return ConfigurationManager.AppSettings[appSettingKey];
        }

        private static string SectionValue(string nameValueSectionName, string key)
        {
            return ((NameValueCollection)ConfigurationManager.GetSection(nameValueSectionName))[key];
        }

        private static string ConnectionString(string connectionStringName)
        {
            return (ConfigurationManager.ConnectionStrings[connectionStringName] ?? new ConnectionStringSettings()).ConnectionString;
        }
    }
}