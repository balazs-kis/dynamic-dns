using System.Text.Json.Serialization;
using DynamicDnsClient.Configuration.Models;

namespace DynamicDnsClient.Configuration;

[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(InstanceConfig))]
public partial class AppConfigContext: JsonSerializerContext
{
}
