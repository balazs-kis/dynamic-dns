using DynamicDnsClient.Configuration.Models;

namespace DynamicDnsClient.Configuration;

public interface IConfigReader
{
    Task<AppConfig?> ReadConfigurationAsync();
}
