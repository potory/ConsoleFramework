using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleFramework.Commands;

[Command(new[] { "help", "h"}, "Displays help information for available commands")]
public class HelpCommand : ICommand
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CommandRegistry _registry;
    
    [Argument(Required = false, Name = "c", Description = "Name of command to display help about")]
    public string CommandName { get; set; }

    public HelpCommand(IServiceProvider serviceProvider, CommandRegistry registry)
    {
        _serviceProvider = serviceProvider;
        _registry = registry;
    }

    public void Evaluate()
    {
        if (string.IsNullOrWhiteSpace(CommandName))
        {
            Console.WriteLine("Available commands:");

            foreach (var commandType in _registry.GetAllCommandTypes().Distinct())
            {
                var commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();

                if (commandAttribute == null)
                {
                    continue;
                }
                
                Console.WriteLine($"{string.Join(", ", commandAttribute.Names)}: {commandAttribute.Description}");
            }

            return;
        }


        try
        {
            var type = _registry.GetAllCommandTypes()
                .Single(x => x.GetCustomAttribute<CommandAttribute>()!.Names.Contains(CommandName.ToLower()));
        
            DisplayCommandHelp(type);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"Unknown command : '{CommandName}'");
        }
    }

    private void DisplayCommandHelp(Type commandType)
    {
        var commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();

        Console.WriteLine($"{string.Join(", ", commandAttribute.Names)}: {commandAttribute.Description}\n");

        var instance = ActivatorUtilities.CreateInstance(_serviceProvider, commandType);
        var properties = commandType.GetProperties()
            .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null);

        var infos = properties as PropertyInfo[] ?? properties.ToArray();

        if (infos.Any())
        {
            Console.WriteLine("  Arguments:");
            foreach (var property in infos)
            {
                var argumentAttribute = property.GetCustomAttribute<ArgumentAttribute>();
                var isRequired = argumentAttribute!.Required ? "required" : "optional";
                string description = !string.IsNullOrWhiteSpace(argumentAttribute.Description) ? " | " + argumentAttribute.Description : string.Empty;

                Console.WriteLine($"    --{argumentAttribute.Name} ({isRequired}): {property.PropertyType.Name}{description}");
            }

            Console.WriteLine($"\nExample usage: {GetExampleUsage(instance)}");
        }
    }

    private static string GetExampleValue(Type type)
    {
        if (type == typeof(int))
        {
            return "42";
        }

        if (type == typeof(double))
        {
            return "3.14";
        }

        if (type == typeof(bool))
        {
            return "true";
        }

        return "\"example string\"";
    }

    private static string GetExampleUsage(object instance)
    {
        var properties = instance.GetType().GetProperties()
            .Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null)
            .ToList();

        var exampleArguments = new List<string>();
        foreach (var property in properties)
        {
            var argumentAttribute = property.GetCustomAttribute<ArgumentAttribute>();
            var exampleValue = GetExampleValue(property.PropertyType);
            var name = argumentAttribute is { Required: true } ? string.Empty : $"--{argumentAttribute.Name}="; 
            
            var exampleArgument = $"{name}{exampleValue}";

            if (!argumentAttribute.Required)
            {
                exampleArgument = $"[{exampleArgument}]";
            }

            exampleArguments.Add(exampleArgument);
        }

        var commandAttribute = instance.GetType().GetCustomAttribute<CommandAttribute>();
        var exampleCommand = commandAttribute.Names.First();

        return $"{exampleCommand} {string.Join(" ", exampleArguments)}";
    }
}