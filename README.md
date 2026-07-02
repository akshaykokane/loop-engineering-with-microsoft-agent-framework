# LoopAgentWithMAF

A .NET sample demonstrating a "Ralph-style" completion-marker loop built on
[Microsoft Agent Framework](https://github.com/microsoft/agent-framework) (`Microsoft.Agents.AI`)
and its experimental `Harness` package. An agent iteratively refines a blog post — planning with
a todo list, editing, and writing output to disk — until it has no todo items left or a max
iteration count is reached.

## How it works

- **`AzureAgentClientFactory`** connects to an Azure AI Foundry project and returns an
  `IChatClient` for a given model deployment, authenticating via `DefaultAzureCredential`
  (Azure CLI login, falling back to an interactive browser prompt).
- **`HarnessAgentFactory`** wraps a chat client into a "harness" agent (`AsHarnessAgent`) with
  todo tracking, file-access tools, and file-memory tools wired up. File access is sandboxed to
  the `refined-blogs/` directory via `FileSystemAgentFileStore`, and all tool calls are
  auto-approved.
- **`BlogLoop`** builds the harness agent, wraps it in a `LoopAgent` with a
  `DelegateLoopEvaluator` that keeps looping while the agent's `TodoProvider` reports remaining
  todo items (capped at `MaxIterations`), and streams the run to the console.
- **`LoopUtils.StreamLoopAsync`** consumes the agent's streaming updates and prints each loop
  iteration and speaker turn as it arrives.
- **`Program.cs`** wires the above together, reads `blogs/blog.txt`, and kicks off the loop.

## Prerequisites

- .NET 10 SDK
- Access to an Azure AI Foundry project with a deployed chat model
- Signed in via `az login` (or another credential supported by `DefaultAzureCredential`)

## Configuration

Set these environment variables to point at your own Azure AI Foundry project and deployment
(otherwise the built-in defaults in `Program.cs` are used):

```bash
export AZURE_AI_PROJECT_ENDPOINT="https://<your-project>.services.ai.azure.com/api/projects/<project-name>"
export AZURE_AI_MODEL_DEPLOYMENT_NAME="gpt-4.1"
```

## Running

Place the blog post to refine at `blogs/blog.txt`, then run:

```bash
dotnet run
```

The refined blog is written to `refined-blogs/` by the agent as its final step.

## Project layout

```
Program.cs               Entry point — wires up the client, harness, and loop, then runs it
AzureAgentClientFactory.cs   Builds the Azure AI Foundry chat client
HarnessAgentFactory.cs       Builds harness agents (todo, file-access, file-memory providers)
BlogLoop.cs               The Ralph-style loop: plan → refine → write until todos are done
utils/LoopUtils.cs        Streams and prints a loop agent's run to the console
blogs/                    Input blog drafts (blog.txt)
refined-blogs/            Output location the agent writes refined drafts to
agent-file-memory/        Agent file-memory scratch space
```
