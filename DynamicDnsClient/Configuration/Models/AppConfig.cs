namespace DynamicDnsClient.Configuration.Models;

public sealed record AppConfig(string SavedStateFilePath, string[]? IpProviderUrls, InstanceConfig[]? Instances)
{
    public const string DomainNamePlaceholder = "{Domain}";
    public const string HostNamePlaceholder = "{Host}";
    public const string DnsApiSecretPlaceholder = "{Secret}";
    public const string UpdatedIpPlaceholder = "{NewIp}";
}
