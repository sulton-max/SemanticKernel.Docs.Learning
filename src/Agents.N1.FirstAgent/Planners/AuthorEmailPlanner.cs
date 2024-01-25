using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Agents.N1.FirstAgent.Planners;

public class AuthorEmailPlanner
{
    [KernelFunction]
    [Description("Returns back the required steps necessary to author an email.")]
    [return: Description("The list of steps needed to author an email.")]
    public async Task<string> GenerateRequiredStepsAsync(
        Kernel kernel,
        [Description("A 2-3 sentence description of what the email should be about.")] string subject,
        [Description("A description of the recipients")] string recipients
    )
    {
        // Prompt the LLM to generate a list of steps to complete the task
        var result = await kernel.InvokePromptAsync<string>(
            """
            "I'm going to write an email to {recipients} about {subject} on behalf of a user.
                    Before I do that, can you succinctly recommend the top 3 steps I should
                    take in a numbered list ? I want to make sure I don't forget anything
                    that would help my user's email sound more professional."
            """,
            new KernelArguments
            {
                { "subject", subject },
                { "recipients", recipients }
            }
        );

        return result;
    }
}