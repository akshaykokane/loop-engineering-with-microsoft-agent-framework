using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

#pragma warning disable MAAI001 // Harness/agent-mode APIs are still experimental.

/// <summary>
/// Builds "harness" agents: chat agents wired up with the todo, file-access
/// and file-memory providers that the Ralph-style loop relies on.
/// </summary>
public sealed class HarnessAgentFactory
{
    private readonly AzureAgentClientFactory _clientFactory;
    private readonly int _maxContextWindowTokens;
    private readonly int _maxOutputTokens;

    public HarnessAgentFactory(AzureAgentClientFactory clientFactory, int maxContextWindowTokens, int maxOutputTokens)
    {
        _clientFactory = clientFactory;
        _maxContextWindowTokens = maxContextWindowTokens;
        _maxOutputTokens = maxOutputTokens;
    }

    public AIAgent Create(
        string name,
        string instructions,
        bool disableTodoProvider = false,
        IList<AITool>? tools = null,
        ToolApprovalAgentOptions? toolApprovalAgentOptions = null) =>
        
        _clientFactory.CreateChatClient().AsHarnessAgent(new HarnessAgentOptions
        {
            Name = name,
            MaxContextWindowTokens = _maxContextWindowTokens,
            MaxOutputTokens = _maxOutputTokens,
            DisableAgentModeProvider = true,
            DisableTodoProvider = disableTodoProvider,
            DisableFileMemory = true,
            DisableFileAccess = false,
            DisableWebSearch = true,
            FileAccessStore = new FileSystemAgentFileStore(
                Path.Combine(Directory.GetCurrentDirectory(), "refined-blogs")),
            ToolApprovalAgentOptions = toolApprovalAgentOptions ?? new ToolApprovalAgentOptions
            {
                AutoApprovalRules = [FileAccessProvider.AllToolsAutoApprovalRule],
            },
            // Configure the file memory provider to store files in a local folder called "agent-files".
            FileMemoryStore = new FileSystemAgentFileStore(
                Path.Combine(AppContext.BaseDirectory, "agent-files")),
            ChatOptions = new ChatOptions
            {
                Instructions = instructions,
                Tools = tools,
                MaxOutputTokens = _maxOutputTokens,
            },
        });
}
