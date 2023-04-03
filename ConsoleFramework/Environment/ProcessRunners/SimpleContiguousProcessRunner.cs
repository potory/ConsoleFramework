namespace ConsoleFramework.Environment.ProcessRunners;

/// <summary>
/// Implements the IContiguousProcessRunner interface to run a contiguous process asynchronously
/// and display its progress in the console window.
/// </summary>
/// <remarks>
/// This class uses a simple console-based UI to display the process's progress and status.
/// It polls the process's progress at regular intervals and updates the console accordingly.
/// If an exception is thrown during the process execution, it catches it and displays an error message.
/// </remarks>
/// <seealso cref="IContiguousProcessRunner"/>
public sealed class SimpleContiguousProcessRunner : IContiguousProcessRunner
{
    /// <summary>
    /// Runs the specified contiguous process asynchronously and displays its progress in the console window.
    /// </summary>
    /// <param name="process">The contiguous process to run.</param>
    /// <returns>A Task object representing the asynchronous operation.</returns>
    public async Task RunProcessAsync(IContiguousProcess process)
    {
        try
        {
            Console.Clear();
            var task = process.RunAsync();

            while (!task.IsCompleted)
            {
                Console.WriteLine($"Running process '{process.Name}'...");
                Console.WriteLine($"Progress: {process.Progress:P}");
                Console.WriteLine($"Status: {process.Status}");

                await Task.Delay(100);
                Console.Clear();
            }

            Console.Clear();
            Console.WriteLine($"Progress: {process.Progress:P}");
            Console.WriteLine($"Status: {process.Status}");
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Error running process: {ex.Message}");
        }
    }
}