using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<(List<Student> Items, int TotalItems)> GetPagedAsync(StudentQuery query);

    Task<Student?> GetByIdAsync(int id, string[] expands);

    Task<Student> CreateAsync(Student student);
    Task<Student?> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
}