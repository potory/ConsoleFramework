namespace ConsoleFramework.Attributes;

/// <summary>
/// Attribute to mark a class as a command, with one or more aliases for its name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// Gets the array of names for the command.
    /// </summary>
    public string[] Names { get; }
    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class with a single name and a description.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    public CommandAttribute(string name, string description)
    {
        Names = new []{name};
        Description = description;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandAttribute"/> class with multiple names and a description.
    /// </summary>
    /// <param name="names">The names of the command.</param>
    /// <param name="description">The description of the command.</param>
    public CommandAttribute(string[] names, string description)
    {
        Names = names;
        Description = description;
    }
}