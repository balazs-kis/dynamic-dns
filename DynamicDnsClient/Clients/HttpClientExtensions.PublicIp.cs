﻿using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Clients;

public static partial class HttpClientExtensions
{
    public static async Task<string?> GetPublicIp(this HttpClient httpClient, AppConfig config)
    {
        try
        {
            foreach (var ipProviderUrl in config.IpProviderUrls!)
            {
                using var request = new HttpRequestMessage();

                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(ipProviderUrl);

                using var response = await httpClient.SendAsync(request);
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
