using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Recipes.N2;

var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var settings = new AzureOpenAiSettings();
configuration.GetSection(nameof(AzureOpenAiSettings)).Bind(settings);

var kernelBuilder = Kernel.Builder;
kernelBuilder.WithAzureChatCompletionService(settings.DeploymentName, settings.Endpoint, settings.ApiKey);
var kernel = kernelBuilder.Build();

// Simple skill execution



// get parent folder from executing folder
var projectFolder = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent;
var skillsFolder = Path.Combine(projectFolder!.FullName, "Skills");

var skills = new[]
{
    "FunSkill",
    "ChatSkill"
};

var allSkills = kernel.ImportSemanticSkillFromDirectory(skillsFolder, skills);

var myInput = "time travel to dinosaur age";

var result = await kernel.RunAsync(allSkills["Joke"]);
Console.WriteLine(result.Result);