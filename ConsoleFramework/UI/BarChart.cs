namespace ConsoleFramework.UI;

public class BarChart
{
    private readonly List<BarChartData> _data;
    private readonly int _maxWidth;
    private readonly string _title;

    public BarChart(List<BarChartData> data, int maxWidth = 15, string title = null)
    {
        _data = data;
        _maxWidth = maxWidth;
        _title = title;
    }

    public void Print()
    {
        // find the maximum value
        int maxValue = 0;
        int maxLabelLength = 0;

        foreach (BarChartData item in _data)
        {
            if (item.Value > maxValue)
            {
                maxValue = item.Value;
            }

            if (item.Label.Length > maxLabelLength)
            {
                maxLabelLength = item.Label.Length;
            }
        }

        // print the chart
        
        if (!string.IsNullOrEmpty(_title))
        {
            Console.WriteLine(_title);
        }

        foreach (BarChartData item in _data)
        {
            Console.Write(item.Label.PadLeft(maxLabelLength+1) + " | ");
            int width = (int)Math.Round((double)item.Value / maxValue * _maxWidth);
            Console.WriteLine(new string('█', width));
        }
    }
}

public class BarChartData
{
    public string Label { get; }
    public int Value { get; }

    public BarChartData(string label, int value)
    {
        Label = label;
        Value = value;
    }
}