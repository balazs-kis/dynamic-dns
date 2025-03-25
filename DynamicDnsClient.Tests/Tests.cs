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

public class Tests : IDisposable
{
    private readonly CancellationToken _ct;
    private readonly WireMockServer _mockServer;
    private readonly string _runId;
    private readonly AppConfig _runConfig;
    private readonly DynamicDns _dynamicDns;

    public Tests()
    {
        _ct = TestContext.Current.CancellationToken;
        
        _mockServer = WireMockServer.Start();
        (_runId, _runConfig) = ConfigSetup.GenerateConfig(_mockServer.Urls[0]);
        
        var configReader = new ConfigReader(_runId);
        var httpClient = new HttpClient();
        var publicIpClient = new PublicIpHttpClient(httpClient, configReader);
        var dynamicDnsClient = new DynamicDnsHttpClient(httpClient, configReader);
        var persistentStateHandler = new PersistentSateHandler(configReader);
        
        _dynamicDns = new DynamicDns(configReader, publicIpClient, dynamicDnsClient, persistentStateHandler);
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
        Assert.DoesNotContain(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
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
        Assert.DoesNotContain(ConsoleLogger.Logs, msg => msg.Contains("[ERR]"));
        GetExpectedUrlCalls(ip).ForEach(expectedUrl => Assert.Contains(expectedUrl, requestedUrls));
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
        ConfigSetup.CleanupConfig(_runId);
        File.Delete(_runConfig.SavedStateFilePath);
    }
}
