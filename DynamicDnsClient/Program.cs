using DynamicDnsClient;
using DynamicDnsClient.Clients;
using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;
using DynamicDnsClient.Saving;

var traceDisabled = args.Contains("--silent") || args.Contains("-s");
var logger = new ConsoleLogger(!traceDisabled);

try
{
    var configReader = new ConfigReader(logger);
    var httpClient = new HttpClient();
    var publicIpClient = new PublicIpHttpClient(httpClient, configReader, logger);
    var dynamicDnsClient = new DynamicDnsHttpClient(httpClient, configReader, logger);
    var persistentStateHandler = new PersistentSateHandler(configReader, logger);

    var dynamicDns = new DynamicDns(configReader, publicIpClient, dynamicDnsClient, persistentStateHandler, logger);
    await dynamicDns.UpdateIpAddressesAsync();
}
catch (Exception ex)
{
    logger.LogError($"Uncaught error during execution. {ex.GetType().Name}: {ex.Message}");
}
