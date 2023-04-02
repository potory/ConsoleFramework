namespace ConsoleFramework.Abstract;

/// <summary>
/// Interface for defining commands to be executed by a command-line interface.
/// </summary>
public interface ICommand : IBaseCommand
{
    /// <summary>
    /// Method for executing the command.
    /// </summary>
    void Evaluate();
}