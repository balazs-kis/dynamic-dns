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
        var persistentStateHandler = new PersistentStateHandler(_configReader, _logger);
        
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
        await _dynamicDns.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage?.AbsoluteUrl).ToArray();
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
        await _dynamicDns.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage?.AbsoluteUrl).ToArray();
        var savedIp = await File.ReadAllTextAsync(_runConfig.SavedStateFilePath, _ct);
        
        Assert.Equal(ip, savedIp);
        Assert.DoesNotContain(_logger.Logs!, msg => msg.Contains("[ERR]"));
        GetExpectedUrlCalls(ip).ForEach(expectedUrl => Assert.Contains(expectedUrl, requestedUrls));
    }

    [Fact]
    public async Task UpdatesAllInstancesWithNewIpWhenFirstProvidersReturnInvalidResponses()
    {
        // Arrange
        const string ip = "90.62.59.132";

        var ipApiUrls = _runConfig.IpProviderUrls!;

        SetUpPublicIpApi(ipApiUrls[0], "<html><body>Error 503</body></html>");
        SetUpPublicIpApi(ipApiUrls[1], "not-an-ip");
        SetUpPublicIpApi(ipApiUrls[2], ip);
        SetUpDdnsApis();

        // Act
        await _dynamicDns.UpdateIpAddressesAsync();

        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage?.AbsoluteUrl).ToArray();
        var savedIp = await File.ReadAllTextAsync(_runConfig.SavedStateFilePath, _ct);

        Assert.Equal(ip, savedIp);
        Assert.DoesNotContain(_logger.Logs!, msg => msg.Contains("[ERR]"));
        Assert.Contains(_logger.Logs!, msg => msg.Contains("[WRN]") && msg.Contains(ipApiUrls[0]));
        Assert.Contains(_logger.Logs!, msg => msg.Contains("[WRN]") && msg.Contains(ipApiUrls[1]));
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
        await _dynamicDns.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage?.AbsoluteUrl).ToArray();
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
        await _dynamicDns.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage?.AbsoluteUrl).ToArray();
        var hasSavedIp = File.Exists(_runConfig.SavedStateFilePath);
        
        Assert.False(hasSavedIp);
        Assert.Contains(_logger.Logs!, msg => msg.Contains("[ERR]"));
        Assert.Empty(requestedUrls);
    }
    
    [Fact]
    public async Task DoesNotUpdateWhenAllPublicIpProvidersReturnInvalidResponses()
    {
        // Arrange
        var ipApiUrls = _runConfig.IpProviderUrls!;

        SetUpPublicIpApi(ipApiUrls[0], "<html><body>Error 503</body></html>");
        SetUpPublicIpApi(ipApiUrls[1], "not-an-ip");
        SetUpPublicIpApi(ipApiUrls[2], "2001:0db8:85a3::8a2e:0370:7334");
        SetUpDdnsApis();

        // Act
        await _dynamicDns.UpdateIpAddressesAsync();

        // Assert
        var hasSavedIp = File.Exists(_runConfig.SavedStateFilePath);

        Assert.False(hasSavedIp);
        Assert.Contains(_logger.Logs!, msg =>
            msg.Contains("[ERR]") && msg.Contains("unable to obtain public IP"));
        ipApiUrls.ForEach(url =>
            Assert.Contains(_logger.Logs!, msg => msg.Contains("[WRN]") && msg.Contains(url)));
    }

    [Fact]
    public async Task UpdatesIpWhenPublicIpProviderThrowsExceptionButNextProviderSucceeds()
    {
        // Arrange
        const string ip = "62.59.90.127";

        var ipApiUrls = _runConfig.IpProviderUrls!;

        _mockServer
            .Given(Request.Create().WithPath(new Uri(ipApiUrls[0]).LocalPath).UsingGet())
            .RespondWith(Response.Create().WithFault(FaultType.MALFORMED_RESPONSE_CHUNK));
        SetUpPublicIpApi(ipApiUrls[1], ip);
        SetUpDdnsApis();

        // Act
        await _dynamicDns.UpdateIpAddressesAsync();

        // Assert
        var savedIp = await File.ReadAllTextAsync(_runConfig.SavedStateFilePath, _ct);

        Assert.Equal(ip, savedIp);
        Assert.DoesNotContain(_logger.Logs!, msg => msg.Contains("[ERR]"));
        Assert.Contains(_logger.Logs!, msg => msg.Contains("[WRN]") && msg.Contains(ipApiUrls[0]));
    }

    [Fact]
    public async Task LogsWarningWhenDdnsApiThrowsException()
    {
        // Arrange
        const string ip = "62.59.90.127";

        SetUpPublicIpApis(ip);

        _runConfig.Instances!.ForEach(i =>
        {
            _mockServer
                .Given(Request.Create().WithPath(new Uri(i.DnsApiUrlTemplate!).LocalPath).UsingGet())
                .RespondWith(Response.Create().WithFault(FaultType.MALFORMED_RESPONSE_CHUNK));
        });

        // Act
        await _dynamicDns.UpdateIpAddressesAsync();

        // Assert
        var hasSavedIp = File.Exists(_runConfig.SavedStateFilePath);

        Assert.False(hasSavedIp);
        Assert.Contains(_logger.Logs!, msg =>
            msg.Contains("[ERR]") && msg.Contains("unable to update public IP"));
    }

    [Fact]
    public async Task LogsWarningWhenStateFileCannotBeWritten()
    {
        // Arrange
        const string ip = "62.59.90.127";

        SetUpPublicIpApis(ip);
        SetUpDdnsApis();

        // Create a directory with the same name as the state file path to cause a write failure
        Directory.CreateDirectory(_runConfig.SavedStateFilePath);

        // Act
        await _dynamicDns.UpdateIpAddressesAsync();

        // Assert
        Assert.Contains(_logger.Logs!, msg => msg.Contains("[WRN]") && msg.Contains("Could not write"));

        // Cleanup
        Directory.Delete(_runConfig.SavedStateFilePath);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LogsErrorAndDoesntSaveNewIpWhenDdnsApiReturnsError(bool isHardFailure)
    {
        // Arrange
        const string ip = "62.59.90.127";
        
        SetUpPublicIpApis(ip);

        if (isHardFailure)
        {
            _runConfig.Instances!.ForEach(i =>
            {
                _mockServer
                    .Given(Request.Create().WithPath(new Uri(i.DnsApiUrlTemplate!).LocalPath).UsingGet())
                    .RespondWith(Response.Create().WithStatusCode(503));
            });
        }
        else
        {
            SetUpDdnsApis(skipSuccessMessage: true);
        }
        
        // Act
        await _dynamicDns.UpdateIpAddressesAsync();
        
        // Assert
        var requestedUrls = _mockServer.LogEntries.Select(l => l.RequestMessage?.AbsoluteUrl).ToArray();
        var hasSavedIp = File.Exists(_runConfig.SavedStateFilePath);
        
        Assert.False(hasSavedIp);
        Assert.Contains(_logger.Logs!, msg => msg.Contains("[ERR]"));
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

    private void SetUpDdnsApis(bool skipSuccessMessage = false) =>
        _runConfig.Instances!.ForEach(i => SetUpDdnsApi(
            i.DnsApiUrlTemplate!, skipSuccessMessage ? string.Empty : i.DnsApiSuccessMessage!));

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
