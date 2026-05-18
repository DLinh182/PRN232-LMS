namespace PRN232.LMS.API.Models.RequestModels
{
    public class UpdateEnrollmentRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
