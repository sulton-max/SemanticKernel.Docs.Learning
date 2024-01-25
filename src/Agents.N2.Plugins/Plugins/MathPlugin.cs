using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Agents.N2.Plugins.Plugins;

public class MathPlugin
{
    [KernelFunction, Description("Given two numbers, returns the sum of them.")]
    public double Add([Description("First number")] double a, [Description("Second number")] double b) => a + b;
    
    [KernelFunction, Description("Given two numbers, returns the difference of them.")]
    public double Subtract([Description("First number")] double a, [Description("Second number")] double b) => a - b;
    
    [KernelFunction, Description("Given two numbers, returns the product of them.")]
    public double Multiply([Description("First number")] double a, [Description("Second number")] double b) => a * b;
    
    [KernelFunction, Description("Given two numbers, returns the quotient of them.")]
    public double Divide([Description("First number")] double a, [Description("Second number")] double b) => a / b;
    
    [KernelFunction, Description("Given two numbers, returns the remainder of them.")]
    public double Modulo([Description("First number")] double a, [Description("Second number")] double b) => a % b;
    
    [KernelFunction, Description("Given two numbers, returns the power of them.")]
    public double Power([Description("First number")] double a, [Description("Second number")] double b) => Math.Pow(a, b);
    
    [KernelFunction, Description("Given a number, returns the square root of it.")]
    public double Sqrt([Description("Number")] double a) => Math.Sqrt(a);
}