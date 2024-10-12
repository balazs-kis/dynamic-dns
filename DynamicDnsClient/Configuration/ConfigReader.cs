using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Configuration;

public static class ConfigReader
{
    private const string ConfigPath = ".config";
    private const char ConfigValueSeparator = '=';
    private const char ConfigListSeparator = ',';

    public static AppConfig? ReadConfiguration()
    {
        AppConfig configuration;

        try
        {
            var items = File.ReadAllLines(ConfigPath);

            var configDictionary = items
                .Select(i => i.Split(ConfigValueSeparator, 2))
                .ToDictionary(i => i[0], i => i[1]);

            configuration = new AppConfig
            {
                IpProviderUrls = configDictionary
                    .GetValueOrDefault(nameof(AppConfig.IpProviderUrls))
                    ?.Split(ConfigListSeparator),
                DomainName = configDictionary.GetValueOrDefault(nameof(AppConfig.DomainName)),
                Hosts = configDictionary
                    .GetValueOrDefault(nameof(AppConfig.Hosts))
                    ?.Split(ConfigListSeparator),
                DnsApiUrlTemplate = configDictionary.GetValueOrDefault(nameof(AppConfig.DnsApiUrlTemplate)),
                DnsApiSecret = configDictionary.GetValueOrDefault(nameof(AppConfig.DnsApiSecret)),
                DnsApiSuccessMessage = configDictionary.GetValueOrDefault(nameof(AppConfig.DnsApiSuccessMessage)),
            };
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogError($"The configuration was not readable. {ex.GetType().Name}: {ex.Message}");
            return null;
        }

        var (isValid, problem) = ValidateConfiguration(configuration);
        if (isValid)
        {
            return configuration;
        }

        ConsoleLogger.LogError($"Required item(s) are missing from the configuration: {problem}");
        return null;
    }

    private static (bool isValid, string? problem) ValidateConfiguration(AppConfig appConfig)
    {
        var problems = new List<string>();

        if (appConfig.IpProviderUrls is null || appConfig.IpProviderUrls.Length == 0)
        {
            problems.Add("IP provider URL(s) not specified");
        }
        
        if (string.IsNullOrWhiteSpace(appConfig.DomainName))
        {
            problems.Add("Domain name not specified");
        }

        if (appConfig.Hosts is null || appConfig.Hosts.Length == 0)
        {
            problems.Add("Host(s) are not specified");
        }

        if (string.IsNullOrWhiteSpace(appConfig.DnsApiUrlTemplate))
        {
            problems.Add("DNS API URL template not specified");
        }

        if (appConfig.DnsApiUrlTemplate?.Contains(AppConfig.DnsApiSecretPlaceholder) == true &&
            string.IsNullOrWhiteSpace(appConfig.DnsApiSecret))
        {
            problems.Add("DNS API secret is needed but not specified");
        }

        var problem = problems.Count > 0 ? string.Join(", ", problems) : null;

        return (problem is null, problem);
    }
}
