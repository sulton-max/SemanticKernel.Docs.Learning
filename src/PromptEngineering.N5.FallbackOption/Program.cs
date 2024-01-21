using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

// Create configurations
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddUserSecrets<Program>();
var configuration = configurationBuilder.Build();

// Create kernel builder
var kernelBuilder = Kernel.CreateBuilder();

// Add OpenAI connector
kernelBuilder.AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: configuration["OpenAiApiSettings:ApiKey"]!);

// Build kernel
var kernel = kernelBuilder.Build();

// Create more specific prompt with structured output
Console.Write("Your request : ");
var request = Console.ReadLine();
var prompt = @"
    ## Instructions

    Provide the intent of the request using the following example format:

    {{intentExample}}

    ## Choices

    You can choose between the following intents:

    {{intentChoices}}

    Choose following intent as fallback option:

    {{fallbackIntent}}

    # Examples

    User : Let's remind others about today's gathering
    Intent : {{sendEmailExample}}

    User : Why it suddenly feels so cold?
    Intent : {{checkWeatherExample}}

    User : Should I wear a coat ?
    Intent : {{checkWeatherExample}}

    User : Should I buy a coat ?
    Intent : {{checkWeatherExample}}

    ## User input

    The user input is

    {{request}}

    ## Intent
";

// Format prompt
var formattedPrompt = prompt
    .Replace("{{intentExample}}", JsonSerializer.Serialize(new UserIntent()))
    .Replace("{{intentChoices}}", EnumExtensions.GetValuesAsString<IntentType>())
    .Replace("{{fallbackIntent}}", JsonSerializer.Serialize(new UserIntent { Intent = IntentType.Unknown }))
    .Replace("{{sendEmailExample}}", JsonSerializer.Serialize(new UserIntent { Intent = IntentType.SendEmail }))
    .Replace("{{checkWeatherExample}}", JsonSerializer.Serialize(new UserIntent { Intent = IntentType.CheckWeather }))
    .Replace("{{request}}", request);

// Display request intent
var result = await kernel.InvokePromptAsync<string>(formattedPrompt);
var userIntent = JsonSerializer.Deserialize<UserIntent>(result!);
Console.WriteLine(userIntent!.Intent);

public class UserIntent
{
    public IntentType Intent { get; set; }
}

public enum IntentType
{
    CheckWeather,
    SendEmail,
    Unknown
}

public static class EnumExtensions
{
    public static Dictionary<string, int> GetValuesAndNames<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues<TEnum>().ToDictionary(enumValue => enumValue.ToString(), enumValue => (int)(object)enumValue);
    }

    public static string GetValuesAsString<TEnum>() where TEnum : struct, Enum
    {
        return string.Join(", ", GetValuesAndNames<TEnum>().Select(value => $"{value.Key} = {value.Value}"));
    }
}