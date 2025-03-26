using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;
using DynamicDnsClient.Tests.Tools;

namespace DynamicDnsClient.Tests;

public class ConfigReaderTests : IDisposable
{
    private readonly CancellationToken _ct;
    private readonly ConfigReader _configReader;

    public ConfigReaderTests()
    {
        var runId = DataGenerator.GenerateWord();

        _ct = TestContext.Current.CancellationToken;
        _configReader = new ConfigReader(runId);
    }

    //[Fact]
    public async Task ReturnsNullIfConfigFileIsNotFound()
    {
        // Arrange
        // No setup is needed.
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }
    
    //[Fact]
    public async Task ReturnsNullIfConfigFileIsNotValidJson()
    {
        // Arrange
        await File.WriteAllTextAsync(_configReader.AppConfigPath, "Not a valid json string.", _ct);
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }
    
    //[Fact]
    public async Task ReturnsNullIfSavedStatePathIsNotProvided()
    {
        // Arrange
        await File.WriteAllTextAsync(
            _configReader.AppConfigPath,
            """
            {
              "ipProviderUrls": [ "https://ip-provider-api.org" ],
              "instances": [
                {
                  "domainName": "domain.eu",
                  "hosts": ["@", "*"],
                  "dnsApiSecret": "ddns-secret",
                  "dnsApiUrlTemplate": "https://ddns.com/update?host={Host}&domain={Domain}&password={Secret}&ip={NewIp}",
                  "dnsApiSuccessMessage": "success-message"
                }
              ]
            }
            """,
            _ct);
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }
    
    //[Fact]
    public async Task ReturnsNullIfIpProviderUrlsAreNotProvided()
    {
        // Arrange
        await File.WriteAllTextAsync(
            _configReader.AppConfigPath,
            """
            {
              "savedStateFilePath": "lastUpdatedPublicIp.txt",
              "ipProviderUrls": [],
              "instances": [
                {
                  "domainName": "domain.eu",
                  "hosts": ["@", "*"],
                  "dnsApiSecret": "ddns-secret",
                  "dnsApiUrlTemplate": "https://ddns.com/update?host={Host}&domain={Domain}&password={Secret}&ip={NewIp}",
                  "dnsApiSuccessMessage": "success-message"
                }
              ]
            }
            """,
            _ct);
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }

    //[Fact]
    public async Task ReturnsNullIfInstancesAreNotProvided()
    {
        // Arrange
        await File.WriteAllTextAsync(
            _configReader.AppConfigPath,
            """
            {
              "savedStateFilePath": "lastUpdatedPublicIp.txt",
              "ipProviderUrls": [ "https://ip-provider-api.org" ],
              "instances": []
            }
            """,
            _ct);
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }

    //[Fact]
    public async Task ReturnsNullIfDomainNameIsNotProvided()
    {
        // Arrange
        await File.WriteAllTextAsync(
            _configReader.AppConfigPath,
            """
            {
              "ipProviderUrls": [ "https://ip-provider-api.org" ],
              "instances": [
                {
                  "hosts": ["@", "*"],
                  "dnsApiSecret": "ddns-secret",
                  "dnsApiUrlTemplate": "https://ddns.com/update?host={Host}&domain={Domain}&password={Secret}&ip={NewIp}",
                  "dnsApiSuccessMessage": "success-message"
                }
              ]
            }
            """,
            _ct);
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }
    
    //[Fact]
    public async Task ReturnsNullIfHostsAreNotProvided()
    {
        // Arrange
        await File.WriteAllTextAsync(
            _configReader.AppConfigPath,
            """
            {
              "ipProviderUrls": [ "https://ip-provider-api.org" ],
              "instances": [
                {
                  "domainName": "domain.eu",
                  "hosts": [],
                  "dnsApiSecret": "ddns-secret",
                  "dnsApiUrlTemplate": "https://ddns.com/update?host={Host}&domain={Domain}&password={Secret}&ip={NewIp}",
                  "dnsApiSuccessMessage": "success-message"
                }
              ]
            }
            """,
            _ct);
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }
    
    //[Fact]
    public async Task ReturnsNullIfDnsApiSecretIsNotProvided()
    {
        // Arrange
        await File.WriteAllTextAsync(
            _configReader.AppConfigPath,
            """
            {
              "ipProviderUrls": [ "https://ip-provider-api.org" ],
              "instances": [
                {
                  "domainName": "domain.eu",
                  "hosts": ["@", "*"],
                  "dnsApiUrlTemplate": "https://ddns.com/update?host={Host}&domain={Domain}&password={Secret}&ip={NewIp}",
                  "dnsApiSuccessMessage": "success-message"
                }
              ]
            }
            """,
            _ct);
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }
    
    //[Fact]
    public async Task ReturnsNullIfDnsApiUrlTemplateIsNotProvided()
    {
        // Arrange
        await File.WriteAllTextAsync(
            _configReader.AppConfigPath,
            """
            {
              "ipProviderUrls": [ "https://ip-provider-api.org" ],
              "instances": [
                {
                  "domainName": "domain.eu",
                  "hosts": ["@", "*"],
                  "dnsApiSecret": "ddns-secret",
                  "dnsApiSuccessMessage": "success-message"
                }
              ]
            }
            """,
            _ct);
        
        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
    }

    public void Dispose()
    {
        File.Delete(_configReader.AppConfigPath);
    }
}