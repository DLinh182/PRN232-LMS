using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;

    public CourseService(ICourseRepository courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<PagedResult<CourseModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands,
        int? semesterId = null)
    {
        var expandList = expands ?? Array.Empty<string>();

        var query = new CourseQuery
        {
            Search = search,
            SemesterId = semesterId,
            Sort = sort,
            Page = page < 1 ? 1 : page,
            Size = size < 1 ? 10 : size,
            Expands = expandList
        };

        var (items, total) = await _courseRepository.GetPagedAsync(query);

        return new PagedResult<CourseModel>
        {
            Items = items.Select(x => MapToModel(x, expandList)).ToList(),
            Page = query.Page,
            PageSize = query.Size,
            TotalItems = total,
            TotalPages = total == 0
                ? 0
                : (int)Math.Ceiling(total / (double)query.Size)
        };
    }

    public async Task<CourseModel?> GetByIdAsync(int id, string[] expands)
    {
        var expandList = expands ?? Array.Empty<string>();
        var course = await _courseRepository.GetByIdAsync(id, expandList);

        if (course == null)
        {
            return null;
        }

        return MapToModel(course, expandList);
    }

    public async Task<CourseModel> CreateAsync(CourseModel model)
    {
        var entity = new Course
        {
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId
        };

        var created = await _courseRepository.CreateAsync(entity);
        return MapToModel(created, Array.Empty<string>());
    }

    public async Task<CourseModel?> UpdateAsync(CourseModel model)
    {
        var entity = new Course
        {
            CourseId = model.CourseId,
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId
        };

        var updated = await _courseRepository.UpdateAsync(entity);

        if (updated == null)
        {
            return null;
        }

        return MapToModel(updated, Array.Empty<string>());
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _courseRepository.DeleteAsync(id);
    }

    private static CourseModel MapToModel(Course course, string[] expands)
    {
        var model = new CourseModel
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId,
            SubjectId = course.SubjectId
        };

        var set = expands.Select(x => x.Trim().ToLower()).ToHashSet();

        if (set.Contains("semester") && course.Semester != null)
        {
            model.Semester = new SemesterModel
            {
                SemesterId = course.Semester.SemesterId,
                SemesterName = course.Semester.SemesterName,
                StartDate = course.Semester.StartDate,
                EndDate = course.Semester.EndDate
            };
        }

        if (set.Contains("subject") && course.Subject != null)
        {
            model.Subject = new SubjectModel
            {
                SubjectId = course.Subject.SubjectId,
                SubjectCode = course.Subject.SubjectCode,
                SubjectName = course.Subject.SubjectName,
                Credit = course.Subject.Credit
            };
        }

        if (set.Contains("enrollments") && course.Enrollments != null)
        {
            model.Enrollments = course.Enrollments.Select(e => new EnrollmentModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status,
                Student = e.Student == null ? null : new StudentModel
                {
                    StudentId = e.Student.StudentId,
                    FullName = e.Student.FullName,
                    Email = e.Student.Email,
                    DateOfBirth = e.Student.DateOfBirth
                }
            }).ToList();
        }

        return model;
    }
}