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
                    CourseName = e.Course.CourseName,
                    SemesterId = e.Course.SemesterId,
                    SubjectId = e.Course.SubjectId,
                    Semester = e.Course.Semester == null ? null : new SemesterResponse
                    {
                        SemesterId = e.Course.Semester.SemesterId,
                        SemesterName = e.Course.Semester.SemesterName,
                        StartDate = e.Course.Semester.StartDate,
                        EndDate = e.Course.Semester.EndDate
                    }
                }
            }).ToList()
        };
    }
}