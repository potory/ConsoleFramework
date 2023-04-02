using ConsoleFramework.Abstract;
using ConsoleFramework.Attributes;

namespace ConsoleFramework;

/// <summary>
/// Represents a registry of available command types.
/// </summary>
public class CommandRegistry
{
    private readonly IDictionary<string, Type> _commandTypes = new Dictionary<string, Type>();

    /// <summary>
    /// Registers a command type with the registry.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command to register.</typeparam>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="TCommand"/> is not decorated with <see cref="CommandAttribute"/> or does not implement <see cref="IBaseCommand"/>.</exception>
    public void RegisterCommandType<TCommand>() where TCommand : IBaseCommand
    {
        var type = typeof(TCommand);
        RegisterCommandType(type);
    }

    public void RegisterCommandType(Type type)
    {
        var commandAttribute =
            type.GetCustomAttributes(typeof(CommandAttribute), false).FirstOrDefault() as CommandAttribute;

        if (commandAttribute == null)
        {
            throw new ArgumentException($"Type {type.FullName} must have a CommandAttribute");
        }

        if (!typeof(IBaseCommand).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type {type.FullName} must implement IBaseCommand");
        }

        foreach (var commandName in commandAttribute.Names)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentException(
                    $"CommandAttribute of type {type.FullName} must have non-empty name(s) specified");
            }

            _commandTypes.Add(commandName.ToLowerInvariant(), type);
        }
    }

    /// <summary>
    /// Gets the type of a command with the specified name.
    /// </summary>
    /// <param name="commandName">The name of the command.</param>
    /// <returns>The type of the command with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown when a command with the specified name is not registered.</exception>
    public Type GetCommandType(string commandName)
    {
        var commandExist = _commandTypes.TryGetValue(commandName.ToLowerInvariant(), out var commandType);
        
        if (!commandExist)
        {
            throw new ArgumentException($"Unknown command '{commandName}'");
        }
        
        return commandType;
    }

    /// <summary>
    /// Gets all registered command types.
    /// </summary>
    /// <returns>An enumerable of all registered command types.</returns>
    public IEnumerable<Type> GetAllCommandTypes() => _commandTypes.Select(x => x.Value);
}