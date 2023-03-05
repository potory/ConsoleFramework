using ConsoleFramework.Attributes;

namespace ConsoleFramework;

public class CommandRegistry
{
    private readonly IDictionary<string, Type> _commandTypes = new Dictionary<string, Type>();

    public void RegisterCommandType<TCommand>() where TCommand : ICommand
    {
        var type = typeof(TCommand);
        var commandAttribute =
            type.GetCustomAttributes(typeof(CommandAttribute), false).FirstOrDefault() as CommandAttribute;

        if (commandAttribute == null)
        {
            throw new ArgumentException($"Type {type.FullName} must have a CommandAttribute");
        }

        if (!typeof(ICommand).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type {type.FullName} must implement ICommand");
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

    public Type GetCommandType(string commandName)
    {
        var commandExist = _commandTypes.TryGetValue(commandName.ToLowerInvariant(), out var commandType);
        
        if (!commandExist)
        {
            throw new ArgumentException($"Unknown command '{commandName}'");
        }
        
        return commandType;
    }

    public IEnumerable<Type> GetAllCommandTypes() => _commandTypes.Select(x => x.Value);
}