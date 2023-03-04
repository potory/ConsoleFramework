namespace ConsoleFramework;

[AttributeUsage(AttributeTargets.Property)]
public class ArgumentAttribute : Attribute
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Required { get; set; }
}