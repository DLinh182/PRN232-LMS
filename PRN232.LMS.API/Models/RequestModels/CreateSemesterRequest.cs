namespace PRN232.LMS.API.Models.RequestModels;

public class CreateSemesterRequest
{
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}