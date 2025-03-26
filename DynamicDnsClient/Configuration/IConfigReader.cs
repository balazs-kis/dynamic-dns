using DynamicDnsClient.Configuration.Models;

namespace DynamicDnsClient.Configuration;

public interface IConfigReader
{
    string AppConfigPath { get; }

    Task<AppConfig?> ReadConfigurationAsync();
}
