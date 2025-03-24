using System.Text.Json;
using DynamicDnsClient.Configuration.Models;
using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Configuration;

public class ConfigReader : IConfigReader
{
    private const string AppSettingsName = "appsettings";
    private const string AppSettingsExtension = ".json";
    
    private readonly string _appConfigPath;

    private AppConfig? _appConfig;
    
    public ConfigReader(string? environment = null)
    {
        _appConfigPath = string.IsNullOrWhiteSpace(environment)
            ? $"{AppSettingsName}{AppSettingsExtension}"
            : $"{AppSettingsName}.{environment}{AppSettingsExtension}";
    }
    
    public async Task<AppConfig?> ReadConfigurationAsync()
    {
        if (_appConfig is not null)
        {
            return _appConfig;
        }
        
        AppConfig? configuration;
        try
        {
            var configJson = await File.ReadAllTextAsync(_appConfigPath);
            configuration = JsonSerializer.Deserialize(configJson, AppConfigContext.Default.AppConfig);
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError($"The configuration was not readable. {ex.GetType().Name}: {ex.Message}");
            return null;
        }

        if (configuration is null)
        {
            ConsoleLogger.LogError("The configuration was not readable.");
            return null;
        }

        var (isValid, problem) = ValidateConfiguration(configuration);
        if (isValid)
        {
            _appConfig = configuration;
            return configuration;
        }

        ConsoleLogger.LogError($"Required item(s) are missing from the configuration: {problem}");
        return null;
    }

    private static (bool isValid, string? problem) ValidateConfiguration(AppConfig appConfig)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(appConfig.SavedStateFilePath))
        {
            problems.Add("Saved state file path not specified");
        }
        
        if (appConfig.IpProviderUrls is null || appConfig.IpProviderUrls.Length == 0)
        {
            problems.Add("IP provider URL(s) not specified");
        }
        
        if (appConfig.Instances is null || appConfig.Instances.Length == 0)
        {
            problems.Add("Dynamic DNS instance(s) not specified");
        }
        else
        {
            foreach (var instance in appConfig.Instances)
            {
                if (string.IsNullOrWhiteSpace(instance.DomainName))
                {
                    problems.Add("Domain name not specified");
                }

                if (instance.Hosts is null || instance.Hosts.Length == 0)
                {
                    problems.Add("Host(s) are not specified");
                }

                if (string.IsNullOrWhiteSpace(instance.DnsApiUrlTemplate))
                {
                    problems.Add("DNS API URL template not specified");
                }

                if (instance.DnsApiUrlTemplate?.Contains(AppConfig.DnsApiSecretPlaceholder) == true &&
                    string.IsNullOrWhiteSpace(instance.DnsApiSecret))
                {
                    problems.Add("DNS API secret is needed but not specified");
                }
            }
        }

        var problem = problems.Count > 0 ? string.Join(", ", problems) : null;

        return (problem is null, problem);
    }
}
