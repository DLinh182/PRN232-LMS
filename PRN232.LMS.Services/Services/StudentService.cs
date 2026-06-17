using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<PagedResult<StudentModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands,
        string? email = null)
    {
        var query = new StudentQuery
        {
            Search = search,
            Email = email,
            Sort = sort,
            Page = page < 1 ? 1 : page,
            Size = size < 1 ? 10 : size,
            Expands = expands ?? Array.Empty<string>()
        };

        var (items, total) = await _studentRepository.GetPagedAsync(query);

        return new PagedResult<StudentModel>
        {
            Items = items.Select(x => MapToModel(x, expands ?? Array.Empty<string>())).ToList(),
            Page = query.Page,
            PageSize = query.Size,
            TotalItems = total,
            TotalPages = total == 0
                ? 0
                : (int)Math.Ceiling(total / (double)query.Size)
        };
    }

    public async Task<StudentModel?> GetByIdAsync(int id, string[] expands)
    {
        var student = await _studentRepository.GetByIdAsync(
            id,
            expands ?? Array.Empty<string>());

        if (student == null)
        {
            return null;
        }

        return MapToModel(student, expands ?? Array.Empty<string>());
    }

    public async Task<StudentModel> CreateAsync(StudentModel model)
    {
        var entity = new Student
        {
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };

        var created = await _studentRepository.CreateAsync(entity);

        return MapToModel(created, Array.Empty<string>());
    }

    public async Task<StudentModel?> UpdateAsync(StudentModel model)
    {
        var entity = new Student
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };

        var updated = await _studentRepository.UpdateAsync(entity);

        if (updated == null)
        {
            return null;
        }

        return MapToModel(updated, Array.Empty<string>());
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _studentRepository.DeleteAsync(id);
    }

    private static StudentModel MapToModel(Student student, string[] expands)
    {
        var model = new StudentModel
        {
            StudentId = student.StudentId,
            FullName = student.FullName,
            Email = student.Email,
            DateOfBirth = student.DateOfBirth
        };

        var set = expands
            .Select(x => x.Trim().ToLower())
            .ToHashSet();

        if (set.Contains("enrollments") && student.Enrollments.Count > 0)
        {
            var includeCourse = set.Contains("course");

            model.Enrollments = student.Enrollments.Select(e => new EnrollmentModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status,
                Course = includeCourse && e.Course != null
                    ? new CourseModel
                    {
                        CourseId = e.Course.CourseId,
                        CourseName = e.Course.CourseName,
                        SemesterId = e.Course.SemesterId,
                        SubjectId = e.Course.SubjectId,
                        Semester = set.Contains("semester") && e.Course.Semester != null
                            ? new SemesterModel
                            {
                                SemesterId = e.Course.Semester.SemesterId,
                                SemesterName = e.Course.Semester.SemesterName,
                                StartDate = e.Course.Semester.StartDate,
                                EndDate = e.Course.Semester.EndDate
                            }
                            : null
                    }
                    : null
            }).ToList();
        }

        return model;
    }
}