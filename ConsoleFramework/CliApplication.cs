using ConsoleFramework.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFramework;

public class CliApplication
{
    private readonly string _welcomeMessage;

    private readonly CommandFactory _factory;
    private readonly CommandRegistry _registry;

    public CliApplication(string welcomeMessage)
    {
        _welcomeMessage = welcomeMessage;
        var serviceProvider = new ServiceCollection()
            .AddSingleton<CommandRegistry>()
            .AddSingleton<CommandFactory>()
            .AddSingleton(sp => sp)
            .BuildServiceProvider();
        
        _registry = serviceProvider.GetService<CommandRegistry>();
        _factory = serviceProvider.GetService<CommandFactory>();

        if (_registry == null)
        {
            throw new Exception();
        }

        _registry.RegisterCommandType<HelpCommand>();
    }

    public void RegisterCommand<TCommand>() where TCommand: ICommand => 
        _registry.RegisterCommandType<TCommand>();

    public void Run(string[] args)
    {
        if (args is { Length: > 0 })
        {
            var command = _factory.CreateCommand(args);
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