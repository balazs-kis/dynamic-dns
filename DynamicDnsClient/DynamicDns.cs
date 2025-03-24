using DynamicDnsClient.Clients;
using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;
using DynamicDnsClient.Saving;

namespace DynamicDnsClient;

public class DynamicDns
{
    private readonly IConfigReader _configReader;
    private readonly IPublicIpHttpClient _publicIpHttpClient;
    private readonly IDynamicDnsHttpClient _dynamicDnsHttpClient;
    private readonly IPersistentSateHandler _persistentSateHandler;

    public DynamicDns(
        IConfigReader configReader,
        IPublicIpHttpClient publicIpHttpClient,
        IDynamicDnsHttpClient dynamicDnsHttpClient,
        IPersistentSateHandler persistentSateHandler)
    {
        _configReader = configReader;
        _publicIpHttpClient = publicIpHttpClient;
        _dynamicDnsHttpClient = dynamicDnsHttpClient;
        _persistentSateHandler = persistentSateHandler;
    }
    
    public async Task UpdateIpAddressesAsync()
    {
        var config = await _configReader.ReadConfigurationAsync();
        if (config is null)
        {
            ConsoleLogger.LogWarning($"Dynamic DNS client is exiting due to invalid configuration.{Environment.NewLine}");
            return;
        }
        
        ConsoleLogger.LogTrace("Dynamic DNS client started.");

        var lastUpdatedPublicIp = await _persistentSateHandler.GetLastUpdatedPublicIpAsync();

        var newPublicIp = await _publicIpHttpClient.GetPublicIpAsync();
        if (newPublicIp is null)
        {
            ConsoleLogger.LogTrace(
                $"Dynamic DNS client is exiting due to being unable to obtain public IP.{Environment.NewLine}");
    
            return;
        }

        if (string.Equals(newPublicIp, lastUpdatedPublicIp))
        {
            ConsoleLogger.LogTrace($"Dynamic DNS client is exiting: IP update is not needed.{Environment.NewLine}");
            return;
        }

        if (await _dynamicDnsHttpClient.UpdateIpForDnsAsync(newPublicIp))
        {
            await _persistentSateHandler.UpdateLastUpdatedPublicIpAsync(newPublicIp);
            ConsoleLogger.LogTrace($"Dynamic DNS client is exiting: run completed.{Environment.NewLine}");
        }
        else
        {
            ConsoleLogger.LogTrace(
                $"Dynamic DNS client is exiting due to being unable to update public IP.{Environment.NewLine}");
        }
    }
}
