using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISemesterRepository
{
    Task<(List<Semester> Items, int TotalItems)> GetPagedAsync(SemesterQuery query);
    Task<Semester?> GetByIdAsync(int id, string[] expands);
    Task<Semester> CreateAsync(Semester semester);
    Task<Semester?> UpdateAsync(Semester semester);
    Task<bool> DeleteAsync(int id);
}