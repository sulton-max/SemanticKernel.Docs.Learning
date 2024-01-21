using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Create configurations
var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddUserSecrets<Program>();
var configuration = configurationBuilder.Build();

// Create kernel builder
var kernelBuilder = Kernel.CreateBuilder();

// Add OpenAI connector
kernelBuilder.AddOpenAIChatCompletion(modelId: "gpt-4", apiKey: configuration["OpenAiApiSettings:ApiKey"]!);

var kernel = kernelBuilder.Build();

// Create inline function
var summarizePrompt = """
                            {{$input}}
                            
                            Summarize the content above in less than 150 characters.
                            
                            Add some memes to make it more interesting.
                        """;

// Create execution settings
var executionSettings = new OpenAIPromptExecutionSettings
{
    MaxTokens = 1000,
    Temperature = 0.2,
    TopP = 0.5
};

// Create prompt config
var promptConfig = new PromptTemplateConfig(summarizePrompt);

// Create prompt template factory and template
var promptTemplateFactory = new KernelPromptTemplateFactory();
var promptTemplate = promptTemplateFactory.Create(promptConfig);

// Render prompt
var renderedPrompt = await promptTemplate.RenderAsync(kernel);
Console.WriteLine(renderedPrompt);

// Create function
var summaryFunction = kernel.CreateFunctionFromPrompt(summarizePrompt, executionSettings);

// Create input
var input =
    "Tesla CEO and 'X' owner, Elon Musk said on Wednesday that Artificial Intelligence (AI) could endanger the existence of human civilisation.\n\"There is some chance, above zero, that AI will kill us all. I think it's slow but there is some chance. I think this also concerns the fragility of human civilization. If you study history, you will realise that every civilisation has a sort of lifespan,\" he said.\n\nHis remarks came during a media interaction as he arrived to attend the United Kingdom hosted world's first global Artificial Intelligence (AI) Safety Summit.";


var summaryResult = await kernel.InvokeAsync(
    summaryFunction,
    new KernelArguments
    {
        ["input"] = input
    }
);

Console.WriteLine(summaryResult);