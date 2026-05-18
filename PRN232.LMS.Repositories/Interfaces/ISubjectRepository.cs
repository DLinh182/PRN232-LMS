using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISubjectRepository
{
    Task<(List<Subject> Items, int TotalItems)> GetPagedAsync(SubjectQuery query);
    Task<Subject?> GetByIdAsync(int id, string[] expands);
    Task<Subject> CreateAsync(Subject subject);
    Task<Subject?> UpdateAsync(Subject subject);
    Task<bool> DeleteAsync(int id);
}