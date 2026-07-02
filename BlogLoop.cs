using Microsoft.Agents.AI;

#pragma warning disable MAAI001 // Harness/agent-mode APIs are still experimental.

/// <summary>
/// A "Ralph-style" completion-marker loop: an agent repeatedly works through
/// its own todo list, refining a blog post, until every todo item is done
/// (or an iteration cap is hit).
/// </summary>
public sealed class BlogLoop
{
    private const int MaxIterations = 6;

    private const string HarnessInstructions =
        """
        You are a planning assistant. First break the task into todo items using your todo tools.
        Then, on each turn, make progress and mark completed items as done. When all items are
        complete, summarize the result.
        Last step should be to write refined blog to refined-blogs folder in current working directory
        """;

    private readonly HarnessAgentFactory _harnessAgentFactory;

    public BlogLoop(HarnessAgentFactory harnessAgentFactory)
    {
        _harnessAgentFactory = harnessAgentFactory;
    }

    public async Task<AgentResponse> RunAsync(string blogText)
    {
        Console.WriteLine("\nStarting the LOOP for blog refinement...");

        AIAgent harnessAgent = BuildHarnessAgent();
        AIAgent loopAgent = BuildLoopAgent(harnessAgent);

        AgentResponse response = await LoopUtils.StreamLoopAsync(loopAgent, "Refine the blog" + blogText);
        Console.WriteLine($"\nFinal response:\n{response.Text}");
        return response;
    }

    private AIAgent BuildHarnessAgent()
    {
        Console.WriteLine("Building harness");
        var harnessAgent = _harnessAgentFactory.Create(name: "ralph", instructions: HarnessInstructions);
        Console.WriteLine("Building harness completed");
        return harnessAgent;
    }

    private static AIAgent BuildLoopAgent(AIAgent harnessAgent)
    {
        // Stops the loop once the harness agent has no todo items left; otherwise
        // asks it to keep going, telling it how many items still remain.
        var todoCompletionEvaluator = new DelegateLoopEvaluator(async (context, cancellationToken) =>
        {
            var todoProvider = context.Agent.GetService<TodoProvider>()
                ?? throw new InvalidOperationException("The agent did not expose a TodoProvider.");

            var remaining = await todoProvider.GetRemainingTodosAsync(context.Session, cancellationToken)
                .ConfigureAwait(false);

            return remaining.Count > 0
                ? LoopEvaluation.Continue($"Not all todos are complete yet ({remaining.Count} remaining). Please complete the remaining todo items.")
                : LoopEvaluation.Stop();
        });

        var loopAgent = new LoopAgent(
            harnessAgent,
            todoCompletionEvaluator,
            new LoopAgentOptions { MaxIterations = MaxIterations });

        Console.WriteLine("Building loop agent completed");
        return loopAgent;
    }
}
