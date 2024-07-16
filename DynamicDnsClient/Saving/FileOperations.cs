using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Saving;

public static class FileOperations
{
    private const string LastUpdatedPublicIpFile = "lastUpdatedPublicIp.txt";
    private const string DefaultIp = "127.0.0.1";
    
    public static string GetLastUpdatedPublicIp()
    {
        try
        {
            if (File.Exists(LastUpdatedPublicIpFile))
            {
                var lastUpdatedPublicIp = File.ReadAllText(LastUpdatedPublicIpFile);
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

    public static void UpdateLastUpdatedPublicIp(string newIp)
    {
        try
        {
            File.WriteAllText(LastUpdatedPublicIpFile, newIp);
            ConsoleLogger.LogTrace($"Wrote last updated public IP to file: {newIp}");
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogWarning(
                $"Could not write the last updated public IP to file. {ex.GetType().Name}: {ex.Message}");
        }
    }
}
