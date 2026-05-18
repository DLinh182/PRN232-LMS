using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Common;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<PagedResult<EnrollmentModel>> GetPagedAsync(
        string? search,
        string? sort,
        int page,
        int size,
        string[] expands);

    Task<EnrollmentModel?> GetByIdAsync(int id, string[] expands);

    Task<EnrollmentModel> CreateAsync(EnrollmentModel model);

    Task<EnrollmentModel?> UpdateAsync(EnrollmentModel model);

    Task<bool> DeleteAsync(int id);
}