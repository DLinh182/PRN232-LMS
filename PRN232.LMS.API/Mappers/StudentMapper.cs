using PRN232.LMS.API.Models.ResponseModels;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.API.Mappers;

public static class StudentMapper
{
    public static StudentResponse ToResponse(StudentModel model)
    {
        return new StudentResponse
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth,
            Enrollments = model.Enrollments?.Select(e => new EnrollmentResponse
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status,
                Course = e.Course == null ? null : new CourseResponse
                {
                    CourseId = e.Course.CourseId,
                    CourseName = e.Course.CourseName
                }
            }).ToList()
        };
    }
}