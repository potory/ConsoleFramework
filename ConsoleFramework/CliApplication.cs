using ConsoleFramework.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFramework;

/// <summary>
/// Represents a command-line interface (CLI) application.
/// </summary>
public class CliApplication
{
    private readonly string _welcomeMessage;

    private readonly CommandFactory _factory;
    private readonly CommandRegistry _registry;

    public IServiceCollection ServiceCollection { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliApplication"/> class with the specified welcome message.
    /// </summary>
    /// <param name="welcomeMessage">The welcome message to be displayed when the application is started.</param>
    public CliApplication(string welcomeMessage)
    {
        _welcomeMessage = welcomeMessage;
        ServiceCollection = new ServiceCollection()
            .AddSingleton<CommandRegistry>()
            .AddSingleton<CommandFactory>()
            .AddSingleton(sp => sp);
    }

    /// <summary>
    /// Registers a command type with the application.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to register.</typeparam>
    public void RegisterCommand<TCommand>() where TCommand: ICommand => 
        _registry.RegisterCommandType<TCommand>();

    /// <summary>
    /// Runs the application with the specified command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments to pass to the application.</param>
    public void Run(string[] args)
    {
        var serviceProvider = ServiceCollection.BuildServiceProvider();

        var registry = serviceProvider.GetService<CommandRegistry>();
        var factory = serviceProvider.GetService<CommandFactory>();

        if (registry == null)
        {
            throw new Exception();
        }

        registry.RegisterCommandType<HelpCommand>();
        
        if (args is { Length: > 0 })
        {
            var command = factory.CreateCommand(args);
            command.Evaluate();
            return;
        }

        Console.WriteLine(_welcomeMessage);

        while (true)
        {
            RunUserInput();
        }
    }

    private void RunUserInput()
    {
        Console.Write("> ");
        var input = Console.ReadLine();

        var commandName = input?.Split(' ')[0];

        try
        {
            var commandType = _registry.GetCommandType(commandName);

            if (commandType == null)
            {
                Console.WriteLine($"Command '{commandName}' not found. Type 'help' for a list of available commands.");
            }

            var command = _factory.CreateCommand(input);
            command.Evaluate();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}