using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<(List<Course> Items, int TotalItems)> GetPagedAsync(CourseQuery query);
    Task<Course?> GetByIdAsync(int id, string[] expands);
    Task<Course> CreateAsync(Course course);
    Task<Course?> UpdateAsync(Course course);
    Task<bool> DeleteAsync(int id);
}