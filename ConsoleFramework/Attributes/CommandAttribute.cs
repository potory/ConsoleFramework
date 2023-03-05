namespace ConsoleFramework.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CommandAttribute : Attribute
{
    public string[] Names { get; }
    public string Description { get; }

    public CommandAttribute(string name, string description)
    {
        Names = new []{name};
        Description = description;
    }

    public CommandAttribute(string[] names, string description)
    {
        Names = names;
        Description = description;
    }
}