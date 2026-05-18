using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories;

public class SemesterRepository : ISemesterRepository
{
    private readonly LmsDbContext _context;

    public SemesterRepository(LmsDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Semester> Items, int TotalItems)> GetPagedAsync(SemesterQuery query)
    {
        IQueryable<Semester> q = _context.Semesters;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            q = q.Where(x => x.SemesterName.ToLower().Contains(keyword));
        }

        q = ApplyExpands(q, query.Expands);
        q = ApplySort(q, query.Sort);

        var totalItems = await q.CountAsync();

        var items = await q
            .Skip(query.Skip)
            .Take(query.Size)
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<Semester?> GetByIdAsync(int id, string[] expands)
    {
        IQueryable<Semester> q = _context.Semesters;
        q = ApplyExpands(q, expands);
        return await q.FirstOrDefaultAsync(x => x.SemesterId == id);
    }

    public async Task<Semester> CreateAsync(Semester semester)
    {
        _context.Semesters.Add(semester);
        await _context.SaveChangesAsync();
        return semester;
    }

    public async Task<Semester?> UpdateAsync(Semester semester)
    {
        var existing = await _context.Semesters
            .FirstOrDefaultAsync(x => x.SemesterId == semester.SemesterId);

        if (existing == null)
        {
            return null;
        }

        existing.SemesterName = semester.SemesterName;
        existing.StartDate = semester.StartDate;
        existing.EndDate = semester.EndDate;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var semester = await _context.Semesters
            .FirstOrDefaultAsync(x => x.SemesterId == id);

        if (semester == null)
        {
            return false;
        }

        _context.Semesters.Remove(semester);
        await _context.SaveChangesAsync();
        return true;
    }

    private static IQueryable<Semester> ApplyExpands(IQueryable<Semester> query, string[] expands)
    {
        if (expands.Length == 0)
        {
            return query;
        }

        var set = expands.Select(x => x.Trim().ToLower()).ToHashSet();

        if (set.Contains("courses"))
        {
            query = query.Include(x => x.Courses);
        }

        return query;
    }

    private static IQueryable<Semester> ApplySort(IQueryable<Semester> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(x => x.SemesterId);
        }

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<Semester>? ordered = null;

        foreach (var field in sortFields)
        {
            var desc = field.StartsWith('-');
            var name = (desc ? field[1..] : field).ToLower();

            ordered = (ordered, name) switch
            {
                (null, "semesterid") => desc
                    ? query.OrderByDescending(x => x.SemesterId)
                    : query.OrderBy(x => x.SemesterId),

                (null, "semestername") => desc
                    ? query.OrderByDescending(x => x.SemesterName)
                    : query.OrderBy(x => x.SemesterName),

                (null, "startdate") => desc
                    ? query.OrderByDescending(x => x.StartDate)
                    : query.OrderBy(x => x.StartDate),

                (null, "enddate") => desc
                    ? query.OrderByDescending(x => x.EndDate)
                    : query.OrderBy(x => x.EndDate),

                (var o, "semesterid") => desc
                    ? o!.ThenByDescending(x => x.SemesterId)
                    : o!.ThenBy(x => x.SemesterId),

                (var o, "semestername") => desc
                    ? o!.ThenByDescending(x => x.SemesterName)
                    : o!.ThenBy(x => x.SemesterName),

                (var o, "startdate") => desc
                    ? o!.ThenByDescending(x => x.StartDate)
                    : o!.ThenBy(x => x.StartDate),

                (var o, "enddate") => desc
                    ? o!.ThenByDescending(x => x.EndDate)
                    : o!.ThenBy(x => x.EndDate),

                _ => ordered
            };

            if (ordered != null)
            {
                query = ordered;
            }
        }

        return ordered ?? query.OrderBy(x => x.SemesterId);
    }
}