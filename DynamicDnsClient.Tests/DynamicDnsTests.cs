using DynamicDnsClient.Clients;
using DynamicDnsClient.Configuration;
using DynamicDnsClient.Configuration.Models;
using DynamicDnsClient.Logging;
using DynamicDnsClient.Saving;
using DynamicDnsClient.Tests.Tools;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit.Internal;

namespace DynamicDnsClient.Tests;

public class DynamicDnsTests : IDisposable
{
    private readonly CancellationToken _ct;
    private readonly WireMockServer _mockServer;
    private readonly ConsoleLogger _logger;
    private readonly ConfigReader _configReader;
    private readonly AppConfig _runConfig;
    private readonly DynamicDns _dynamicDns;

    public DynamicDnsTests()
    {
        var runId = DataGenerator.GenerateWord();
        
        _ct = TestContext.Current.CancellationToken;
        _logger = new ConsoleLogger(true);
        _mockServer = WireMockServer.Start();
        _runConfig = ConfigSetup.GenerateConfig(runId, _mockServer.Urls[0]);
        _configReader = new ConfigReader(_logger, runId);

        _logger.Logs = new List<string>(25);
        
        var httpClient = new HttpClient();
        var publicIpClient = new PublicIpHttpClient(httpClient, _configReader, _logger);
        var dynamicDnsClient = new DynamicDnsHttpClient(httpClient, _configReader, _logger);
        var persistentStateHandler = new PersistentSateHandler(_configReader, _logger);
        
        _dynamicDns = new DynamicDns(_configReader, publicIpClient, dynamicDnsClient, persistentStateHandler, _logger);
    }

    [Fact]
    public async Task UpdatesAllInstancesWithNewIp()
    {
        // Arrange
        const string ip = "62.59.90.127";
        
        SetUpPublicIpApis(ip);
        SetUpDdnsApis();
        
        // Act
        await _dynamicDns!.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage.AbsoluteUrl).ToArray();
        var savedIp = await File.ReadAllTextAsync(_runConfig.SavedStateFilePath, _ct);
        
        Assert.Equal(ip, savedIp);
        Assert.DoesNotContain(_logger.Logs!, msg => msg.Contains("[ERR]"));
        GetExpectedUrlCalls(ip).ForEach(expectedUrl => Assert.Contains(expectedUrl, requestedUrls));
    }

    [Fact]
    public async Task UpdatesAllInstancesWithNewIpWhenOnlyOnePublicIpProviderIsAvailable()
    {
        // Arrange
        const string ip = "132.59.62.90";

        var ipApiUrl = _runConfig.IpProviderUrls!.Last();
        
        SetUpPublicIpApi(ipApiUrl, ip);
        SetUpDdnsApis();
        
        // Act
        await _dynamicDns!.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage.AbsoluteUrl).ToArray();
        var savedIp = await File.ReadAllTextAsync(_runConfig.SavedStateFilePath, _ct);
        
        Assert.Equal(ip, savedIp);
        Assert.DoesNotContain(_logger.Logs!, msg => msg.Contains("[ERR]"));
        GetExpectedUrlCalls(ip).ForEach(expectedUrl => Assert.Contains(expectedUrl, requestedUrls));
    }

    [Fact]
    public async Task DoesNotUpdateWhenThereIsNoPublicIpChange()
    {
        // Arrange
        const string ip = "62.59.90.127";
        
        SetUpPublicIpApis(ip);
        SetUpDdnsApis();

        await File.WriteAllTextAsync(_runConfig.SavedStateFilePath, ip, _ct);
        
        // Act
        await _dynamicDns!.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage.AbsoluteUrl).ToArray();
        var savedIp = await File.ReadAllTextAsync(_runConfig.SavedStateFilePath, _ct);
        
        Assert.Equal(ip, savedIp);
        Assert.DoesNotContain(_logger.Logs!, msg => msg.Contains("[ERR]"));
        GetExpectedUrlCalls(ip).ForEach(expectedUrl => Assert.DoesNotContain(expectedUrl, requestedUrls));
    }

    [Fact]
    public async Task DoesNotRunWhenThereIsNoValidConfiguration()
    {
        // Arrange
        const string ip = "62.59.90.127";
        
        SetUpPublicIpApis(ip);
        SetUpDdnsApis();
        
        File.Delete(_configReader.AppConfigPath);
        
        // Act
        await _dynamicDns!.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage.AbsoluteUrl).ToArray();
        var hasSavedIp = File.Exists(_runConfig.SavedStateFilePath);
        
        Assert.False(hasSavedIp);
        Assert.Contains(_logger.Logs!, msg => msg.Contains("[ERR]"));
        Assert.Empty(requestedUrls);
    }

    private IEnumerable<string> GetExpectedUrlCalls(string newIp)
    {
        foreach (var instance in _runConfig.Instances!)
        {
            foreach (var host in instance.Hosts!)
            {
                yield return instance.DnsApiUrlTemplate!
                    .Replace(AppConfig.HostNamePlaceholder, host)
                    .Replace(AppConfig.DomainNamePlaceholder, instance.DomainName)
                    .Replace(AppConfig.DnsApiSecretPlaceholder, instance.DnsApiSecret)
                    .Replace(AppConfig.UpdatedIpPlaceholder, newIp);
            }
        }
    }

    private void SetUpPublicIpApis(string publicIp) =>
        _runConfig.IpProviderUrls!.ForEach(ipProviderUrl => SetUpPublicIpApi(ipProviderUrl, publicIp));

    private void SetUpPublicIpApi(string ipProviderUrl, string publicIp) =>
        _mockServer
            .Given(Request.Create().WithPath(new Uri(ipProviderUrl).LocalPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(publicIp));

    private void SetUpDdnsApis() =>
        _runConfig.Instances!.ForEach(i => SetUpDdnsApi(i.DnsApiUrlTemplate!, i.DnsApiSuccessMessage!));

    private void SetUpDdnsApi(string ddnsUrl, string successMessage) =>
        _mockServer
            .Given(Request.Create().WithPath(new Uri(ddnsUrl).LocalPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(successMessage));

    public void Dispose()
    {
        _mockServer.Stop();
        File.Delete(_configReader.AppConfigPath);
        File.Delete(_runConfig.SavedStateFilePath);
    }
}
