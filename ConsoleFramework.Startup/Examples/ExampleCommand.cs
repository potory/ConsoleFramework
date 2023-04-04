using ConsoleFramework;
using ConsoleFramework.Abstract;
using ConsoleFramework.Attributes;

[Command("example", "An example command")]
public class ExampleCommand : ICommand
{
    [Argument(Name = "message", Required = true)]
    public string Message { get; set; }
    [Argument(Name = "optional", Required = false, Description = "some optional value for things")]
    public int Optional { get; set; }

    public void Evaluate()
    {
        if (Optional == 0)
        {
            Console.WriteLine(Message);
        }
        else
        {
            Console.WriteLine(Message + " optional: " + Optional);
        }
    }
}