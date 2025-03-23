using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Clients;

public class PublicIpHttpClient : IPublicIpHttpClient
{
    private readonly IHttpClient _httpClient;
    private readonly IConfigReader _configReader;

    public PublicIpHttpClient(IHttpClient httpClient, IConfigReader configReader)
    {
        _httpClient = httpClient;
        _configReader = configReader;
    }
    
    public async Task<string?> GetPublicIpAsync()
    {
        try
        {
            var appConfig = await _configReader.ReadConfigurationAsync();
            
            foreach (var ipProviderUrl in appConfig!.IpProviderUrls!)
            {
                using var request = new HttpRequestMessage();

                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(ipProviderUrl);

                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    ConsoleLogger.LogWarning(
                        $"Could not obtain public IP address from '{ipProviderUrl}'. " +
                        $"Status code: {response.StatusCode}");
                    
                    continue;
                }
                    
                var publicIp = (await response.Content.ReadAsStringAsync()).TrimEnd();

                ConsoleLogger.LogTrace($"Public IP obtained from '{ipProviderUrl}': {publicIp}");
                return publicIp;
            }
            
            ConsoleLogger.LogWarning("Could not obtain public IP address from any configured providers.");
            return null;
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogWarning($"Could not obtain public IP address. {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }
}
