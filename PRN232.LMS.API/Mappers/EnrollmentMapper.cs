using PRN232.LMS.API.Models.ResponseModels;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.API.Mappers;

public static class EnrollmentMapper
{
    public static EnrollmentResponse ToResponse(EnrollmentModel model)
    {
        return new EnrollmentResponse
        {
            EnrollmentId = model.EnrollmentId,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status,
            Student = model.Student == null ? null : new StudentResponse
            {
                StudentId = model.Student.StudentId,
                FullName = model.Student.FullName,
                Email = model.Student.Email,
                DateOfBirth = model.Student.DateOfBirth
            },
            Course = model.Course == null ? null : new CourseResponse
            {
                CourseId = model.Course.CourseId,
                CourseName = model.Course.CourseName
            }
        };
    }
}