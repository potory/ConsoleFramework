namespace ConsoleFramework.UI;

public static class FramedMessage
{
    public static void Print(string message)
    {
        string[] lines = message.Split("\n");

        int maxLineLength = 0;
        foreach (string line in lines)
        {
            if (line.Length > maxLineLength)
            {
                maxLineLength = line.Length;
            }
        }

        int width = maxLineLength + 4;

        // Print top border
        Console.WriteLine(new string('-', width));

        // Print message with side borders
        foreach (string line in lines)
        {
            Console.WriteLine("| " + line.PadRight(maxLineLength) + " |");
        }

        // Print bottom border
        Console.WriteLine(new string('-', width));
    }
}