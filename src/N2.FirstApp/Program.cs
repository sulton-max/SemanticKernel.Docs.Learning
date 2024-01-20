using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TemplateEngine;
using N2.FirstApp;

// Create configuration
var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var openAiSettings = configuration.GetSection(nameof(AzureOpenAiSettings)).Get<AzureOpenAiSettings>() ??
                     throw new InvalidOperationException("Open AI settings not found.");

// Create kernel builder
var kernelBuilder = new KernelBuilder();

kernelBuilder.WithAzureChatCompletionService(openAiSettings.DeploymentName,
    openAiSettings.Endpoint,
    openAiSettings.ApiKey);

// Build the kernel
var kernel = kernelBuilder.Build();

// Prompt template string
var mySemanticFunctionInline =
    """
    {{$input}}

    Summarize the content above in less than 140 characters.
    """;
    
// Prompt template config
var promptConfig = new PromptTemplateConfig
{
};
    
// Create prompt template
var promptTemplate = new PromptTemplate(mySemanticFunctionInline, promptConfig, kernel);

// Create semantic function config
var semanticFunctionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
    