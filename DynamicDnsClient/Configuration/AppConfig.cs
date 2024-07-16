namespace DynamicDnsClient.Configuration
{
    public sealed record AppConfig
    {
        public const string DomainNamePlaceholder = "{Domain}";
        public const string HostNamePlaceholder = "{Host}";
        public const string DnsApiSecretPlaceholder = "{Secret}";
        public const string UpdatedIpPlaceholder = "{NewIp}";
    
        public string[]? IpProviderUrls { get; init; }
        public string? DomainName { get; init; }
        public string[]? Hosts { get; init; }
        public string? DnsApiUrlTemplate { get; init; }
        public string? DnsApiSecret { get; init; }
    }
}
