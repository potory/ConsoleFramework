namespace ConsoleFramework;

public class CliRuntimeException : Exception
{
    public CliRuntimeException(string errorMessage) : base(errorMessage)
    {
    }
}