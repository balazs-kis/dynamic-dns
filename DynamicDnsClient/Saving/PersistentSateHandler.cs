using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Saving;

public class PersistentSateHandler : IPersistentSateHandler
{
    private const string DefaultIp = "127.0.0.1";
    
    private readonly IConfigReader _configReader;
    private readonly ILogger _logger;

    public PersistentSateHandler(IConfigReader configReader, ILogger logger)
    {
        _configReader = configReader;
        _logger = logger;
    }
    
    public async Task<string> GetLastUpdatedPublicIpAsync()
    {
        try
        {
            var config = await _configReader.ReadConfigurationAsync();
            var savedStateFilePath = config!.SavedStateFilePath;
            
            if (File.Exists(savedStateFilePath))
            {
                var lastUpdatedPublicIp = await File.ReadAllTextAsync(savedStateFilePath);
                _logger.LogTrace($"Read last updated public IP from file: {lastUpdatedPublicIp}");
                
                return lastUpdatedPublicIp;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"Could not read the last updated public IP from file. {ex.GetType().Name}: {ex.Message}");
        }

        return DefaultIp;
    }

    public async Task UpdateLastUpdatedPublicIpAsync(string newIp)
    {
        try
        {
            var config = await _configReader.ReadConfigurationAsync();
            var savedStateFilePath = config!.SavedStateFilePath;
            
            await File.WriteAllTextAsync(savedStateFilePath, newIp);
            _logger.LogTrace($"Wrote last updated public IP to file: {newIp}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"Could not write the last updated public IP to file. {ex.GetType().Name}: {ex.Message}");
        }
    }
}
