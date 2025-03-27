using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;
using DynamicDnsClient.Tests.Tools;

namespace DynamicDnsClient.Tests;

public class ConfigReaderTests : IDisposable
{
    private readonly CancellationToken _ct;
    private readonly ConfigReader _configReader;
    private readonly ConsoleLogger _logger;

    public ConfigReaderTests()
    {
        var runId = DataGenerator.GenerateWord();

        _ct = TestContext.Current.CancellationToken;
        _logger = new ConsoleLogger(true);
        _configReader = new ConfigReader(_logger, runId);

        _logger.Logs = new List<string>(25);
    }

    [Fact]
    public async Task ReturnsNullIfConfigFileIsNotFound() => await TestInvalidConfig(null);

    [Fact]
    public async Task ReturnsNullIfConfigFileIsNotValidJson() => await TestInvalidConfig("Not a valid json string.");

    [Fact]
    public async Task ReturnsNullIfConfigFileContainsNull() => await TestInvalidConfig("null");

    [Fact]
    public async Task ReturnsNullIfSavedStatePathIsNotProvided() => await TestInvalidConfig(
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
        """);

    [Fact]
    public async Task ReturnsNullIfDomainNameIsNotProvided() => await TestInvalidConfig(
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
        """);

    [Fact]
    public async Task ReturnsNullIfDnsApiSecretIsNotProvided() => await TestInvalidConfig(
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
        """);

    [Fact]
    public async Task ReturnsNullIfDnsApiUrlTemplateIsNotProvided() => await TestInvalidConfig(
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
        """);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReturnsNullIfIpProviderUrlsAreNotProvided(bool isNull)
    {
        var ipProviderUrls = isNull ? string.Empty : "\"ipProviderUrls\": [],";
        var configContent =
            $$"""
                {
                  "savedStateFilePath": "lastUpdatedPublicIp.txt",
                  {{ipProviderUrls}}
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
              """;

        await TestInvalidConfig(configContent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReturnsNullIfInstancesAreNotProvided(bool isNull)
    {
        var instances = isNull ? string.Empty : "\"instances\": [],";
        var configContent =
            $$"""
              {
                {{instances}}
                "savedStateFilePath": "lastUpdatedPublicIp.txt",
                "ipProviderUrls": [ "https://ip-provider-api.org" ]
              }
              """;

        await TestInvalidConfig(configContent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReturnsNullIfHostsAreNotProvided(bool isNull)
    {
        var hosts = isNull ? string.Empty : "\"hosts\": [],";
        var configContent =
            $$"""
              {
                "ipProviderUrls": [ "https://ip-provider-api.org" ],
                "instances": [
                  {
                    "domainName": "domain.eu",
                    {{hosts}}
                    "dnsApiSecret": "ddns-secret",
                    "dnsApiUrlTemplate": "https://ddns.com/update?host={Host}&domain={Domain}&password={Secret}&ip={NewIp}",
                    "dnsApiSuccessMessage": "success-message"
                  }
                ]
              }
              """;

        await TestInvalidConfig(configContent);
    }

    private async Task TestInvalidConfig(string? configContent)
    {
        // Arrange
        if (configContent is not null)
        {
            await File.WriteAllTextAsync(_configReader.AppConfigPath, configContent, _ct);
        }

        // Act
        var config = await _configReader.ReadConfigurationAsync();

        // Assert
        Assert.Null(config);
        Assert.Contains(_logger.Logs!, msg => msg.Contains("[ERR]"));
    }

    public void Dispose()
    {
        File.Delete(_configReader.AppConfigPath);
    }
}
