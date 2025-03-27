namespace DynamicDnsClient.Clients;

public interface IPublicIpHttpClient
{
    Task<string?> GetPublicIpAsync();
}
