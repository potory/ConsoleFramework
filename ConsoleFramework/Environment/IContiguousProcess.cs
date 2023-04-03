namespace ConsoleFramework.Environment;

public interface IContiguousProcess
{
    string Name { get; }
    double Progress { get; }
    ProcessStatus Status { get; }
    Task RunAsync();
}