namespace DynamicDnsClient.Configuration.Models;

public sealed record InstanceConfig(
    string? DomainName,
    string[]? Hosts,
    string? DnsApiSecret,
    string? DnsApiUrlTemplate,
    string? DnsApiSuccessMessage);
