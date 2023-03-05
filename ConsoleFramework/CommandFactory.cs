using System.Reflection;
using System.Text;
using ConsoleFramework.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFramework;

public class CommandFactory
{
    private readonly CommandRegistry _registry;
    private readonly IServiceProvider _provider;

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

        // Get the type of the command based on its name
        var commandType = _registry.GetCommandType(commandName);

        return CreateCommand(commandType, args, commandName);
    }

    public ICommand CreateCommand(string[] args)
    {
        var commandName = args[0];

        // Get the type of the command based on its name
        var commandType = _registry.GetCommandType(commandName);

        return CreateCommand(commandType, args.Skip(1).ToArray(), commandName);
    }

    private ICommand CreateCommand(Type commandType, string[] args, string commandName)
    {
        // Create an instance of the command type
        var command = (ICommand)ActivatorUtilities.CreateInstance(_provider, commandType);

        // Populate the command's properties based on the arguments
        var argIndex = 0;
        foreach (var prop in GetArgumentProperties(commandType))
        {
            var attribute = prop.GetCustomAttribute<ArgumentAttribute>();

            if (attribute == null)
            {
                throw new ArgumentException();
            }

            if (argIndex >= args.Length && attribute!.Required)
            {
                throw new ArgumentException(
                    $"Missing required argument '{attribute.Name}' for command '{commandName}'");
            }

            var arg = argIndex < args.Length ? args[argIndex] : null;

            // If the argument has a name, extract it
            var argName = GetArgumentName(arg);

            if (argName != null)
            {
                if (argName != attribute.Name)
                {
                    throw new ArgumentException(
                        $"Unexpected argument '{argName}' for command '{commandName}', expected '{prop.Name}'");
                }
            }

            arg = GetArgumentValue(arg);

            // Convert the argument value to the property type and set the property
            var propType = prop.PropertyType;

            try
            {
                var convertedValue = ConvertArgumentValue(arg, propType);
                prop.SetValue(command, convertedValue);

                argIndex++;
            }
            catch (FormatException ex)
            {
                string errorMessage =
                    $"Failed to set property value: Invalid format for argument '{arg}' of argument '{attribute.Name}'.";
                throw new CliRuntimeException(errorMessage);
            }
        }

        return command;
    }

    private static IEnumerable<PropertyInfo> GetArgumentProperties(Type commandType)
    {
        return commandType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(ArgumentAttribute)));
    }

    private static string GetArgumentName(string arg)
    {
        if (arg == null)
        {
            return null;
        }

        if (!arg.StartsWith("--"))
        {
            return null;
        }

        var parts = arg.Split('=');
        var name = parts[0].Substring(2);
        return name;
    }

    private static string GetArgumentValue(string arg)
    {
        if (arg == null)
        {
            return null;
        }

        if (!arg.StartsWith("--"))
        {
            return arg.Trim('"');
        }

        var parts = arg.Split('=');

        if (parts.Length == 1)
        {
            return "true";
        }

        var value = parts[1].Trim('"');
        return value;
    }

    private static object ConvertArgumentValue(string arg, Type targetType)
    {
        if (arg == null)
        {
            return GetDefaultValue(targetType);
        }

        Type underlyingType = Nullable.GetUnderlyingType(targetType);

        if (underlyingType == null)
        {
            var convertedValue = targetType.IsEnum
                ? Enum.Parse(targetType, arg, ignoreCase: true)
                : Convert.ChangeType(arg, targetType);

            return convertedValue;
        }
        else
        {
            var convertedValue = targetType.IsEnum
                ? Enum.Parse(targetType, arg, ignoreCase: true)
                : Convert.ChangeType(arg, underlyingType);
            
            Type nullableType = typeof(Nullable<>).MakeGenericType(underlyingType);
            return Activator.CreateInstance(nullableType, convertedValue);
        }
    }

    private static object GetDefaultValue(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    public static string[] GetTokens(string input)
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

public class CliRuntimeException : Exception
{
    public CliRuntimeException(string errorMessage) : base(errorMessage)
    {
    }
}