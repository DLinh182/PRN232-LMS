using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<PagedResult<SemesterModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands);

    Task<SemesterModel?> GetByIdAsync(int id, string[] expands);

    Task<SemesterModel> CreateAsync(SemesterModel model);

    Task<SemesterModel?> UpdateAsync(SemesterModel model);

    Task<bool> DeleteAsync(int id);
}