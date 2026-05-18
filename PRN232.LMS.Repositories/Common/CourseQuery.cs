namespace PRN232.LMS.Repositories.Common;

public class CourseQuery
{
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string[] Expands { get; set; } = Array.Empty<string>();

    public int Skip => (Page - 1) * Size;
}