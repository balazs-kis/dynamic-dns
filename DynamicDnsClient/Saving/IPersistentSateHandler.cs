namespace DynamicDnsClient.Saving;

public interface IPersistentSateHandler
{
    Task<string> GetLastUpdatedPublicIpAsync();
    Task UpdateLastUpdatedPublicIpAsync(string newIp);
}
