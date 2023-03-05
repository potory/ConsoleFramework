using System.Reflection;
using ConsoleFramework.Attributes;

namespace ConsoleFramework;

public class CommandArgumentInjector
{
    private readonly string _commandName;

    private readonly Queue<string> _unnamedArguments = new();
    private readonly Dictionary<string, string> _namedArguments = new();

    private readonly IReadOnlyList<PropertyInfo> _requiredProperties;
    private readonly IReadOnlyList<PropertyInfo> _optionalProperties;

    private readonly IReadOnlyList<ArgumentAttribute> _requiredAttributes;
    private readonly IReadOnlyList<ArgumentAttribute> _optionalAttributes;

    public CommandArgumentInjector(Type type)
    {
        _commandName = type.GetCustomAttribute<CommandAttribute>()?.Names.First();
        
        _requiredProperties = type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(ArgumentAttribute)) && p.GetCustomAttribute<ArgumentAttribute>()!.Required).ToArray();
        _optionalProperties = type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(ArgumentAttribute)) && !p.GetCustomAttribute<ArgumentAttribute>()!.Required).ToArray();

        _requiredAttributes = _requiredProperties.Select(x => x.GetCustomAttribute<ArgumentAttribute>()).ToArray();
        _optionalAttributes = _optionalProperties.Select(x => x.GetCustomAttribute<ArgumentAttribute>()).ToArray();
    }

    public void Fill(ICommand command, string[] args)
    {
        CacheArguments(args);

        SetRequiredArguments(command);
        SetOptionalArguments(command);
    }

    private void CacheArguments(string[] args)
    {
        _namedArguments.Clear();
        _unnamedArguments.Clear();

        foreach (var arg in args)
        {
            string name = GetArgumentName(arg);

            if (string.IsNullOrEmpty(name))
            {
                _unnamedArguments.Enqueue(GetArgumentValue(arg));
            }
            else
            {
                _namedArguments.Add(name, GetArgumentValue(arg));
            }
        }
    }

    private void SetRequiredArguments(ICommand command)
    {
        int count = _requiredAttributes.Count;

        for (int index = 0; index < count; index++)
        {
            var attribute = _requiredAttributes[index];
            var prop = _requiredProperties[index];

            var attributeName = attribute.Name;
            var argument = GetArgument(attributeName);
            
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException(
                    $"Missing required argument '{attributeName}' for command '{_commandName}'");
            }
            
            var value = GetArgumentValue(argument);

            try
            {
                SetValueToProperty(command, prop, value);
            }
            catch (FormatException)
            {
                string errorMessage =
                    $"Failed to set property value: Invalid format for argument '{value}' of argument '{attributeName}'.";

                throw new CliRuntimeException(errorMessage);
            }
        }
    }

    private void SetOptionalArguments(ICommand command)
    {
        int count = _optionalProperties.Count;

        for (int index = 0; index < count; index++)
        {
            var attribute = _optionalAttributes[index];
            var prop = _optionalProperties[index];
            
            var attributeName = attribute.Name;
            var argument = GetArgument(attributeName);

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
                    $"Failed to set property value: Invalid format for argument '{value}' of argument '{attributeName}'.";

                throw new CliRuntimeException(errorMessage);
            }
        }
    }

    private static void SetValueToProperty(ICommand command, PropertyInfo prop, string value)
    {
        // Convert the argument value to the property type and set the property
        var propType = prop.PropertyType;

        var convertedValue = ConvertArgumentValue(value, propType);
        prop.SetValue(command, convertedValue);
    }

    private string GetArgument(string attributeName)
    {
        string argument = string.Empty;

        if (_namedArguments.ContainsKey(attributeName))
        {
            argument = _namedArguments[attributeName];
        }
        else if (_unnamedArguments.Count > 0)
        {
            argument = _unnamedArguments.Dequeue();
        }

        return argument;
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
}