public class CSVService
{
    public IResult BuildFileResult<T>(
        IEnumerable<T> data,
        string fileName,
        params (string Header, Func<T, object?> Value)[] columns)
    {
        List<string> lines = [];

        // Header row
        lines.Add(string.Join(", ", columns.Select(c => Escape(c.Header))));

        // Data rows
        foreach (var item in data)
        {
            var row = string.Join(", ", columns.Select(c => Escape(c.Value(item)?.ToString())));
            lines.Add(row);
        }

        var csvContent = string.Join("\n", lines);
        var bytes = System.Text.Encoding.UTF8.GetPreamble()
            .Concat(System.Text.Encoding.UTF8.GetBytes(csvContent))
            .ToArray();

        return Results.File(bytes, "text/csv", fileName);
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace(",", " | ");
    }
}
