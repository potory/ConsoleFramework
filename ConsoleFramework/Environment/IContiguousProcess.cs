namespace ConsoleFramework.Environment;

/// <summary>
/// Represents an interface for a contiguous process.
/// </summary>
public interface IContiguousProcess
{
    /// <summary>
    /// Gets the name of the process.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the progress of the process as a percentage.
    /// </summary>
    double Progress { get; }

    /// <summary>
    /// Gets the message associated with the process.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the status of the process.
    /// </summary>
    ProcessStatus Status { get; }

    /// <summary>
    /// Runs the process asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RunAsync();
}