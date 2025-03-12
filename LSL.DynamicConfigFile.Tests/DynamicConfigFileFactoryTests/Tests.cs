using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using LSL.DynamicConfigFile.Xml;
using System.Xml.Linq;
using FluentAssertions.Execution;

namespace LSL.DynamicConfigFile.Tests.DynamicConfigFileFactoryTests
{    
    public class Tests
    {
        private static DynamicConfigFileFactory BuildSut() => new DynamicConfigFileFactory();

        [TestCase("Test.config", "dynamic.key.value")]
        [TestCase("Empty.config", null)]
        public void DynamicConfigFileFactory_WhenSwitchingConfigFiles_ItShouldUseTheNewFile(string file, string expectedDynamicKeyValue)
        {
            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
            var newConfigFile = Path.Combine(AppContext.BaseDirectory, $@"TestConfigs\{file}");

            using (var dcf = BuildSut()
                .Create(newConfigFile))
            {
                AppSetting("original.key").Should().BeNull();
                AppSetting("dynamic.key").Should().Be(expectedDynamicKeyValue);

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
            var stringContent = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, @"TestConfigs\Test.config"));
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

        [TestCase("Test.config")]
        [TestCase("Empty.config")]
        public void DynamicConfigFileFactory_WhenInitialisingFromAFile_ItShouldUseTheNewFile(string file)
        {
            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
            ConnectionString("DB1").Should().Be("DB1.connection.string");
            ConnectionString("DB2").Should().Be("");
            var newConfigFile = Path.GetTempFileName();

            using (var dcf = BuildSut()
                .CreateFromFileAsXDocument(
                    Path.Combine(AppContext.BaseDirectory, $@"TestConfigs\{file}"),
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
                ConnectionString("DB1").Should().Be("");
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
        public void DynamicConfigFileFactory_WhenInitialisingFromAnXDocument_ItShouldUseTheNewFile()
        {
            AppSetting("original.key").Should().Be("original.key.value");
            AppSetting("dynamic.key").Should().BeNull();
            ConnectionString("DB1").Should().Be("DB1.connection.string");
            ConnectionString("DB2").Should().Be("");
            var newConfigFile = Path.GetTempFileName();

            using (var dcf = BuildSut()
                .CreateFromXDocument(
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
                ConnectionString("DB1").Should().Be("");
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
        public void AddElement_GivenAnElementAsTheSource_ItShouldAddTheNewElement()
        {
            var doc = XDocument.Parse("<configuration/>");
            var app = doc.GetAppSettings();
            app.AddElement(new XElement("Als"));
            app.Element("Als").Should().NotBeNull();
        }

        [Test]
        public void AddElement_GivenAnXDocumentAsTheSource_ItShouldAddTheNewElement()
        {
            var doc = XDocument.Parse("<configuration/>");
            doc.AddElement(new XElement("Als"));
            doc.Root.Element("Als").Should().NotBeNull();
        }

        [Test]
        public void AddElement_GivenAnInvalidSourceSource_ItShouldThrowAnException()
        {
            void Test(XNode source) => new Action(() => source.AddElement(new XElement("no-care"))).Should().Throw<ArgumentException>();

            using (var scope = new AssertionScope())
            {
                Test(new XText("value"));
                Test(new XComment("value"));
                Test(new XProcessingInstruction("target", "value"));
                Test(new XDocumentType("target", "value", "2", "3"));
            }
        }          

        [Test]
        public void GetAppSettings_GivenADocumentWithTheWrongRoot_ItShouldThrowAnException()
        {
            new Action(() => BuildSut().CreateFromFileAsXDocument(
                Path.Combine(AppContext.BaseDirectory, "TestConfigs/WrongRoot.config"),
                xml => xml.SetAppSettings(new Dictionary<string, string> { ["no-care"] = "meh" })                
            )).Should()
            .Throw<ArgumentException>()
            .WithMessage("Provided node must have a root of 'configuration'");
        }
        private static string AppSetting(string appSettingKey) => ConfigurationManager.AppSettings[appSettingKey];

        private static string SectionValue(string nameValueSectionName, string key) => 
            ((NameValueCollection)ConfigurationManager.GetSection(nameValueSectionName))[key];

        private static string ConnectionString(string connectionStringName) => 
            (ConfigurationManager.ConnectionStrings[connectionStringName] ?? new ConnectionStringSettings()).ConnectionString;
    }
}