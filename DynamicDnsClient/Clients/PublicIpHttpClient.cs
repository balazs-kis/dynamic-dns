using System.Net;
using System.Net.Sockets;
using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Clients;

public class PublicIpHttpClient : IPublicIpHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfigReader _configReader;
    private readonly ILogger _logger;

    public PublicIpHttpClient(HttpClient httpClient, IConfigReader configReader, ILogger logger)
    {
        _httpClient = httpClient;
        _configReader = configReader;
        _logger = logger;
    }
    
    public async Task<string?> GetPublicIpAsync()
    {
        var appConfig = await _configReader.ReadConfigurationAsync();

        foreach (var ipProviderUrl in appConfig!.IpProviderUrls!)
        {
            try
            {
                using var request = new HttpRequestMessage();

                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(ipProviderUrl);

                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        $"Could not obtain public IP address from '{ipProviderUrl}'. " +
                        $"Status code: {response.StatusCode}");

                    continue;
                }

                var publicIp = (await response.Content.ReadAsStringAsync()).TrimEnd();

                if (!IPAddress.TryParse(publicIp, out var address) ||
                    address.AddressFamily != AddressFamily.InterNetwork)
                {
                    var truncatedResponse = publicIp.Length > 50
                        ? publicIp[..50] + "..."
                        : publicIp;

                    _logger.LogWarning(
                        $"Could not obtain public IP address from '{ipProviderUrl}'. " +
                        $"Response is not a valid IPv4 address: '{truncatedResponse}'");

                    continue;
                }

                _logger.LogTrace($"Public IP obtained from '{ipProviderUrl}': {publicIp}");
                return publicIp;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Could not obtain public IP address from '{ipProviderUrl}'. " +
                    $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        _logger.LogWarning("Could not obtain public IP address from any configured providers.");
        return null;
    }
}
