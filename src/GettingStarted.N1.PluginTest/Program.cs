using GettingStarted.N1.PluginTest.Plugins;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Create configuration builder
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddUserSecrets<Program>();
var configurations = configurationBuilder.Build();

// Create builder
var kernelBuilder = Kernel.CreateBuilder();

// Configure Open AI
kernelBuilder.AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: configurations["OpenAiApiSettings:ApiKey"]!);

// Add light plugin
kernelBuilder.Plugins.AddFromType<LightPlugin>();

// Build the kernel
var kernel = kernelBuilder.Build();

// Create chat history
var chatHistory = new ChatHistory();

// Get chat completion service
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Start the conversation
while (true)
{
    // Get user input
    Console.Write("User > ");
    chatHistory.AddUserMessage(Console.ReadLine() ?? string.Empty);

    // Enable auto function calling
    var promptExecutionSettings = new OpenAIPromptExecutionSettings
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    };

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings: promptExecutionSettings, kernel: kernel);
    
    // Print the results
    Console.WriteLine("Assistant > " + result);
    
    // Add the message from the agent to the chat history
    chatHistory.AddMessage(result.Role, result.Content!);
}