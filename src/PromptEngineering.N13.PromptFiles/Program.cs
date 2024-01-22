using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

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

// Load plugins
var prompts = kernel.CreatePluginFromPromptDirectory("Prompts");

// Create few shot examples
var fewShotExamples = new List<ChatHistory>
{
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Let's remind others about today's gathering"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize(new UserIntent(IntentType.SendEmail)))
        }
    ),
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Send summary of last month's meeting"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize(new UserIntent(IntentType.SendMessage)))
        }
    ),
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Why it suddenly feels so cold?"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize( new UserIntent(IntentType.CheckWeather)))
        }
    ),
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Should I wear a coat ?"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize( new UserIntent(IntentType.CheckWeather)))
        }
    ),
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Should I buy a coat ?"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize( new UserIntent(IntentType.Unknown)))
        }
    ),
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Did anyone replied to the email ?"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize( new UserIntent(IntentType.Unknown)))
        }
    )
};

// Create chat history
var history = new ChatHistory(
    new[]
    {
        new ChatMessageContent(AuthorRole.User, "I hate sending emails, no one ever reads them"),
        new ChatMessageContent(AuthorRole.Assistant, "I'm sorry to hear that. Messages may be a better way to communicate.")
    }
);

// Create more specific prompt with structured output
Console.Write("Your request : ");
var request = Console.ReadLine();

var kernelArguments = new KernelArguments
{
    { "intentExample", new UserIntent(IntentType.Unknown) },
    { "intentChoices", EnumExtensions.GetValuesAsString<IntentType>() },
    { "fallbackIntent", new UserIntent(IntentType.Unknown) },
    { "examples", fewShotExamples },
    { "history", history },
    { "request", request }
};

// Render template
var templateFactory = new HandlebarsPromptTemplateFactory();
var template = templateFactory.Create(templateConfig);
var renderedPrompt = await template.RenderAsync(kernel, kernelArguments);

var getIntentFunction = kernel.CreateFunctionFromPrompt(templateConfig, new HandlebarsPromptTemplateFactory());

// Display request intent
var intent = await kernel.InvokeAsync<string>(getIntentFunction, kernelArguments);
var userIntent = JsonSerializer.Deserialize<UserIntent>(intent!);
Console.WriteLine(userIntent!.Intent);

public record UserIntent(IntentType Intent);

public enum IntentType
{
    Unknown,
    CheckWeather,
    SendEmail,
    SendMessage,
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