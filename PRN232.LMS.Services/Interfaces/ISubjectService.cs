using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<PagedResult<SubjectModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands,
        int? credit = null);

    Task<SubjectModel?> GetByIdAsync(int id, string[] expands);

    Task<SubjectModel> CreateAsync(SubjectModel model);

    Task<SubjectModel?> UpdateAsync(SubjectModel model);

    Task<bool> DeleteAsync(int id);
}