using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<PagedResult<StudentModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands);

    Task<StudentModel?> GetByIdAsync(int id, string[] expands);

    Task<StudentModel> CreateAsync(StudentModel model);

    Task<StudentModel?> UpdateAsync(StudentModel model);

    Task<bool> DeleteAsync(int id);
}