﻿using System.Text;
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
    
    public static (string Id, AppConfig Config) GenerateConfig(string mockServerUrl)
    {
        var id = GenerateWord();

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

        return (id, config);
    }

    public static void CleanupConfig(params string[] ids)
    {
        foreach (var id in ids)
        {
            File.Delete($"appsettings.{id}.json");
        }
    }

    private static string GenerateWord()
    {
        const string vowels = "aeiou";
        const string consonants = "bcdfghjklmnpqrstvwxz";
        
        var nextCharacterIsVowel = Random.Shared.Next(2) == 1;
        var numberOfVowels = Random.Shared.Next(3, 5);
        var numberOfConsonants = numberOfVowels + (nextCharacterIsVowel ? Random.Shared.Next(2) : 0);

        var wordLength = numberOfVowels + numberOfConsonants;
        var builder = new StringBuilder(wordLength);

        for (var i = 0; i < wordLength; i++)
        {
            builder.Append(nextCharacterIsVowel
                ? vowels[Random.Shared.Next(vowels.Length)]
                : consonants[Random.Shared.Next(consonants.Length)]);
            
            nextCharacterIsVowel = !nextCharacterIsVowel;
        }
        
        return builder.ToString();
    }
}