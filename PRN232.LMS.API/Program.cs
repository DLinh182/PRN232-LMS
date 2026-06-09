using System;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PRN232.LMS.API.Common;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Repositories.Repositories;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error =>
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Invalid value."
                        : error.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new ApiResponse<object>
        {
            Success = false,
            Message = "Bad request",
            Data = null,
            Errors = errors
        });
    };
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PRN232 LMS API",
        Version = "v1",
        Description = "Learning Management System RESTful API for PRN232 Lab 1"
    });
});

builder.Services.AddDbContext<LmsDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        });
});

builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

    var retry = 5;
    var delay = TimeSpan.FromSeconds(2);

    while (true)
    {
        try
        {
            db.Database.Migrate();
            await SeedDatabaseAsync(db);
            break;
        }
        catch (Exception ex)
        {
            retry--;
            Console.WriteLine("Waiting DB... " + ex.Message);

            if (retry == 0) throw;

            Thread.Sleep(delay);
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")))
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var isDevelopment = app.Environment.IsDevelopment();

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "Internal server error",
            Errors = new
            {
                code = "INTERNAL_SERVER_ERROR",
                detail = isDevelopment ? exception?.Message : null
            }
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    });
});

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedDatabaseAsync(LmsDbContext db)
{
    if (await db.Students.AnyAsync())
    {
        return;
    }

    var semesters = new List<Semester>
    {
        new() { SemesterName = "Spring 2025", StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 4, 30) },
        new() { SemesterName = "Summer 2025", StartDate = new DateTime(2025, 5, 1), EndDate = new DateTime(2025, 8, 31) },
        new() { SemesterName = "Fall 2025", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2025, 12, 31) },
        new() { SemesterName = "Spring 2026", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 4, 30) },
        new() { SemesterName = "Summer 2026", StartDate = new DateTime(2026, 5, 1), EndDate = new DateTime(2026, 8, 31) }
    };

    db.Semesters.AddRange(semesters);

    var subjects = Enumerable.Range(1, 10)
        .Select(i => new Subject
        {
            SubjectCode = $"SUB{i:00}",
            SubjectName = $"Subject {i}",
            Credit = 3 + (i % 2)
        })
        .ToList();

    db.Subjects.AddRange(subjects);

    var students = Enumerable.Range(1, 50)
        .Select(i => new Student
        {
            FullName = $"Nguyen Van {i:00}",
            Email = $"student{i:00}@lms.local",
            DateOfBirth = new DateTime(2000, 1, 1).AddDays(i)
        })
        .ToList();

    db.Students.AddRange(students);

    await db.SaveChangesAsync();

    var courses = Enumerable.Range(1, 20)
        .Select(i => new Course
        {
            CourseName = $"Course {i:00}",
            SemesterId = semesters[(i - 1) % semesters.Count].SemesterId,
            SubjectId = subjects[(i - 1) % subjects.Count].SubjectId
        })
        .ToList();

    db.Courses.AddRange(courses);

    await db.SaveChangesAsync();

    var enrollments = Enumerable.Range(1, 500)
        .Select(i => new Enrollment
        {
            StudentId = students[(i - 1) % students.Count].StudentId,
            CourseId = courses[(i - 1) % courses.Count].CourseId,
            EnrollDate = new DateTime(2025, 1, 1).AddDays(i % 120),
            Status = i % 5 == 0 ? "inactive" : "active"
        })
        .ToList();

    db.Enrollments.AddRange(enrollments);
    await db.SaveChangesAsync();
}