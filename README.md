# ConsoleFramework
ConsoleFramework is a .NET Core library designed to help developers build console applications with ease. It provides a set of interfaces, classes, and attributes that can be used to create a command-line interface (CLI) with support for commands, arguments, and options.

### Installation

You can install ConsoleFramework by cloning its GitHub repository and adding a reference to the ConsoleFramework project in your solution. To do so, follow these steps:

1. Clone the ConsoleFramework repository from GitHub: <br>
```git clone https://github.com/potory/ConsoleFramework.git```
2. Add the ConsoleFramework project to your solution:
   - In Visual Studio, right-click your solution and select "Add" -> "Existing Project".
   - Navigate to the location where you cloned the ConsoleFramework repository and select the `ConsoleFramework.csproj` file.
3. Add a reference to the ConsoleFramework project in your console application project:
   - In Visual Studio, right-click your console application project and select "Add" -> "Reference".
   - In the "Reference Manager" window, select the "Projects" tab.
   - Select the ConsoleFramework project and click "Add".

Now you can start using the ConsoleFramework library in your console application.

### Usage
To create a command-line application with ConsoleFramework, follow these steps:

1. Create a new console application project in Visual Studio.
2. Add a reference to the `ConsoleFramework` project.
3. Define one or more commands by creating classes that implement the `ICommand` interface. Each command can accept dependencies in its constructor.
4. Register the commands in the `CliApplication` by calling the `RegisterCommand` method for each command class.
5. Define the properties of your command class with `[Argument]` attribute.
6. Create a new instance of the `CliApplication` class, passing in the `IServiceProvider` for your application.
7. Register any necessary services with the `ServiceCollection` property of the `CliApplication` instance.
8. Call the Run method of the `CliApplication` instance, passing in the command-line arguments.

For example, let's say you want to create a command-line application with two commands: `Greet` and `Count`. Here's how you would define the commands:

```csharp
[Command("greet", "Greets the user.")]
public class GreetCommand : ICommand
{
    private readonly ILogger<GreetCommand> _logger;
    private readonly IDateTimeService _dateTimeService;

    public GreetCommand(ILogger<GreetCommand> logger, IDateTimeService dateTimeService)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
    }

    [Argument(Name = "name", Description = "The name of the person to greet.")]
    public string Name { get; set; }

    public void Evaluate()
    {
        var timeOfDay = _dateTimeService.Now.Hour < 12 ? "morning" : "afternoon";
        _logger.LogInformation($"Good {timeOfDay}, {Name}!");
    }
}

[Command("count", "Counts the number of words in the specified text.")]
public class CountCommand : ICommand
{
    [Argument(Name = "text", Description = "The text to count the words in.")]
    public string Text { get; set; }

    public void Evaluate()
    {
        var wordCount = Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        Console.WriteLine($"Word count: {wordCount}");
    }
}
```

Then, in your `Main` method, you can register the commands and start the `CliApplication` like this:

```csharp
static void Main(string[] args)
{
    var application = new CliApplication(serviceProvider);
    
    application.ServiceCollection
        .AddLogging()
        .AddScoped<IDateTimeService, DateTimeService>();

    app.RegisterCommand<GreetCommand>();
    app.RegisterCommand<CountCommand>();
    application.Run(args);
}
```

You can run your application from the command line like this:

```
myapp greet --name "John Doe"
myapp count --text "The quick brown fox jumps over the lazy dog."
```