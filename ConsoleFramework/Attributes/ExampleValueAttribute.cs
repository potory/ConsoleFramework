namespace ConsoleFramework.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ExampleValueAttribute : Attribute
{
    public object Value { get; }

    public ExampleValueAttribute(object value)
    {
        Value = value;
    }
}