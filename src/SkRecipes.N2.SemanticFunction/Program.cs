using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;

// Create configurations
var configurations = new ConfigurationBuilder();
configurations.AddUserSecrets<Program>();
var configuration = configurations.Build();

// Create kernel builder
var kernelBuilder = Kernel.CreateBuilder();

// Add OpenAI connector
kernelBuilder.AddOpenAIChatCompletion(modelId: "gpt-4", configuration["OpenAiApiSettings:ApiKey"]!);

// Create builder
var kernel = kernelBuilder.Build();

// Import plugins
var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Plugins");
var funPlugin = kernel.ImportPluginFromPromptDirectory(pluginsDirectory, "FunPlugin");

if (funPlugin.TryGetFunction("Joke", out var function))
{
    // Invoke function
    var result = await kernel.InvokeAsync<string>(
        function,
        new KernelArguments
        {
            ["input"] = "Travelling with your gaming laptop",
            ["$audience_type"] = "Developers"
        }
    );

    Console.WriteLine(result);
}