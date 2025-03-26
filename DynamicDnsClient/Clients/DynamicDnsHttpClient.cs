using DynamicDnsClient.Configuration;
using DynamicDnsClient.Configuration.Models;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Clients;

public class DynamicDnsHttpClient : IDynamicDnsHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfigReader _configReader;
    private readonly ILogger _logger;

    public DynamicDnsHttpClient(HttpClient httpClient, IConfigReader configReader, ILogger logger)
    {
        _httpClient = httpClient;
        _configReader = configReader;
        _logger = logger;
    }
    
    public async Task<bool> UpdateIpForDnsAsync(string newIp)
    {
        var appConfig = await _configReader.ReadConfigurationAsync();
        
        foreach (var instance in appConfig!.Instances!)
        {
            foreach (var host in instance.Hosts!)
            {
                try
                {
                    var requestUri = instance.DnsApiUrlTemplate!
                        .Replace(AppConfig.HostNamePlaceholder, host)
                        .Replace(AppConfig.DomainNamePlaceholder, instance.DomainName)
                        .Replace(AppConfig.DnsApiSecretPlaceholder, instance.DnsApiSecret)
                        .Replace(AppConfig.UpdatedIpPlaceholder, newIp);

                    using var request = new HttpRequestMessage();

                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(requestUri);

                    using var response = await _httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            $"Could not update IP for '{host}.{instance.DomainName}' with IP {newIp}. " +
                            $"Response status code: {response.StatusCode}");

                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(instance.DnsApiSuccessMessage))
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        if (!responseContent.Contains(instance.DnsApiSuccessMessage))
                        {
                            _logger.LogWarning(
                                $"Could not update IP for '{host}.{instance.DomainName}' with IP {newIp}. " +
                                "Response status code was OK " +
                                $"but the response did not contain '{instance.DnsApiSuccessMessage}'");

                            return false;
                        }
                    }

                    _logger.LogInformation(
                        $"Successfully updated '{host}.{instance.DomainName}' with IP {newIp}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"Could not update IP for '{host}.{instance.DomainName}' with IP {newIp}. " +
                        $"{ex.GetType().Name}: {ex.Message}");

                    return false;
                }
            }
        }

        return true;
    }
}
