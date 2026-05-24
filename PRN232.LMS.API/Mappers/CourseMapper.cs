using PRN232.LMS.API.Models.ResponseModels;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.API.Mappers;

public static class CourseMapper
{
    public static CourseResponse ToResponse(CourseModel model)
    {
        return new CourseResponse
        {
            CourseId = model.CourseId,
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId,

            Semester = model.Semester == null ? null : new SemesterResponse
            {
                SemesterId = model.Semester.SemesterId,
                SemesterName = model.Semester.SemesterName,
                StartDate = model.Semester.StartDate,
                EndDate = model.Semester.EndDate
            },

            Subject = model.Subject == null ? null : new SubjectResponse
            {
                SubjectId = model.Subject.SubjectId,
                SubjectCode = model.Subject.SubjectCode,
                SubjectName = model.Subject.SubjectName,
                Credit = model.Subject.Credit
            },

            Enrollments = model.Enrollments == null ? null : model.Enrollments.Select(e => new EnrollmentResponse
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status,

                Student = e.Student == null ? null : new StudentResponse
                {
                    StudentId = e.Student.StudentId,
                    FullName = e.Student.FullName,
                    Email = e.Student.Email,
                    DateOfBirth = e.Student.DateOfBirth
                }
            }).ToList()
        };
    }
}