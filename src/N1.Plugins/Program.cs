using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using N1.Plugins;

// Create configuration
var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var openAiSettings = configuration.GetSection(nameof(AzureOpenAiSettings)).Get<AzureOpenAiSettings>() ??
                     throw new InvalidOperationException("Open AI settings not found.");

// Create the kernel builder
var kernelBuilder = new KernelBuilder();

// Configure Open API
kernelBuilder.WithAzureChatCompletionService(openAiSettings.DeploymentName,
    openAiSettings.Endpoint,
    openAiSettings.ApiKey);

// Build the kernel
var kernel = kernelBuilder.Build();

// Use fun plugin
var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Plugins");

var funPluginFunctions = kernel.ImportSemanticFunctionsFromDirectory(pluginsDirectory, "FunPlugin");

var result = await kernel.RunAsync("deploying on friday", funPluginFunctions["Joke"]);
var resultString = result.GetValue<string>();

Console.WriteLine(resultString);