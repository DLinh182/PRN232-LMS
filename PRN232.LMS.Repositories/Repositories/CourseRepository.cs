using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly LmsDbContext _context;

    public CourseRepository(LmsDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Course> Items, int TotalItems)> GetPagedAsync(CourseQuery query)
    {
        IQueryable<Course> q = _context.Courses;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            q = q.Where(x => x.CourseName.ToLower().Contains(keyword));
        }

        q = ApplyExpands(q, query.Expands);
        q = ApplySort(q, query.Sort);

        var totalItems = await q.CountAsync();

        var offset = (query.Page - 1) * query.Size;

        var items = await q
            .Skip(offset)
            .Take(query.Size)
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<Course?> GetByIdAsync(int id, string[] expands)
    {
        IQueryable<Course> q = _context.Courses;
        q = ApplyExpands(q, expands);
        return await q.FirstOrDefaultAsync(x => x.CourseId == id);
    }

    public async Task<Course> CreateAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<Course?> UpdateAsync(Course course)
    {
        var existing = await _context.Courses
            .FirstOrDefaultAsync(x => x.CourseId == course.CourseId);

        if (existing == null)
        {
            return null;
        }

        existing.CourseName = course.CourseName;
        existing.SemesterId = course.SemesterId;
        existing.SubjectId = course.SubjectId;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.CourseId == id);

        if (course == null)
        {
            return false;
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return true;
    }

    private static IQueryable<Course> ApplyExpands(IQueryable<Course> query, string[] expands)
    {
        if (expands.Length == 0)
        {
            return query;
        }

        var set = expands.Select(x => x.Trim().ToLower()).ToHashSet();

        if (set.Contains("semester"))
        {
            query = query.Include(x => x.Semester);
        }

        if (set.Contains("subject"))
        {
            query = query.Include(x => x.Subject);
        }
        if (set.Contains("enrollments"))
        {
            query = query.Include(x => x.Enrollments)
                         .ThenInclude(e => e.Student);
        }

        return query;
    }

    private static IQueryable<Course> ApplySort(IQueryable<Course> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(x => x.CourseId);
        }

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<Course>? ordered = null;

        foreach (var field in sortFields)
        {
            var desc = field.StartsWith('-');
            var name = (desc ? field[1..] : field).ToLower();

            ordered = (ordered, name) switch
            {
                (null, "courseid") => desc
                    ? query.OrderByDescending(x => x.CourseId)
                    : query.OrderBy(x => x.CourseId),

                (null, "coursename") => desc
                    ? query.OrderByDescending(x => x.CourseName)
                    : query.OrderBy(x => x.CourseName),

                (null, "semesterid") => desc
                    ? query.OrderByDescending(x => x.SemesterId)
                    : query.OrderBy(x => x.SemesterId),

                (null, "subjectid") => desc
                    ? query.OrderByDescending(x => x.SubjectId)
                    : query.OrderBy(x => x.SubjectId),

                (var o, "courseid") => desc
                    ? o!.ThenByDescending(x => x.CourseId)
                    : o!.ThenBy(x => x.CourseId),

                (var o, "coursename") => desc
                    ? o!.ThenByDescending(x => x.CourseName)
                    : o!.ThenBy(x => x.CourseName),

                (var o, "semesterid") => desc
                    ? o!.ThenByDescending(x => x.SemesterId)
                    : o!.ThenBy(x => x.SemesterId),

                (var o, "subjectid") => desc
                    ? o!.ThenByDescending(x => x.SubjectId)
                    : o!.ThenBy(x => x.SubjectId),

                _ => ordered
            };

            if (ordered != null)
            {
                query = ordered;
            }
        }

        return ordered ?? query.OrderBy(x => x.CourseId);
    }
}