using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Clients;

public static partial class HttpClientExtensions
{
    public static async Task<bool> UpdateIpForDns(this HttpClient httpClient, AppConfig config, string newIp)
    {
        foreach (var host in config.Hosts!)
        {
            try
            {
                var requestUri = config.DnsApiUrlTemplate!
                    .Replace(AppConfig.HostNamePlaceholder, host)
                    .Replace(AppConfig.DomainNamePlaceholder, config.DomainName)
                    .Replace(AppConfig.DnsApiSecretPlaceholder, config.DnsApiSecret)
                    .Replace(AppConfig.UpdatedIpPlaceholder, newIp);

                using var request = new HttpRequestMessage();

                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(requestUri);

                using var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    ConsoleLogger.LogWarning(
                        $"Could not update IP for '{host}.{config.DomainName}'. " +
                        $"Response status code: {response.StatusCode}");

                    return false;
                }

                if (!string.IsNullOrWhiteSpace(config.DnsApiSuccessMessage))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (!responseContent.Contains(config.DnsApiSuccessMessage))
                    {
                        ConsoleLogger.LogWarning(
                            $"Could not update IP for '{host}.{config.DomainName}'. " +
                            "Response status code was OK " +
                            $"but the response did not contain '{config.DnsApiSuccessMessage}'");

                        return false;
                    }
                }

                ConsoleLogger.LogInformation($"Successfully updated '{host}.{config.DomainName}' with IP {newIp}");
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogWarning(
                    $"Could not update IP for '{host}.{config.DomainName}'. {ex.GetType().Name}: {ex.Message}");
                    
                return false;
            }
        }

        return true;
    }
}
