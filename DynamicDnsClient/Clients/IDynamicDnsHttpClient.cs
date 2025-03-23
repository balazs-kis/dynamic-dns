namespace DynamicDnsClient.Clients;

public interface IDynamicDnsHttpClient
{
    Task<bool> UpdateIpForDnsAsync(string newIp);
}
