namespace DynamicDnsClient.Saving;

public interface IPersistentStateHandler
{
    Task<string> GetLastUpdatedPublicIpAsync();
    Task UpdateLastUpdatedPublicIpAsync(string newIp);
}
