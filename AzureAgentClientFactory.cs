using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.AI;

#pragma warning disable OPENAI001 // Responses API is still experimental.

/// <summary>
/// Connects to the Azure AI Foundry project and hands back a chat client
/// for whichever model deployment the loop agents should use.
/// </summary>
public sealed class AzureAgentClientFactory
{
    private readonly AIProjectClient _projectClient;
    private readonly string _deploymentName;

    public AzureAgentClientFactory(string endpoint, string deploymentName)
    {
        _deploymentName = deploymentName;
        _projectClient = new AIProjectClient(new Uri(endpoint), CreateCredential());
    }

    public IChatClient CreateChatClient() =>
        _projectClient.GetProjectOpenAIClient().GetResponsesClient().AsIChatClient(_deploymentName);


    private static DefaultAzureCredential CreateCredential() =>
        new(new DefaultAzureCredentialOptions
        {
            ExcludeManagedIdentityCredential = true,
            ExcludeEnvironmentCredential = true,
            ExcludeWorkloadIdentityCredential = true,
            ExcludeVisualStudioCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeAzureDeveloperCliCredential = true,
            ExcludeAzureCliCredential = false,
            ExcludeInteractiveBrowserCredential = false,
        });
}
