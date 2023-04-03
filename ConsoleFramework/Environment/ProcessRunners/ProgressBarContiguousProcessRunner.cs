using ConsoleFramework.UI;

namespace ConsoleFramework.Environment.ProcessRunners;

/// <summary>
/// Implements the IContiguousProcessRunner interface to run a contiguous process asynchronously
/// and display its progress using a console-based progress bar.
/// </summary>
/// <remarks>
/// This class draw a progress bar in the console window.
/// It polls the process's progress at regular intervals and updates the progress bar accordingly.
/// If an exception is thrown during the process execution, it catches it and displays an error message.
/// </remarks>
/// <seealso cref="IContiguousProcessRunner"/>
public class ProgressBarContiguousProcessRunner : IContiguousProcessRunner
{
    /// <summary>
    /// Runs the specified contiguous process asynchronously and displays its progress using a console-based progress bar.
    /// </summary>
    /// <param name="process">The contiguous process to run.</param>
    /// <returns>A Task object representing the asynchronous operation.</returns>
    public async Task RunProcessAsync(IContiguousProcess process)
    {
        try
        {
            int progress = 0;

            var progressBar = new ProgressBar(50);

            Log(process, progressBar);
            var task = process.RunAsync();

            while (!task.IsCompleted)
            {
                Log(process, progressBar);

                await Task.Delay(1000);

                var currentProgress = (int) Math.Floor(process.Progress * 50);

                if (currentProgress <= progress)
                {
                    continue;
                }

                progressBar.Increment(currentProgress - progress);
                progress = currentProgress;
            }

            progressBar.Complete();
            Log(process, progressBar);
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Error running process: {ex.Message}");
        }
    }

    private static void Log(IContiguousProcess process, ProgressBar progressBar)
    {
        Console.Clear();

        Console.WriteLine($"Running process '{process.Name}'...");
        Console.WriteLine($"Status: {process.Status}");

        if (!string.IsNullOrEmpty(process.Message))
        {
            FramedMessage.Print(process.Message);
        }

        progressBar.Draw();
    }
}