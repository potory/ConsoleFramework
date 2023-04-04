namespace ConsoleFramework.UI;

public class Table
{
    private readonly List<TableRow> _rows;
    private readonly bool _hasHeader;

    public Table(List<TableRow> rows, bool hasHeader = true)
    {
        _rows = rows;
        _hasHeader = hasHeader;
    }

    public void Print()
    {
        // find the maximum length of each column
        int[] columnWidths = new int[_rows[0].Values.Length];

        foreach (var row in _rows)
        {
            for (int j = 0; j < row.Values.Length; j++)
            {
                int width = row.Values[j].Length;
                if (width > columnWidths[j])
                {
                    columnWidths[j] = width;
                }
            }
        }

        // print the table
        Console.WriteLine("+" + new string('-', columnWidths.Sum(x => x + 3) - 1) + "+");

        for (int i = 0; i < _rows.Count; i++)
        {
            Console.Write("| ");
            for (int j = 0; j < _rows[i].Values.Length; j++)
            {
                Console.Write(_rows[i].Values[j].PadRight(columnWidths[j]) + " | ");
            }
            Console.WriteLine();
            if (i == 0 && _hasHeader)
            {
                Console.WriteLine("+" + new string('-', columnWidths.Sum(x => x + 3) - 1) + "+");
            }
        }

        Console.WriteLine("+" + new string('-', columnWidths.Sum(x => x + 3) - 1) + "+");
    }
}

public class TableRow
{
    public readonly string[] Values;

    public TableRow(string[] values)
    {
        Values = values;
    }
}