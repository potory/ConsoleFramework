using ConsoleFramework.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFramework;

/// <summary>
/// Represents a command-line interface (CLI) application.
/// </summary>
public class CliApplication
{
    private readonly string _welcomeMessage;

    private CommandFactory _factory;
    private CommandRegistry _registry;

    private readonly HashSet<Type> _commandsToRegister = new();

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
        _commandsToRegister.Add(typeof(TCommand));

    /// <summary>
    /// Runs the application with the specified command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments to pass to the application.</param>
    public async Task Run(string[] args)
    {
        var serviceProvider = ServiceCollection.BuildServiceProvider();

        _registry = serviceProvider.GetService<CommandRegistry>();
        _factory = serviceProvider.GetService<CommandFactory>();

        if (_registry == null)
        {
            throw new Exception();
        }

        _registry.RegisterCommandType<HelpCommand>();

        foreach (var type in _commandsToRegister)
        {
            _registry.RegisterCommandType(type);
        }
        
        if (args is { Length: > 0 })
        {
            var command = _factory.CreateCommand(args);
            await RunCommand(command);

            return;
        }

        Console.WriteLine(_welcomeMessage);

        while (true)
        {
            await RunUserInput();
        }
    }

    private async Task RunUserInput()
    {
        Console.Write("> ");
        var input = Console.ReadLine();
        
        try
        {
            var commandName = input?.Split(' ')[0];
            var commandType = _registry.GetCommandType(commandName);

            if (commandType == null)
            {
                Console.WriteLine($"Command '{commandName}' not found. Type 'help' for a list of available commands.");
            }

            var command = _factory.CreateCommand(input);
            await RunCommand(command);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static async Task RunCommand(ICommand command)
    {
        if (command is IAsyncCommand asyncCommand)
        {
            await asyncCommand.EvaluateAsync();
            return;
        }
        
        command.Evaluate();
    }
}