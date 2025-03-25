using DynamicDnsClient;
using DynamicDnsClient.Clients;
using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;
using DynamicDnsClient.Saving;

if (args.Contains("--silent") || args.Contains("-s"))
{
    ConsoleLogger.TraceEnabled = false;
}

try
{
    var configReader = new ConfigReader();
    var httpClient = new HttpClient();
    var publicIpClient = new PublicIpHttpClient(httpClient, configReader);
    var dynamicDnsClient = new DynamicDnsHttpClient(httpClient, configReader);
    var persistentStateHandler = new PersistentSateHandler(configReader);

    var dynamicDns = new DynamicDns(configReader, publicIpClient, dynamicDnsClient, persistentStateHandler);
    await dynamicDns.UpdateIpAddressesAsync();
}
catch (Exception ex)
{
    ConsoleLogger.LogError($"Uncaught error during execution. {ex.GetType().Name}: {ex.Message}");
}
