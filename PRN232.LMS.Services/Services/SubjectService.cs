using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Services;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;

    public SubjectService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<PagedResult<SubjectModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands,
        int? credit = null)
    {
        var expandList = expands ?? Array.Empty<string>();

        var query = new SubjectQuery
        {
            Search = search,
            Credit = credit,
            Sort = sort,
            Page = page < 1 ? 1 : page,
            Size = size < 1 ? 10 : size,
            Expands = expandList
        };

        var (items, total) = await _subjectRepository.GetPagedAsync(query);

        return new PagedResult<SubjectModel>
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

    public async Task<SubjectModel?> GetByIdAsync(int id, string[] expands)
    {
        var expandList = expands ?? Array.Empty<string>();
        var subject = await _subjectRepository.GetByIdAsync(id, expandList);

        if (subject == null)
        {
            return null;
        }

        return MapToModel(subject, expandList);
    }

    public async Task<SubjectModel> CreateAsync(SubjectModel model)
    {
        var entity = new Subject
        {
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };

        var created = await _subjectRepository.CreateAsync(entity);
        return MapToModel(created, Array.Empty<string>());
    }

    public async Task<SubjectModel?> UpdateAsync(SubjectModel model)
    {
        var entity = new Subject
        {
            SubjectId = model.SubjectId,
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };

        var updated = await _subjectRepository.UpdateAsync(entity);

        if (updated == null)
        {
            return null;
        }

        return MapToModel(updated, Array.Empty<string>());
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _subjectRepository.DeleteAsync(id);
    }

    private static SubjectModel MapToModel(Subject subject, string[] expands)
    {
        var model = new SubjectModel
        {
            SubjectId = subject.SubjectId,
            SubjectCode = subject.SubjectCode,
            SubjectName = subject.SubjectName,
            Credit = subject.Credit
        };

        var set = expands.Select(x => x.Trim().ToLower()).ToHashSet();

        if (set.Contains("courses") && subject.Courses != null)
        {
            model.Courses = subject.Courses.Select(c => new CourseModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId,
                SubjectId = c.SubjectId
            }).ToList();
        }

        return model;
    }
}