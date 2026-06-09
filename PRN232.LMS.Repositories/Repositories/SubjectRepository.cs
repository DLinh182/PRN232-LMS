using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories;

public class SubjectRepository : ISubjectRepository
{
    private readonly LmsDbContext _context;

    public SubjectRepository(LmsDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Subject> Items, int TotalItems)> GetPagedAsync(SubjectQuery query)
    {
        IQueryable<Subject> q = _context.Subjects;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            q = q.Where(x =>
                x.SubjectCode.ToLower().Contains(keyword) ||
                x.SubjectName.ToLower().Contains(keyword));
        }

        if (query.Credit.HasValue)
        {
            q = q.Where(x => x.Credit == query.Credit.Value);
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

    public async Task<Subject?> GetByIdAsync(int id, string[] expands)
    {
        IQueryable<Subject> q = _context.Subjects;
        q = ApplyExpands(q, expands);
        return await q.FirstOrDefaultAsync(x => x.SubjectId == id);
    }

    public async Task<Subject> CreateAsync(Subject subject)
    {
        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();
        return subject;
    }

    public async Task<Subject?> UpdateAsync(Subject subject)
    {
        var existing = await _context.Subjects
            .FirstOrDefaultAsync(x => x.SubjectId == subject.SubjectId);

        if (existing == null)
        {
            return null;
        }

        existing.SubjectCode = subject.SubjectCode;
        existing.SubjectName = subject.SubjectName;
        existing.Credit = subject.Credit;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(x => x.SubjectId == id);

        if (subject == null)
        {
            return false;
        }

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();
        return true;
    }

    private static IQueryable<Subject> ApplyExpands(IQueryable<Subject> query, string[] expands)
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

    private static IQueryable<Subject> ApplySort(IQueryable<Subject> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(x => x.SubjectId);
        }

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<Subject>? ordered = null;

        foreach (var field in sortFields)
        {
            var desc = field.StartsWith('-');
            var name = (desc ? field[1..] : field).ToLower();

            ordered = (ordered, name) switch
            {
                (null, "subjectid") => desc
                    ? query.OrderByDescending(x => x.SubjectId)
                    : query.OrderBy(x => x.SubjectId),

                (null, "subjectcode") => desc
                    ? query.OrderByDescending(x => x.SubjectCode)
                    : query.OrderBy(x => x.SubjectCode),

                (null, "subjectname") => desc
                    ? query.OrderByDescending(x => x.SubjectName)
                    : query.OrderBy(x => x.SubjectName),

                (null, "credit") => desc
                    ? query.OrderByDescending(x => x.Credit)
                    : query.OrderBy(x => x.Credit),

                (var o, "subjectid") => desc
                    ? o!.ThenByDescending(x => x.SubjectId)
                    : o!.ThenBy(x => x.SubjectId),

                (var o, "subjectcode") => desc
                    ? o!.ThenByDescending(x => x.SubjectCode)
                    : o!.ThenBy(x => x.SubjectCode),

                (var o, "subjectname") => desc
                    ? o!.ThenByDescending(x => x.SubjectName)
                    : o!.ThenBy(x => x.SubjectName),

                (var o, "credit") => desc
                    ? o!.ThenByDescending(x => x.Credit)
                    : o!.ThenBy(x => x.Credit),

                _ => ordered
            };

            if (ordered != null)
            {
                query = ordered;
            }
        }

        return ordered ?? query.OrderBy(x => x.SubjectId);
    }
}