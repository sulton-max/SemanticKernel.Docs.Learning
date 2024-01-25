using System.Text;
using Agents.N1.FirstAgent.Planners;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Create kernel builder
var kernelBuilder = Kernel.CreateBuilder();

// Create configurations
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddUserSecrets<Program>();
var configuration = configurationBuilder.Build();

// Add open AI connector
kernelBuilder.AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: configuration["OpenAiApiSettings:ApiKey"]!);
kernelBuilder.AddOpenAITextGeneration(modelId: "gpt-4", apiKey: "");

// Add notifications infrastructure
kernelBuilder.Services.Configure<SmtpEmailSenderSettings>(configuration.GetSection(nameof(SmtpEmailSenderSettings)));
kernelBuilder.Plugins.AddFromType<AuthorEmailPlanner>();
kernelBuilder.Plugins.AddFromType<EmailPlugin>();

// Add logging
kernelBuilder.Services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Trace));

// Builder kernel
var kernel = kernelBuilder.Build();

// Add persona
var chatHistory = new ChatHistory(
    """
    You are a friendly assistant who likes to follow the rules. You will complete required steps and request approval before taking any consequential
    actions. If the user doesn't provide enough information for you to complete a task, you will keep asking questions until you have enough information to complete the task.
    """
);

// Get chat completion service
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

while (true)
{
    // Get user input
    Console.Write("User > ");
    chatHistory.AddUserMessage(Console.ReadLine()!);

    // Get chat completions
    var executionSettings = new OpenAIPromptExecutionSettings
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings: executionSettings, kernel: kernel);

    var message = new StringBuilder();
    
    // Stream message content
    await foreach (var content in result)
    {
        if (content.Role.HasValue)
        {
            Console.WriteLine("Assistant > ");
            Console.WriteLine(content.Content);
        }
        
        Console.Write(content);
        message.Append(content);
    }

    Console.WriteLine();
    
    // Add message to chat history
    chatHistory.AddAssistantMessage(message.ToString());
}