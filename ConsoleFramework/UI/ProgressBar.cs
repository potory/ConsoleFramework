namespace ConsoleFramework.UI;

public class ProgressBar
{
    private readonly int _total;
    private int _value;

    public ProgressBar(int total)
    {
        _total = total;
        _value = 0;
        Console.CursorVisible = false;
    }

    public void Increment(int amount) => 
        _value += amount;

    public void Draw()
    {
        Console.Write("[");
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write(new string(' ', _value));
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(new string(' ', _total - _value));
        Console.Write("]");
        Console.CursorVisible = true;
        Console.Write($" {_value * 100 / _total}%");
        Console.CursorVisible = false;
    }

    public void Complete() => 
        Console.CursorVisible = true;
}