using ConsoleFramework.Abstract;
using ConsoleFramework.Attributes;
using ConsoleFramework.Environment;

namespace ConsoleFramework.Examples;

/// <summary>
/// A command that runs an example contiguous process asynchronously.
/// </summary>
[Command("contiguous-example", "Runs an example contiguous process.")]
public sealed class ExampleContiguousCommand : IAsyncCommand
{
    private readonly IContiguousProcessRunner _runner;

    /// <summary>
    /// Initializes a new instance of the ExampleContiguousCommand class.
    /// </summary>
    /// <param name="runner">The IContiguousProcessRunner implementation to use for running the process.</param>
    public ExampleContiguousCommand(IContiguousProcessRunner runner)
    {
        _runner = runner;
    }

    /// <summary>
    /// Runs the example contiguous process asynchronously.
    /// </summary>
    public async Task EvaluateAsync()
    {
        await _runner.RunProcessAsync(new ExampleContiguousProcess());
    }
}

/// <summary>
/// An example contiguous process that runs for a fixed amount of time and reports progress.
/// </summary>
public class ExampleContiguousProcess : IContiguousProcess
{
    /// <summary>
    /// Gets the name of the process.
    /// </summary>
    public string Name => "Example contiguous process";

    /// <summary>
    /// Gets the progress of the process, ranging from 0 to 1.
    /// </summary>
    public double Progress { get; private set; }

    /// <summary>
    /// Gets the status of the process.
    /// </summary>
    public ProcessStatus Status { get; private set; }

    /// <summary>
    /// Runs the process asynchronously.
    /// </summary>
    public async Task RunAsync()
    {
        Status = ProcessStatus.Running;
        Progress = 0;

        // Run the process for 10 seconds, updating the progress every second.
        for (int i = 0; i < 10; i++)
        {
            Progress = (double) i / 10;
            await Task.Delay(1000);
        }

        Progress = 1;
        Status = ProcessStatus.Completed;
    }
}