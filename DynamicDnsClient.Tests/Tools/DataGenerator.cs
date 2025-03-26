using System.Text;

namespace DynamicDnsClient.Tests.Tools;

public static class DataGenerator
{
    public static string GenerateWord()
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