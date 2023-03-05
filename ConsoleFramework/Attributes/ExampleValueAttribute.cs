namespace ConsoleFramework.Attributes;

/// <summary>
/// Attribute to provide an example value for an argument.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ExampleValueAttribute : Attribute
{
    /// <summary>
    /// Gets the example value for the argument.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleValueAttribute"/> class with a value.
    /// </summary>
    /// <param name="value">The example value for the argument.</param>
    public ExampleValueAttribute(object value)
    {
        Value = value;
    }
}