namespace PRN232.LMS.API.Common;

public class QueryParameters
{
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Fields { get; set; }
    public string? Expand { get; set; }

    public string[] GetExpandList()
        => string.IsNullOrWhiteSpace(Expand)
            ? Array.Empty<string>()
            : Expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public string[] GetFieldList()
        => string.IsNullOrWhiteSpace(Fields)
            ? Array.Empty<string>()
            : Fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}