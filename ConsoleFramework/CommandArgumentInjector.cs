using System.Reflection;
using ConsoleFramework.Abstract;
using ConsoleFramework.Attributes;

namespace ConsoleFramework;

/// <summary>
/// Provides functionality to inject command arguments into a command object.
/// </summary>
public class CommandArgumentInjector
{
    private readonly string _commandName;

    private readonly Queue<string> _unnamedArguments = new();
    private readonly Dictionary<string, string> _namedArguments = new();

    private readonly IReadOnlyList<PropertyInfo> _requiredProperties;
    private readonly IReadOnlyList<PropertyInfo> _optionalProperties;

    private readonly IReadOnlyList<ArgumentAttribute> _requiredAttributes;
    private readonly IReadOnlyList<ArgumentAttribute> _optionalAttributes;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgumentInjector"/> class with the specified type.
    /// </summary>
    /// <param name="type">The type of object to inject command arguments into.</param>
    public CommandArgumentInjector(Type type)
    {
        _commandName = type.GetCustomAttribute<CommandAttribute>()?.Names.First();
        
        _requiredProperties = type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(ArgumentAttribute)) && p.GetCustomAttribute<ArgumentAttribute>()!.Required).ToArray();
        _optionalProperties = type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(ArgumentAttribute)) && !p.GetCustomAttribute<ArgumentAttribute>()!.Required).ToArray();

        _requiredAttributes = _requiredProperties.Select(x => x.GetCustomAttribute<ArgumentAttribute>()).ToArray();
        _optionalAttributes = _optionalProperties.Select(x => x.GetCustomAttribute<ArgumentAttribute>()).ToArray();
    }

    /// <summary>
    /// Fills the specified command object with the given arguments.
    /// </summary>
    /// <param name="command">The command object to fill.</param>
    /// <param name="args">The arguments to fill the command object with.</param>
    public void Fill(IBaseCommand command, string[] args)
    {
        CacheArguments(args);

        SetRequiredArguments(command);
        SetOptionalArguments(command);
    }

    /// <summary>
    /// Caches the given arguments for later use.
    /// </summary>
    /// <param name="args">The arguments to cache.</param>
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

    /// <summary>
    /// Sets the required arguments of the given command object.
    /// </summary>
    /// <param name="command">The command object to set the required arguments for.</param>
    private void SetRequiredArguments(IBaseCommand command)
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

    /// <summary>
    /// Sets the optional arguments of the given command object.
    /// </summary>
    /// <param name="command">The command object to set the optional arguments for.</param>
    private void SetOptionalArguments(IBaseCommand command)
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

    /// <summary>
    /// Sets the value of the specified property on the given command object to the specified value.
    /// </summary>
    /// <param name="command">The command object to set the property value on.</param>
    /// <param name="prop">The property to set the value of.</param>
    /// <param name="value">The value to set the property to.</param>
    private static void SetValueToProperty(IBaseCommand command, PropertyInfo prop, string value)
    {
        // Convert the argument value to the property type and set the property
        var propType = prop.PropertyType;

        var convertedValue = ConvertArgumentValue(value, propType);
        prop.SetValue(command, convertedValue);
    }

    /// <summary>
    /// Gets the argument with the specified attribute name.
    /// </summary>
    /// <param name="attributeName">The name of the attribute to get the argument for.</param>
    /// <returns>The argument with the specified attribute name.</returns>
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

    /// <summary>
    /// Converts the specified argument value to the specified target type.
    /// </summary>
    /// <param name="arg">The argument value to convert.</param>
    /// <param name="targetType">The type to convert the argument value to.</param>
    /// <returns>The converted argument value.</returns>
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

    /// <summary>
    /// Gets the default value for the specified type.
    /// </summary>
    /// <param name="type">The type to get the default value for.</param>
    /// <returns>The default value for the specified type.</returns>
    private static object GetDefaultValue(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    /// <summary>
    /// Gets the name of the argument from the specified argument string.
    /// </summary>
    /// <param name="arg">The argument string to get the name from.</param>
    /// <returns>The name of the argument.</returns>
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
        var name = parts[0][2..];
        return name;
    }

    /// <summary>
    /// Gets the value of the argument from the specified argument string.
    /// </summary>
    /// <param name="arg">The argument string to get the value from.</param>
    /// <returns>The value of the argument.</returns>
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