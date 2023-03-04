namespace ConsoleFramework.Tests;

using System;
using Xunit;

public class CommandRegistryTests
{
    [Fact]
    public void LoadCommands_LoadsBuiltInCommands()
    {
        // Arrange
        int expectedCount = 2;

        // Act
        CommandRegistry.LoadCommands(typeof(ConsoleApplication).Assembly);

        // Assert
        Assert.Equal(expectedCount, CommandRegistry.GetCommandNames().Count);
    }

    [Fact]
    public void LoadPlugins_LoadsPluginCommands()
    {
        // Arrange
        int expectedCount = 1;

        // Act
        CommandRegistry.LoadPlugins("TestPlugins");

        // Assert
        Assert.Equal(expectedCount, CommandRegistry.GetCommandNames().Count);
    }

    [Fact]
    public void GetCommandType_ReturnsNullForUnknownCommand()
    {
        // Arrange
        string commandName = "foo";

        // Act
        Type commandType = CommandRegistry.GetCommandType(commandName);

        // Assert
        Assert.Null(commandType);
    }

    [Fact]
    public void GetCommandType_ReturnsCommandTypeForKnownCommand()
    {
        // Arrange
        string commandName = "help";
        Type expectedType = typeof(HelpCommand);

        // Act
        CommandRegistry.LoadCommands(typeof(ConsoleApplication).Assembly);
        Type commandType = CommandRegistry.GetCommandType(commandName);

        // Assert
        Assert.Equal(expectedType, commandType);
    }


    [Fact]
    public void CommandAttribute_SetsNameAndDescription()
    {
        // Arrange
        string expectedName = "test";
        string expectedDescription = "This is a test command";

        // Act
        CommandAttribute commandAttribute = new CommandAttribute(expectedName, expectedDescription);

        // Assert
        Assert.Equal(expectedName, commandAttribute.Name);
        Assert.Equal(expectedDescription, commandAttribute.Description);
    }

    [Command("test", "Test command")]
    private class TestCommand : ICommand
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public void Execute(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}