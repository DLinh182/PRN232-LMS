using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<PagedResult<CourseModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands,
        int? semesterId = null);

    Task<CourseModel?> GetByIdAsync(int id, string[] expands);

    Task<CourseModel> CreateAsync(CourseModel model);

    Task<CourseModel?> UpdateAsync(CourseModel model);

    Task<bool> DeleteAsync(int id);
}