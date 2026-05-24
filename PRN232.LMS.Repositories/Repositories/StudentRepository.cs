using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly LmsDbContext _context;

    public StudentRepository(LmsDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Student> Items, int TotalItems)> GetPagedAsync(StudentQuery query)
    {
        IQueryable<Student> q = _context.Students;

        // SEARCH
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            q = q.Where(x =>
                x.FullName.ToLower().Contains(keyword) ||
                x.Email.ToLower().Contains(keyword));
        }

        // EXPAND (trước khi sort/paging)
        q = ApplyExpands(q, query.Expands);

        // SORT
        q = ApplySort(q, query.Sort);

        var totalItems = await q.CountAsync();

        var offset = (query.Page - 1) * query.Size;

        var items = await q
            .Skip(offset)
            .Take(query.Size)
            .ToListAsync();

        return (items, totalItems);
    }

    public async Task<Student?> GetByIdAsync(int id, string[] expands)
    {
        IQueryable<Student> q = _context.Students;

        q = ApplyExpands(q, expands);

        return await q.FirstOrDefaultAsync(x => x.StudentId == id);
    }

    public async Task<Student> CreateAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task<Student?> UpdateAsync(Student student)
    {
        var existingStudent = await _context.Students
            .FirstOrDefaultAsync(x => x.StudentId == student.StudentId);

        if (existingStudent == null)
        {
            return null;
        }

        existingStudent.FullName = student.FullName;
        existingStudent.Email = student.Email;
        existingStudent.DateOfBirth = student.DateOfBirth;

        await _context.SaveChangesAsync();

        return existingStudent;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(x => x.StudentId == id);

        if (student == null)
        {
            return false;
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return true;
    }

    private static IQueryable<Student> ApplyExpands(IQueryable<Student> query, string[] expands)
    {
        if (expands.Length == 0)
        {
            return query;
        }

        var set = expands
            .Select(x => x.Trim().ToLower())
            .ToHashSet();

        if (set.Contains("enrollments") || set.Contains("course"))
        {
            query = query.Include(x => x.Enrollments);
        }

        // GET /students?expand=enrollments,course OR /students?expand=course
        if (set.Contains("course"))
        {
            query = query
                .Include(x => x.Enrollments)
                .ThenInclude(e => e.Course);
        }

        return query;
    }

    private static IQueryable<Student> ApplySort(IQueryable<Student> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(x => x.StudentId);
        }

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<Student>? ordered = null;

        foreach (var field in sortFields)
        {
            var desc = field.StartsWith('-');
            var name = (desc ? field[1..] : field).ToLower();

            ordered = (ordered, name) switch
            {
                (null, "fullname") => desc
                    ? query.OrderByDescending(x => x.FullName)
                    : query.OrderBy(x => x.FullName),

                (null, "email") => desc
                    ? query.OrderByDescending(x => x.Email)
                    : query.OrderBy(x => x.Email),

                (null, "dateofbirth") => desc
                    ? query.OrderByDescending(x => x.DateOfBirth)
                    : query.OrderBy(x => x.DateOfBirth),

                (null, "studentid") => desc
                    ? query.OrderByDescending(x => x.StudentId)
                    : query.OrderBy(x => x.StudentId),

                (var o, "fullname") => desc
                    ? o!.ThenByDescending(x => x.FullName)
                    : o!.ThenBy(x => x.FullName),

                (var o, "email") => desc
                    ? o!.ThenByDescending(x => x.Email)
                    : o!.ThenBy(x => x.Email),

                (var o, "dateofbirth") => desc
                    ? o!.ThenByDescending(x => x.DateOfBirth)
                    : o!.ThenBy(x => x.DateOfBirth),

                (var o, "studentid") => desc
                    ? o!.ThenByDescending(x => x.StudentId)
                    : o!.ThenBy(x => x.StudentId),

                _ => ordered
            };

            if (ordered != null)
            {
                query = ordered;
            }
        }

        return ordered ?? query.OrderBy(x => x.StudentId);
    }
}