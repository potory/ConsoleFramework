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

        var properties = GetArgumentProperties(commandType).ToList();
        var attributes = properties.Select(x => x.GetCustomAttribute<ArgumentAttribute>()).ToList();

        var namedArguments = GetArguments(args, out var unnamedArguments);

        SetRequiredArguments(commandName, attributes, properties, namedArguments, unnamedArguments, command);
        SetOptionalArguments(attributes, properties, namedArguments, unnamedArguments, command);

        return command;
    }

    private static void SetRequiredArguments(string commandName, List<ArgumentAttribute> attributes, List<PropertyInfo> properties,
        Dictionary<string, string> namedArguments, Queue<string> unnamedArguments, ICommand command)
    {
        while (attributes.Any(x => x.Required))
        {
            int index = attributes.FindIndex(x => x.Required);

            var attribute = attributes[index];
            var prop = properties[index];

            var attributeName = attribute.Name;
            var argument = GetArgument(attributeName, namedArguments, unnamedArguments);

            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException(
                    $"Missing required argument '{attributeName}' for command '{commandName}'");
            }

            var value = GetArgumentValue(argument);

            try
            {
                SetValueToProperty(command, prop, value);
                attributes.Remove(attribute);
                properties.Remove(prop);
            }
            catch (FormatException)
            {
                string errorMessage =
                    $"Failed to set property value: Invalid format for argument '{value}' of argument '{attributeName}'.";

                throw new CliRuntimeException(errorMessage);
            }
        }
    }

    private static void SetOptionalArguments(List<ArgumentAttribute> attributes, List<PropertyInfo> properties, 
        Dictionary<string, string> namedArguments, Queue<string> unnamedArguments, ICommand command)
    {
        while (attributes.Count > 0)
        {
            var attribute = attributes[0];
            var prop = properties[0];

            attributes.RemoveAt(0);
            properties.RemoveAt(0);

            var attributeName = attribute.Name;
            var argument = GetArgument(attributeName, namedArguments, unnamedArguments);

            if (string.IsNullOrEmpty(argument))
            {
                continue;
            }

            var value = GetArgumentValue(argument);

            try
            {
                SetValueToProperty(command, prop, value);
            }
            catch (FormatException)
            {
                string errorMessage =
                    $"Failed to set property value: Invalid format for argument '{value}' of argument '{attribute.Name}'.";

                throw new CliRuntimeException(errorMessage);
            }
        }
    }

    private static Dictionary<string, string> GetArguments(string[] args, out Queue<string> unnamedArguments)
    {
        var namedArguments = new Dictionary<string, string>();
        unnamedArguments = new Queue<string>();

        foreach (var arg in args)
        {
            string name = GetArgumentName(arg);

            if (string.IsNullOrEmpty(name))
            {
                unnamedArguments.Enqueue(GetArgumentValue(arg));
            }
            else
            {
                namedArguments.Add(name, GetArgumentValue(arg));
            }
        }

        return namedArguments;
    }

    private static void SetValueToProperty(ICommand command, PropertyInfo prop, string value)
    {
        // Convert the argument value to the property type and set the property
        var propType = prop.PropertyType;

        var convertedValue = ConvertArgumentValue(value, propType);
        prop.SetValue(command, convertedValue);
    }

    private static IEnumerable<PropertyInfo> GetArgumentProperties(Type commandType) =>
        commandType.GetProperties().Where(p => Attribute.IsDefined(p, typeof(ArgumentAttribute)));

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

    private static string GetArgument(string attributeName, Dictionary<string, string> namedArguments, Queue<string> unnamedArguments)
    {
        string argument = string.Empty;

        if (namedArguments.ContainsKey(attributeName))
        {
            argument = namedArguments[attributeName];
            namedArguments.Remove(attributeName);
        }
        else if (unnamedArguments.Count > 0)
        {
            argument = unnamedArguments.Dequeue();
        }

        return argument;
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

public class CliRuntimeException : Exception
{
    public CliRuntimeException(string errorMessage) : base(errorMessage)
    {
    }
}