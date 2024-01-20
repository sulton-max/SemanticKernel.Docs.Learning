using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Recipes.N1;

// Create configuration
var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

// Bind open ai settings
var openAiSettings = new AzureOpenAiSettings();
configuration.GetSection(nameof(AzureOpenAiSettings)).Bind(openAiSettings);

// Create kernel builder
var kernelBuilder = Kernel.Builder;

// Create kernel
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
    Completion =
    {
        MaxTokens = 1000, Temperature = 0.2, TopP = 0.5,
    }
};

// Create prompt template
var promptTemplate = new PromptTemplate(
    mySemanticFunctionInline, promptConfig, kernel
);

// Create semantic function config
var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
var summaryFunction = kernel.RegisterSemanticFunction("MySkill", "Summary", functionConfig);

// Input for function
var input =
"""
I think with some confidence I can say that 2023 is going to be the most exciting year that
the AI community has ever had,” writes Kevin Scott, chief technology officer at Microsoft,
in a Q&A on the company’s AI blog. He acknowledges that he also thought 2022 was the most
exciting year for AI, but he believes that the pace of innovation is only increasing.
This is particularly true with generative AI, which doesn’t simply analyze large data sets
but is a tool people can use to create entirely new works. We can already see its promise
in systems like GPT-3, which can do anything from helping copyedit and summarize text to
providing inspiration, and DALL-E 2, which can create useful and arresting works of art
based on text inputs. Here are some of Scott’s predictions about how AI will change the
way we work and play.
""";

// Result of function, yay that's it :)
var summary = await kernel.RunAsync(input, summaryFunction);
Console.WriteLine(summary);