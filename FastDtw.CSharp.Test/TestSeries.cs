using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace FastDtw.CSharp.Test;

/// <summary>
/// Loads column A of Test.csv for tests that need a realistic, smooth series.
/// <para>Parsing is pinned to the invariant culture. The file uses '.' as the decimal separator, and
/// the ambient culture reads "41.98" as 4198 wherever ',' is the decimal separator (for example
/// en-DE), silently scaling every value by 100.</para>
/// </summary>
internal static class TestSeries
{
    private const string FileName = "Test.csv";

    internal static double[] LoadColumnA()
    {
        var values = new List<double>();
        foreach (var line in File.ReadAllLines(FileName))
        {
            var fields = line.Split(',');
            if (double.TryParse(fields[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                values.Add(value);
            }
        }

        return values.ToArray();
    }
}
