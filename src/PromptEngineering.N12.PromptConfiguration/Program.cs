using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

// Create configurations
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddUserSecrets<Program>();
var configuration = configurationBuilder.Build();

// Create kernel builder
var kernelBuilder = Kernel.CreateBuilder();

// Add OpenAI connector
kernelBuilder.AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: configuration["OpenAiApiSettings:ApiKey"]!);
kernelBuilder.Services.AddLogging(configuration => configuration.AddDebug().SetMinimumLevel(LogLevel.Trace));

// Add plugins

#pragma warning disable SKEXP0050
kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
#pragma warning restore SKEXP0050

// Build kernel
var kernel = kernelBuilder.Build();

var templateConfig = new PromptTemplateConfig
{
    Name = "IntentPrompt",
    Description = "Prompt to get user intent",
    Template = """
                   <message role=""system"">
                       Instructions: Provide the intent of the request using the following example format. Keep in mind that chat history has higher priority than
                       examples. Bonus : You can double check reasoning before giving answer. Thanks!
                       {{intentExample}}
                   </message>
                   <message role=""system"">
                       Choices : You can choose between the following intents:
                       {{intentChoices}}
               
                       Choose following intent as fallback option:
                       {{fallbackIntent}}
                   </message>
               
                   <message role=""system"">
                       Examples - chat history has higher priority than examples
               
                       {{#each examples}}
                            {{#each this}}
                                <message role=""{{role}}"">{{content}}</message>
                            {{/each}}
                       {{/each}}
                   </message>
                   <message role=""system"">
                       Chat history : {{ConversationSummaryPlugin-SummarizeConversation history}}
                   </message>
               
                   <message role=""user"">{{request}}</message>
                   <message role=""system"">Intent : </message>
               """,
    TemplateFormat = "handlebars",
    InputVariables =
    [
        new InputVariable
        {
            Name = "history",
            Description = "The history of the conversation",
            IsRequired = false
        },
        new InputVariable
        {
            Name = "request",
            Description = "The user request",
            IsRequired = true
        },
    ],
    ExecutionSettings =
    {
        {"default", new OpenAIPromptExecutionSettings
        {
            MaxTokens = 1000,
            Temperature = 0
        }}
    },
    DefaultExecutionSettings = new OpenAIPromptExecutionSettings()
    {
        
    }
};

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
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize(new UserIntent(IntentType.CheckWeather)))
        }
    ),
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Should I wear a coat ?"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize(new UserIntent(IntentType.CheckWeather)))
        }
    ),
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Should I buy a coat ?"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize(new UserIntent(IntentType.Unknown)))
        }
    ),
    new(
        new[]
        {
            new ChatMessageContent(AuthorRole.User, "Did anyone replied to the email ?"),
            new ChatMessageContent(AuthorRole.System, "Intent"),
            new ChatMessageContent(AuthorRole.Assistant, JsonSerializer.Serialize(new UserIntent(IntentType.Unknown)))
        }
    )
};

// Create chat history
var history = new ChatHistory(
    new[]
    {
        new ChatMessageContent(AuthorRole.User, "I hate sending emails, no one ever reads them"),
        new ChatMessageContent(AuthorRole.Assistant, "I'm sorry to hear that. Messages may be a better way to communicate."),
        new ChatMessageContent(AuthorRole.User, "Ok, send message to notify the team about meeting."),
        new ChatMessageContent(AuthorRole.Assistant, "Message service is currently unavailable. The only option is to send an email."),
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