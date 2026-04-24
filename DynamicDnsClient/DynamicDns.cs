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
    private readonly IPersistentStateHandler _persistentStateHandler;
    private readonly ILogger _logger;

    public DynamicDns(
        IConfigReader configReader,
        IPublicIpHttpClient publicIpHttpClient,
        IDynamicDnsHttpClient dynamicDnsHttpClient,
        IPersistentStateHandler persistentStateHandler,
        ILogger logger)
    {
        _configReader = configReader;
        _publicIpHttpClient = publicIpHttpClient;
        _dynamicDnsHttpClient = dynamicDnsHttpClient;
        _persistentStateHandler = persistentStateHandler;
        _logger = logger;
    }
    
    public async Task UpdateIpAddressesAsync()
    {
        var config = await _configReader.ReadConfigurationAsync();
        if (config is null)
        {
            _logger.LogError("Dynamic DNS client is exiting due to invalid configuration.");
            return;
        }
        
        _logger.LogTrace("Dynamic DNS client started.");

        var lastUpdatedPublicIp = await _persistentStateHandler.GetLastUpdatedPublicIpAsync();

        var newPublicIp = await _publicIpHttpClient.GetPublicIpAsync();
        if (newPublicIp is null)
        {
            _logger.LogError(
                "Dynamic DNS client is exiting due to being unable to obtain public IP.");

            return;
        }

        if (string.Equals(newPublicIp, lastUpdatedPublicIp))
        {
            _logger.LogTrace("Dynamic DNS client is exiting: IP update is not needed.");
            return;
        }

        if (await _dynamicDnsHttpClient.UpdateIpForDnsAsync(newPublicIp))
        {
            await _persistentStateHandler.UpdateLastUpdatedPublicIpAsync(newPublicIp);
            _logger.LogTrace("Dynamic DNS client is exiting: run completed.");
        }
        else
        {
            _logger.LogError("Dynamic DNS client was unable to update public IP.");
        }
    }
}
