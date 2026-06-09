using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly LmsDbContext _context;

    public EnrollmentRepository(LmsDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Enrollment> Items, int TotalItems)> GetPagedAsync(EnrollmentQuery query)
    {
        IQueryable<Enrollment> q = _context.Enrollments;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            q = q.Where(x => x.Status.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLower();
            q = q.Where(x => x.Status.ToLower() == status);
        }

        if (query.StudentId.HasValue)
        {
            q = q.Where(x => x.StudentId == query.StudentId.Value);
        }

        if (query.CourseId.HasValue)
        {
            q = q.Where(x => x.CourseId == query.CourseId.Value);
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

    public async Task<Enrollment?> GetByIdAsync(int id, string[] expands)
    {
        IQueryable<Enrollment> q = _context.Enrollments;
        q = ApplyExpands(q, expands);
        return await q.FirstOrDefaultAsync(x => x.EnrollmentId == id);
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<Enrollment?> UpdateAsync(Enrollment enrollment)
    {
        var existing = await _context.Enrollments
            .FirstOrDefaultAsync(x => x.EnrollmentId == enrollment.EnrollmentId);

        if (existing == null)
        {
            return null;
        }

        existing.StudentId = enrollment.StudentId;
        existing.CourseId = enrollment.CourseId;
        existing.EnrollDate = enrollment.EnrollDate;
        existing.Status = enrollment.Status;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(x => x.EnrollmentId == id);

        if (enrollment == null)
        {
            return false;
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

    private static IQueryable<Enrollment> ApplyExpands(IQueryable<Enrollment> query, string[] expands)
    {
        if (expands.Length == 0)
        {
            return query;
        }

        var set = expands
            .Select(x => x.Trim().ToLower())
            .ToHashSet();

        if (set.Contains("student"))
        {
            query = query.Include(x => x.Student);
        }

        if (set.Contains("course"))
        {
            query = query.Include(x => x.Course);
        }

        return query;
    }

    private static IQueryable<Enrollment> ApplySort(IQueryable<Enrollment> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(x => x.EnrollmentId);
        }

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<Enrollment>? ordered = null;

        foreach (var field in sortFields)
        {
            var desc = field.StartsWith('-');
            var name = (desc ? field[1..] : field).ToLower();

            ordered = (ordered, name) switch
            {
                (null, "enrollmentid") => desc
                    ? query.OrderByDescending(x => x.EnrollmentId)
                    : query.OrderBy(x => x.EnrollmentId),

                (null, "studentid") => desc
                    ? query.OrderByDescending(x => x.StudentId)
                    : query.OrderBy(x => x.StudentId),

                (null, "courseid") => desc
                    ? query.OrderByDescending(x => x.CourseId)
                    : query.OrderBy(x => x.CourseId),

                (null, "enrolldate") => desc
                    ? query.OrderByDescending(x => x.EnrollDate)
                    : query.OrderBy(x => x.EnrollDate),

                (null, "status") => desc
                    ? query.OrderByDescending(x => x.Status)
                    : query.OrderBy(x => x.Status),

                (var o, "enrollmentid") => desc
                    ? o!.ThenByDescending(x => x.EnrollmentId)
                    : o!.ThenBy(x => x.EnrollmentId),

                (var o, "studentid") => desc
                    ? o!.ThenByDescending(x => x.StudentId)
                    : o!.ThenBy(x => x.StudentId),

                (var o, "courseid") => desc
                    ? o!.ThenByDescending(x => x.CourseId)
                    : o!.ThenBy(x => x.CourseId),

                (var o, "enrolldate") => desc
                    ? o!.ThenByDescending(x => x.EnrollDate)
                    : o!.ThenBy(x => x.EnrollDate),

                (var o, "status") => desc
                    ? o!.ThenByDescending(x => x.Status)
                    : o!.ThenBy(x => x.Status),

                _ => ordered
            };

            if (ordered != null)
            {
                query = ordered;
            }
        }

        return ordered ?? query.OrderBy(x => x.EnrollmentId);
    }
}
