using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFramework;

public class CommandFactory
{
    private readonly CommandRegistry _registry;
    private readonly IServiceProvider _provider;
    private readonly Dictionary<Type, CommandArgumentInjector> _fillers = new();

    public CommandFactory(IServiceProvider provider)
    {
        _provider = provider;
        _registry = provider.GetService<CommandRegistry>();
    }

    public ICommand CreateCommand(string input)
    {
        // Split the input into command name and arguments
        var tokens = GetTokens(input);

        var commandName = tokens[0];
        var args = tokens.Skip(1).ToArray();

        return CreateCommand(commandName, args);
    }

    public ICommand CreateCommand(string[] args)
    {
        var commandName = args[0];

        return CreateCommand(commandName, args.Skip(1).ToArray());
    }

    private ICommand CreateCommand(string commandName, string[] args)
    {
        // Get the type of the command based on its name
        var commandType = _registry.GetCommandType(commandName);

        // Create an instance of the command type
        var command = (ICommand)ActivatorUtilities.CreateInstance(_provider, commandType);

        if (!_fillers.ContainsKey(commandType))
        {
            _fillers.Add(commandType, new CommandArgumentInjector(commandType));
        }

        _fillers[commandType].Fill(command, args);

        return command;
    }

    private static string[] GetTokens(string input)
    {
        List<string> output = new List<string>();
        bool inQuotes = false;
        StringBuilder currentString = new StringBuilder();

        foreach (char c in input)
        {
            if (c == '\"')
            {
                inQuotes = !inQuotes;
                currentString.Append(c);
            }
            else if (c == ' ' && !inQuotes)
            {
                output.Add(currentString.ToString());
                currentString.Clear();
            }
            else
            {
                currentString.Append(c);
            }
        }

        output.Add(currentString.ToString());

        return output.ToArray();
    }
}