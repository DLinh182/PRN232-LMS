using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public EnrollmentService(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<PagedResult<EnrollmentModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands)
    {
        var expandList = expands ?? Array.Empty<string>();

        var query = new EnrollmentQuery
        {
            Search = search,
            Sort = sort,
            Page = page < 1 ? 1 : page,
            Size = size < 1 ? 10 : size,
            Expands = expandList
        };

        var (items, total) = await _enrollmentRepository.GetPagedAsync(query);

        return new PagedResult<EnrollmentModel>
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

    public async Task<EnrollmentModel?> GetByIdAsync(int id, string[] expands)
    {
        var expandList = expands ?? Array.Empty<string>();

        var enrollment = await _enrollmentRepository.GetByIdAsync(id, expandList);

        if (enrollment == null)
        {
            return null;
        }

        return MapToModel(enrollment, expandList);
    }

    public async Task<EnrollmentModel> CreateAsync(EnrollmentModel model)
    {
        var entity = new Enrollment
        {
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status
        };

        var created = await _enrollmentRepository.CreateAsync(entity);

        return MapToModel(created, Array.Empty<string>());
    }

    public async Task<EnrollmentModel?> UpdateAsync(EnrollmentModel model)
    {
        var entity = new Enrollment
        {
            EnrollmentId = model.EnrollmentId,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status
        };

        var updated = await _enrollmentRepository.UpdateAsync(entity);

        if (updated == null)
        {
            return null;
        }

        return MapToModel(updated, Array.Empty<string>());
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _enrollmentRepository.DeleteAsync(id);
    }

    private static EnrollmentModel MapToModel(Enrollment enrollment, string[] expands)
    {
        var model = new EnrollmentModel
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        };

        var set = expands
            .Select(x => x.Trim().ToLower())
            .ToHashSet();

        if (set.Contains("student") && enrollment.Student != null)
        {
            model.Student = new StudentModel
            {
                StudentId = enrollment.Student.StudentId,
                FullName = enrollment.Student.FullName,
                Email = enrollment.Student.Email,
                DateOfBirth = enrollment.Student.DateOfBirth
            };
        }

        if (set.Contains("course") && enrollment.Course != null)
        {
            model.Course = new CourseModel
            {
                CourseId = enrollment.Course.CourseId,
                CourseName = enrollment.Course.CourseName
            };
        }

        return model;
    }
}