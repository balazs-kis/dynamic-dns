using DynamicDnsClient.Logging;

namespace DynamicDnsClient.Configuration
{
    public static class ConfigReader
    {
        private const string ConfigPath = ".config";
        private const char ConfigValueSeparator = '=';
        private const char ConfigListSeparator = ',';

        public static AppConfig? ReadConfiguration()
        {
            var items = File.ReadAllLines(ConfigPath);
            AppConfig configuration;

            try
            {
                var configDictionary = items
                    .Select(i => i.Split(ConfigValueSeparator))
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
                    DnsApiSecret = configDictionary.GetValueOrDefault(nameof(AppConfig.DnsApiSecret))
                };
            }
            catch (Exception ex)
            {
                ConsoleLogger.LogError($"The configuration was not readable. {ex.GetType().Name}: {ex.Message}");
                return null;
            }

            var (isValid, problems) = ValidateConfiguration(configuration);
            if (isValid)
            {
                return configuration;
            }

            ConsoleLogger.LogError($"Required item(s) are missing from the configuration: {problems}");
            return null;
        }

        private static (bool isValid, string? problems) ValidateConfiguration(AppConfig appConfig)
        {
            var problemsBuilder = new List<string>();

            if (appConfig.IpProviderUrls is null || appConfig.IpProviderUrls.Length == 0)
            {
                problemsBuilder.Add("IP provider URL(s) not specified");
            }
        
            if (string.IsNullOrWhiteSpace(appConfig.DomainName))
            {
                problemsBuilder.Add("Domain name not specified");
            }

            if (appConfig.Hosts is null || appConfig.Hosts.Length == 0)
            {
                problemsBuilder.Add("Host(s) are not specified");
            }

            if (string.IsNullOrWhiteSpace(appConfig.DnsApiUrlTemplate))
            {
                problemsBuilder.Add("DNS API URL template not specified");
            }

            if (appConfig.DnsApiUrlTemplate?.Contains(AppConfig.DnsApiSecretPlaceholder) == true &&
                string.IsNullOrWhiteSpace(appConfig.DnsApiSecret))
            {
                problemsBuilder.Add("DNS API secret is needed but not specified");
            }

            var problems = problemsBuilder.Count > 0 ? string.Join(", ", problemsBuilder) : null;

            return (problems is null, problems);
        }
    }
}
