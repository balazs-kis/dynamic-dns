using System.Text.Json.Serialization;
using DynamicDnsClient.Configuration.Models;

namespace DynamicDnsClient.Configuration;

[JsonSourceGenerationOptions(WriteIndented = false, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(InstanceConfig))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
public partial class AppConfigContext : JsonSerializerContext
{
}
