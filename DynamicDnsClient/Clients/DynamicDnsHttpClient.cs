using DynamicDnsClient.Configuration;
using DynamicDnsClient.Configuration.Models;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Clients;

public class DynamicDnsHttpClient : IDynamicDnsHttpClient
{
    private readonly IHttpClient _httpClient;
    private readonly IConfigReader _configReader;

    public DynamicDnsHttpClient(IHttpClient httpClient, IConfigReader configReader)
    {
        _httpClient = httpClient;
        _configReader = configReader;
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
                        ConsoleLogger.LogWarning(
                            $"Could not update IP for '{host}.{instance.DomainName}'. " +
                            $"Response status code: {response.StatusCode}");

                        return false;
                    }

                    if (!string.IsNullOrWhiteSpace(instance.DnsApiSuccessMessage))
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        if (!responseContent.Contains(instance.DnsApiSuccessMessage))
                        {
                            ConsoleLogger.LogWarning(
                                $"Could not update IP for '{host}.{instance.DomainName}'. " +
                                "Response status code was OK " +
                                $"but the response did not contain '{instance.DnsApiSuccessMessage}'");

                            return false;
                        }
                    }

                    ConsoleLogger.LogInformation(
                        $"Successfully updated '{host}.{instance.DomainName}' with IP {newIp}");
                }
                catch (Exception ex)
                {
                    ConsoleLogger.LogWarning(
                        $"Could not update IP for '{host}.{instance.DomainName}'. {ex.GetType().Name}: {ex.Message}");

                    return false;
                }
            }
        }

        return true;
    }
}
