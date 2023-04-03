namespace ConsoleFramework.Environment;

/// <summary>
/// Defines the interface for a class that can run a contiguous process asynchronously.
/// </summary>
public interface IContiguousProcessRunner
{
    /// <summary>
    /// Runs the specified contiguous process asynchronously.
    /// </summary>
    /// <param name="process">The contiguous process to run.</param>
    /// <returns>A Task object representing the asynchronous operation.</returns>
    Task RunProcessAsync(IContiguousProcess process);
}