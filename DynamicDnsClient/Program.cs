using DynamicDnsClient.Clients;
using DynamicDnsClient.Configuration;
using DynamicDnsClient.Logging;
using DynamicDnsClient.Saving;

if (args.Contains("--silent") || args.Contains("-s"))
{
    ConsoleLogger.TraceEnabled = false;
}

ConsoleLogger.LogTrace("Dynamic DNS client started.");

var lastUpdatedPublicIp = FileOperations.GetLastUpdatedPublicIp();

var config = ConfigReader.ReadConfiguration();
if (config is null)
{
    ConsoleLogger.LogWarning($"Dynamic DNS client is exiting due to invalid configuration.{Environment.NewLine}");
    return;
}

using var httpClient = new HttpClient();

var newPublicIp = await httpClient.GetPublicIp(config);
if (newPublicIp is null)
{
    ConsoleLogger.LogTrace(
        $"Dynamic DNS client is exiting due to being unable to obtain public IP.{Environment.NewLine}");
    
    return;
}

if (string.Equals(newPublicIp, lastUpdatedPublicIp))
{
    ConsoleLogger.LogTrace($"Dynamic DNS client is exiting: IP update is not needed.{Environment.NewLine}");
    return;
}

if (await httpClient.UpdateIpForDns(config, newPublicIp))
{
    FileOperations.UpdateLastUpdatedPublicIp(newPublicIp);
    ConsoleLogger.LogTrace($"Dynamic DNS client is exiting: run completed.{Environment.NewLine}");
}
else
{
    ConsoleLogger.LogTrace(
        $"Dynamic DNS client is exiting due to being unable to update public IP.{Environment.NewLine}");
}
