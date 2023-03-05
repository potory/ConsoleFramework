namespace ConsoleFramework.Attributes;

/// <summary>
/// Attribute to mark a property as an argument for a command.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ArgumentAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the argument.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the description of the argument.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// Gets or sets whether the argument is required.
    /// </summary>
    public bool Required { get; set; }
}