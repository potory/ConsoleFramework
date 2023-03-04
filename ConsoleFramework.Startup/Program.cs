namespace ConsoleFramework.Startup;

public static class ConsoleApplication
{
    public static void Main(string[] args)
    {
        string greetings = "Welcome to My CLI Tool!\n"+
                           "Type 'help' to see available commands.\n" +
                           "Let's get started!";

        var cli = new CliApplication(greetings);
        cli.RegisterCommand<ExampleCommand>();

        cli.Run(args);
    }
}