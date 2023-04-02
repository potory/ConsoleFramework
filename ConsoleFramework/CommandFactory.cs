using System.Text;
using ConsoleFramework.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFramework;

/// <summary>
/// Represents a factory for creating command instances.
/// </summary>
public class CommandFactory
{
    private readonly CommandRegistry _registry;
    private readonly IServiceProvider _provider;
    private readonly Dictionary<Type, CommandArgumentInjector> _fillers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandFactory"/> class with the specified service provider.
    /// </summary>
    /// <param name="provider">The service provider to use for creating command instances.</param>
    public CommandFactory(IServiceProvider provider)
    {
        _provider = provider;
        _registry = provider.GetService<CommandRegistry>();
    }

    /// <summary>
    /// Creates a command instance with the specified input.
    /// </summary>
    /// <param name="input">The input string to create the command from.</param>
    /// <returns>A new command instance.</returns>
    public IBaseCommand CreateCommand(string input)
    {
        // Split the input into command name and arguments
        var tokens = GetTokens(input);

        var commandName = tokens[0];
        var args = tokens.Skip(1).ToArray();

        return CreateCommand(commandName, args);
    }

    /// <summary>
    /// Creates a command instance with the specified arguments.
    /// </summary>
    /// <param name="args">The arguments to create the command from.</param>
    /// <returns>A new command instance.</returns>
    public IBaseCommand CreateCommand(string[] args)
    {
        var commandName = args[0];

        return CreateCommand(commandName, args.Skip(1).ToArray());
    }

    private IBaseCommand CreateCommand(string commandName, string[] args)
    {
        // Get the type of the command based on its name
        var commandType = _registry.GetCommandType(commandName);

        // Create an instance of the command type
        var command = (IBaseCommand)ActivatorUtilities.CreateInstance(_provider, commandType);

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