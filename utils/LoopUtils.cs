using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

public static class LoopUtils
{
    public static async Task<AgentResponse> StreamLoopAsync(AIAgent loopAgent, string input, AgentSession? session = null)
    {
        string? currentResponseId = null;
        ChatRole? currentRole = null;
        var runCount = 0;
        var updates = new List<AgentResponseUpdate>();

        Console.WriteLine("Starting the loop agent streaming run...");


        await foreach (var update in loopAgent.RunStreamingAsync(input, session))
        {
            // A new ResponseId signals the start of another inner run (loop iteration).
            if (update.ResponseId is { } responseId && responseId != currentResponseId)
            {
                currentResponseId = responseId;
                currentRole = null;
                Console.WriteLine($"\n--- run {++runCount} ---");
            }

            // Print a role-based prefix whenever the speaker changes — for example the loop's on-behalf-of
            // user feedback versus the agent's response.
            if (update.Role is { } role && role != currentRole)
            {
                currentRole = role;
                var prefix = role == ChatRole.User ? "User" : role == ChatRole.Assistant ? "Agent" : role.Value;
                Console.Write($"\n{prefix}: ");
            }

            Console.Write(update.Text);
            updates.Add(update);
        }

        Console.WriteLine();
        return updates.ToAgentResponse();
    }
}