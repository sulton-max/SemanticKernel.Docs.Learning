using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace GettingStarted.N1.PluginTest.Plugins;

public class LightPlugin
{
    public bool IsOn { get; set; } = false;
    
    [KernelFunction]
    [Description("Gets the state of the light.")]
    public string GetState() => this.IsOn ? "on" : "off";

    [KernelFunction]
    [Description("Changes the state of the light.")]
    public string ChangeState(bool newState)
    {
        IsOn = newState;
        var state = GetState();

        // Print the message
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.Write($"[Light is now {state}\n]");
        Console.ResetColor();

        return state;
    }
}