# Summary

A package to provide a disposable dynamic configuration file.

<!-- MarkdownTOC -->

- LSL.DynamicConfigFile package
    - IDynamicConfigFileFactory

<!-- /MarkdownTOC -->

# LSL.DynamicConfigFile package

## IDynamicConfigFileFactory

The default implementation provides an instance that can be used in non-IOC applications.

The interface defines a single entry point that accepts an Action that will be given a configuration object for setting up the dynamic configuration file:

```csharp
public interface IDynamicConfigFileFactory
{
    IDynamicConfigFile Create(Action<IDynamicConfigFileConfiguration> configurator);
}
```

A simple sample that creates a disposable configuration file for the current AppDomain:

```csharp
using (var new DynamicConfigFileFactory()
    .Create(cfg => {
        cfg.WithAppDomainOf(AppDomain.CurrentDomain)
            .WithConfigurationFileOf("TheNewConfigFile.config");
    }))
{
    //within the using clause we are using "TheNewConfigFile.config"
}

//Outside the using block we are back to the original config file for the appDomain
```


<a name="extension-methods-for-quicker-creation"></a>
### Extension methods for quicker creation
All the following examples assume a configuration file of:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="original.key" value="original.key.value"/>
  </appSettings>
</configuration>
```

The dynamic config file is defined as:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="dynamic.key" value="dynamic.key.value"/>
  </appSettings>
</configuration>
```

Assuming a helper method to test the value of an appSetting defined as:

```csharp
    private static void TestConfigValue(string appSetingKey, string expectedValue)
    {
        ConfigurationManager.AppSettings[appSetingKey]
            .Should()
            .Be(expectedValue);
    }
```


```csharp
/*
    Before applying a dynamic config we expect the
    original values to be present
*/
TestConfigValue("original.key", "original.key.value");
TestConfigValue("dynamic.key", null);

var newConfigFile = Path.Combine(Environment.CurrentDirectory, @"TestConfigs\Test.config");

using (var dcf = new DynamicConfigFileFactory()
    .Create(AppDomain.CurrentDomain, newConfigFile))
{
    /*
        The original value should have gone
        and the dynamic value should be in the settings
    */
    TestConfigValue("original.key", null);
    TestConfigValue("dynamic.key", "dynamic.key.value");

    dcf.AppDomain.Should().Be(AppDomain.CurrentDomain);
    dcf.ConfigFile.Should().Be(newConfigFile);
}

/*
    After disposing of the dynamic config file we
    should have reverted back to the original values
*/
TestConfigValue("original.key", "original.key.value");
TestConfigValue("dynamic.key", null);

/*
    A shortcut method can be used to omit the appdomain
*/
using (var dcf = new DynamicConfigFileFactory()
    .Create(newConfigFile)) {} /* Defaults to using AppDomain.CurrentDomain */
```

<a name="createfromstringcontent-extension-method"></a>
### CreateFromStringContent extension method

```csharp
public static IDynamicConfigFile CreateFromStringContent(
            this IDynamicConfigFileFactory source, 
            string stringContent,
            Action<IDynamicConfigFileConfiguration> configurator = null)
```
The following sample shows how to create a dynamic config file from string content:

```csharp
using (var dcf = new DynamicConfigFileFactory()
    .CreateFromStringContent(@"
        <configuration>
            <appSettings>
                <add key=""dynamic"" value=""dynamic.value""/>
            </appSettings >
        </configuration >
    "))
{
        /*
            Within this using clause we will have a new configuration file
            generated from the text given above.

            Since we did not pass in the optional configurator, the default
            AppDomain has its configuration changed to a new file that is calculated by taking the path of the existing configuration file 
            and appending ".dynamic.config" to the end of it.
        */
} 

using (var dcf = new DynamicConfigFileFactory    ()
    .CreateFromStringContent(@"
        <configuration>
            <appSettings>
                <add key=""dynamic"" value=""dynamic.value""/>
            </appSettings >
        </configuration >
    ",
    cfg => cfg.WithConfigurationFileOf(Path.GetTempFileName()))
    {
        /*
            Within this using clause we will have a new configuration file
            generated from the text given above.

            The default AppDomain has its configuration changed to a new file 
            that is provided by The Path.GetTempFileName() function.
        */        
    }
```

<a name="createfromexistingfile-extension-method"></a>
### CreateFromExistingFileAsXDocument Extension Method

```csharp
public static IDynamicConfigFile CreateFromExistingFileAsXDocument(
    this IDynamicConfigFileFactory source,
    Action<XDocument> existingConfigurationConfigurator,
    Action<IDynamicConfigFileConfiguration> configurator = null)
```

This extension method allows us to directly modify the XDocument of a copy of the original configuration file.

```csharp
using (var dcf = new DynamicConfigFileFactory()
    .CreateFromExistingFileAsXDocument(
        xml => {
            /* Manipluate the configuration file */
            var appSettings = new XElement("appSettings");
            xml.Root.Add(appSettings);
        },
        cfg => cfg.WithConfigurationFileOf(Path.GetTempFileName())
    ))
    {
        /*
            Within this using clause we would have the new appSettings section.
            once again the new file is provided by Path.GetTempFileName().

            NOTE: This relies on there NOT being an existing section.
        */
    }
```

<a name="xelement-helper-methods"></a>
#### XElement helper methods

Since it's a bit cumbersome to directly manipulate the XDocument there are a number of helper methods to ease the pain.

<a name="setappsettings"></a>
##### SetAppSettings

```csharp
using (var dcf = new DynamicConfigFileFactory()    
    .CreateFromExistingFileAsXDocument(
        xml => {
            /* add/update appSettings */
            xml
                .SetAppSettings(new Dictionary<string, string>
                {
                    {"dynamic.key", "dynamic.key.value"},
                    {"original.key", "original.key.updated" }
                })
        },
        cfg => cfg.WithConfigurationFileOf(Path.GetTempFileName())
    ))
    {
        /*
            Within this using clause we would have the new appSettings.
            Notice how it either adds or updates existing keys
        */
    }
```

<a name="setconnectionstrings"></a>
##### SetConnectionStrings

```csharp
using (var dcf = new DynamicConfigFileFactory()
    .CreateFromExistingFileAsXDocument(
        xml => {
            /* add/update appSettings */
            xml
                .SetConnectionStrings(new Dictionary<string, string>
                {
                    {"DB2", "DB2.connection.string"}
                });
        },
        cfg => cfg.WithConfigurationFileOf(Path.GetTempFileName())
    ))
    {
        /*
            Within this using clause we would have the new connectionStrings.
            Notice how it either adds or updates existing keys and it does not
            set the provider. This is primarily for changing existing connectionString values
        */
    }
```

<a name="setkeyvalueelement"></a>
##### SetKeyValueElement

Since there are a number of setions that define elements that only have a key and value attribute, this is provided to ease the pain with those sections.

The following sample shows how to dynamically create a quartz.net configuration section:

```csharp
/* 
    This sample DOES require a configSection to be set up to prevent the configuration system from falling over owing to it not knowing what a quartz section should be. The xml would be:

    <configuration>
      <configSections>
        <section name="quartz" type="System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0,Culture=neutral, PublicKeyToken=b77a5c561934e089" />
      </configSections>  
    </configuration>

    This could also be added as part of the dynamic config file initialisation.
*/

using (new DynamicConfigFileFactory()
    .CreateFromExistingFileAsXDocument(xml =>
        {
            var quartzNode = new XElement("quartz");                        
            xml.Root.Add(quartzNode);

            new Dictionary<string, string>
            {
                { "quartz.scheduler.instanceName", "ServerScheduler" + quartzFile } ,
                { "quartz.threadPool.type", "Quartz.Simpl.SimpleThreadPool, Quartz" },
                { "quartz.threadPool.threadCount", "10" },
                { "quartz.threadPool.threadPriority", "Normal" },
                { "quartz.jobStore.misfireThreshold", "60000" },
                { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                { "quartz.plugin.xml.type", "ConsoleSchedulerService.Quartz.JobInitializationPluginWithVariableReplacement, ConsoleSchedulerService" },
                { "quartz.plugin.xml.overwriteExistingJobs", "true" },
                { "quartz.plugin.xml.fileNames", quartzFilePath }
            }.ForEach(kvp => quartzNode.SetKeyValueElement(kvp.Key, kvp.Value));
    },
    cfg => cfg.WithConfigurationFileOf(Path.GetTempFileName())))
{
    /* 
        As always the config file is only available whilst in 
        the using clause. 
    */
}
```
