namespace PRN232.LMS.API.Common;

public class PagedApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public PaginationMetadata? Pagination { get; set; }
    public object? Errors { get; set; }
}