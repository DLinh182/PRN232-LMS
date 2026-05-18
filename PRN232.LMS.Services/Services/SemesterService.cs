using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Services;

public class SemesterService : ISemesterService
{
    private readonly ISemesterRepository _semesterRepository;

    public SemesterService(ISemesterRepository semesterRepository)
    {
        _semesterRepository = semesterRepository;
    }

    public async Task<PagedResult<SemesterModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands)
    {
        var expandList = expands ?? Array.Empty<string>();

        var query = new SemesterQuery
        {
            Search = search,
            Sort = sort,
            Page = page < 1 ? 1 : page,
            Size = size < 1 ? 10 : size,
            Expands = expandList
        };

        var (items, total) = await _semesterRepository.GetPagedAsync(query);

        return new PagedResult<SemesterModel>
        {
            Items = items.Select(MapToModel).ToList(),
            Page = query.Page,
            PageSize = query.Size,
            TotalItems = total,
            TotalPages = total == 0
                ? 0
                : (int)Math.Ceiling(total / (double)query.Size)
        };
    }

    public async Task<SemesterModel?> GetByIdAsync(int id, string[] expands)
    {
        var expandList = expands ?? Array.Empty<string>();
        var semester = await _semesterRepository.GetByIdAsync(id, expandList);

        if (semester == null)
        {
            return null;
        }

        return MapToModel(semester);
    }

    public async Task<SemesterModel> CreateAsync(SemesterModel model)
    {
        var entity = new Semester
        {
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

        var created = await _semesterRepository.CreateAsync(entity);
        return MapToModel(created);
    }

    public async Task<SemesterModel?> UpdateAsync(SemesterModel model)
    {
        var entity = new Semester
        {
            SemesterId = model.SemesterId,
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

        var updated = await _semesterRepository.UpdateAsync(entity);

        if (updated == null)
        {
            return null;
        }

        return MapToModel(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _semesterRepository.DeleteAsync(id);
    }

    private static SemesterModel MapToModel(Semester semester)
    {
        return new SemesterModel
        {
            SemesterId = semester.SemesterId,
            SemesterName = semester.SemesterName,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate
        };
    }
}