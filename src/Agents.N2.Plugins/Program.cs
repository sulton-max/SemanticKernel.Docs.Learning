using System.Text;
using Agents.N2.Plugins.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Create kernel builder
var kernelBuilder = Kernel.CreateBuilder();

// Create configuration
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddUserSecrets<Program>();
var configuration = configurationBuilder.Build();

// Register Open AI connector
kernelBuilder.AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: configuration["OpenAiApiSettings:ApiKey"]!);

// Register plugins
kernelBuilder.Plugins.AddFromType<MathPlugin>();

// Build kernel
var kernel = kernelBuilder.Build();

// Create chat history
var chatHistory = new ChatHistory(
    """
        You're helpful math assistant. You need to ask questions until you get exact math problem. Describe how you solved the problem too.
        Don't use complex words. Be polite.
    """
    );

// Start chat
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

while (true)
{
    Console.Write("User > ");
    chatHistory.AddUserMessage(Console.ReadLine()!);

    var executionSettings = new OpenAIPromptExecutionSettings
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Stream response
    var response = chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings: executionSettings, kernel: kernel);
    var message = new StringBuilder();    
    await foreach(var content in response)
    {
        if (content.Role.HasValue)
            Console.Write($"{content.Role.ToString()} > ");
        
        Console.Write(content);
    }

    Console.WriteLine();

    chatHistory.AddAssistantMessage(message.ToString());
}