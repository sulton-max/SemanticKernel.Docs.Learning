using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

// Create configurations
var configurations = new ConfigurationBuilder();
configurations.AddUserSecrets<Program>();
var configuration = configurations.Build();

// Create kernel builder
var kernelBuilder = Kernel.CreateBuilder();

// Add OpenAI connector
kernelBuilder.AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: configuration["OpenAiApiSettings:ApiKey"]!);

// Build kernel
var kernel = kernelBuilder.Build();

// Create template
var chat = kernel.CreateFunctionFromPrompt(
    """
            {{$history}}
    		User : {{$request}}
    		Assistant:
    """
);

// Create chat history
var chatHistory = new ChatHistory();

// Start chat loop
while (true)
{
    // Get user input
    Console.Write("User > ");
    var request = Console.ReadLine();

    // Get chat response
    var chatResult = kernel.InvokeStreamingAsync<StreamingChatMessageContent>(
        chat,
        new KernelArguments
        {
            { "request", request },
            { "history", string.Join("\n", chatHistory.Select(message => message.Role + ": " + message.Content)) }
        }
    );

    // Stream the response
    var message = "";
    await foreach (var chunk in chatResult)
    {
        if (chunk.Role.HasValue) Console.Write(chunk.Role + " > ");
        message += chunk;
        Console.Write(chunk);
    }

    Console.WriteLine();

    // Append to history
    chatHistory.AddUserMessage(request!);
    chatHistory.AddAssistantMessage(message);
}