using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Saving;

public class PersistentSateHandler : IPersistentSateHandler
{
    private const string DefaultIp = "127.0.0.1";
    
    private readonly IConfigReader _configReader;

    public PersistentSateHandler(IConfigReader configReader)
    {
        _configReader = configReader;
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
                ConsoleLogger.LogTrace($"Read last updated public IP from file: {lastUpdatedPublicIp}");
                
                return lastUpdatedPublicIp;
            }
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogWarning(
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
            ConsoleLogger.LogTrace($"Wrote last updated public IP to file: {newIp}");
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogWarning(
                $"Could not write the last updated public IP to file. {ex.GetType().Name}: {ex.Message}");
        }
    }
}
