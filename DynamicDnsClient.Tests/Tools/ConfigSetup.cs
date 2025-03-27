using System.Text.Encodings.Web;
using System.Text.Json;
using DynamicDnsClient.Configuration.Models;

namespace DynamicDnsClient.Tests.Tools;

public static class ConfigSetup
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
    
    public static AppConfig GenerateConfig(string id, string mockServerUrl)
    {
        var config = new AppConfig(
            $"lastUpdatedPublicIp.{id}.txt",
            [
                $"{mockServerUrl}/{id}/ip-api-1",
                $"{mockServerUrl}/{id}/ip-api-2",
                $"{mockServerUrl}/{id}/ip-api-3",
            ],
            [
                new InstanceConfig(
                    $"{id}.eu",
                    ["@", "*"],
                    $"{id}-eu-secret",
                    $"{mockServerUrl}/{id}/ddns-1/update?host={{Host}}&domain={{Domain}}&password={{Secret}}&ip={{NewIp}}",
                    $"{id}-eu-success-message"),
                new InstanceConfig(
                    $"{id}.com",
                    ["@"],
                    $"{id}-com-secret",
                    $"{mockServerUrl}/{id}/ddns-2/update?host={{Host}}&domain={{Domain}}&password={{Secret}}&ip={{NewIp}}",
                    $"{id}-com-success-message")
            ]);
        
        File.WriteAllText($"appsettings.{id}.json", JsonSerializer.Serialize(config, SerializerOptions));

        return config;
    }
}