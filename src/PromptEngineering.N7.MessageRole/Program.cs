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

// Create chat history
var history = @"
    <message role=""""user""""> I hate sending emails, no one ever reads them</message>
    <message role=""""assistant"""">I'm sorry to hear that. Messages may be a better way to communicate.</message>
";

// Create more specific prompt with structured output
Console.Write("Your request : ");
var request = Console.ReadLine();
var prompt = @"
    <message role=""""system"""">
        Instructions: Provide the intent of the request using the following example format and also consider chat history with user. 
        {{intentExample}}
    </message>
    <message role=""""system"""">
        Choices : You can choose between the following intents:
        {{intentChoices}}

        Choose following intent as fallback option:
        {{fallbackIntent}}
    </message>

    <message role=""""system"""">
        Examples - chat history has higher priority than examples

        <message role=""""user"""">Let's remind others about today's gathering</message>
        <message role=""""system"""">Intent : </message>
        <message role=""""assistant"""">{{sendEmailExample}}</message>
    
        <message role=""""user"""">Send summary of last month's meeting</message>
        <message role=""""system"""">Intent : </message>
        <message role=""""assistant"""">{{sendMessageExample}}</message>
    
        <message role=""""user""""> Why it suddenly feels so cold?</message>
        <message role=""""system"""">Intent : </message>
        <message role=""""assistant"""">{{checkWeatherExample}}</message>
    
        <message role=""""user"""">Should I wear a coat ?</message>
        <message role=""""system"""">Intent : </message>
        <message role=""""assistant"""">{{checkWeatherExample}}</message>
    
        <message role=""""user"""">Should I buy a coat ?</message>
        <message role=""""system"""">Intent : </message>
        <message role=""""assistant"""">{{fallbackIntent}}</message>
    
        <message role=""""user"""">Did anyone replied to the email ?</message>
        <message role=""""system"""">Intent : </message>
        <message role=""""assistant"""">{{fallbackIntent}}</message>

    </message>
    <message role=""""system"""">
        Chat history : 
        {{history}}
    </message>

    <message role=""""user"""">{{request}}</message>
    <message role=""""system"""">Intent : </message>
";

// Format prompt
var formattedPrompt = prompt.Replace("{{intentExample}}", JsonSerializer.Serialize(new UserIntent()))
    .Replace("{{intentChoices}}", EnumExtensions.GetValuesAsString<IntentType>())
    .Replace(
        "{{fallbackIntent}}",
        JsonSerializer.Serialize(
            new UserIntent
            {
                Intent = IntentType.Unknown
            }
        )
    )
    .Replace(
        "{{sendEmailExample}}",
        JsonSerializer.Serialize(
            new UserIntent
            {
                Intent = IntentType.SendEmail
            }
        )
    )
    .Replace(
        "{{sendMessageExample}}",
        JsonSerializer.Serialize(
            new UserIntent
            {
                Intent = IntentType.SendMessage
            }
        )
    )
    .Replace(
        "{{checkWeatherExample}}",
        JsonSerializer.Serialize(
            new UserIntent
            {
                Intent = IntentType.CheckWeather
            }
        )
    )
    .Replace("{{history}}", history)
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
    SendMessage,
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