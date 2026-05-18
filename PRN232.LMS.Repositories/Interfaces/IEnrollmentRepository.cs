using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<(List<Enrollment> Items, int TotalItems)> GetPagedAsync(EnrollmentQuery query);
    Task<Enrollment?> GetByIdAsync(int id, string[] expands);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<Enrollment?> UpdateAsync(Enrollment enrollment);
    Task<bool> DeleteAsync(int id);
}