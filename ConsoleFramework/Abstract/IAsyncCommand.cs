namespace ConsoleFramework.Abstract;

/// <summary>
/// Interface for defining asynchronous commands to be executed by a command-line interface.
/// </summary>
public interface IAsyncCommand : IBaseCommand
{
    /// <summary>
    /// Method for executing the command asynchronously.
    /// </summary>
    Task EvaluateAsync();
}