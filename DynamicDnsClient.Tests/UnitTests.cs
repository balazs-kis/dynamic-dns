using DynamicDnsClient.Clients;
using DynamicDnsClient.Configuration;
using DynamicDnsClient.Configuration.Models;
using DynamicDnsClient.Saving;
using DynamicDnsClient.Tests.Tools;

namespace DynamicDnsClient.Tests;

public class UnitTests : IDisposable
{
    private readonly CancellationToken _ct;
    private readonly string _runId;
    private readonly AppConfig _runConfig;
    private readonly DynamicDns _dynamicDns;
    
    public UnitTests()
    {
        _ct = TestContext.Current.CancellationToken;
        (_runId, _runConfig) = ConfigSetup.GenerateConfig();
        
        var configReader = new ConfigReader(_runId);
        var httpClientWrapper = new HttpClientWrapper(new HttpClient());
        var publicIpClient = new PublicIpHttpClient(httpClientWrapper, configReader);
        var dynamicDnsClient = new DynamicDnsHttpClient(httpClientWrapper, configReader);
        var persistentStateHandler = new PersistentSateHandler(configReader);
        
        _dynamicDns = new DynamicDns(configReader, publicIpClient, dynamicDnsClient, persistentStateHandler);
    }
    
    [Fact]
    public async Task Test()
    {
        await _dynamicDns!.UpdateIpAddressesAsync();
    }

    public void Dispose()
    {
        ConfigSetup.CleanupConfig(_runId);
        File.Delete(_runConfig.SavedStateFilePath);
    }
}
