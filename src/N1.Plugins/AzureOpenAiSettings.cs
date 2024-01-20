namespace N1.Plugins;

public class AzureOpenAiSettings
{
    public string DeploymentName { get; set; } = default!;
    
    public string Endpoint { get; set; } = default!;
    
    public string ApiKey { get; set; } = default!;
}