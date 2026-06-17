using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.RequestModels;
using PRN232.LMS.API.Models.ResponseModels;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters qp)
    {
        if (qp.Page < 1 || qp.Size < 1 || qp.Size > 100)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid page or size"
            });
        }

        var result = await _studentService.GetPagedAsync(
            qp.Search,
            qp.Sort,
            qp.Page,
            qp.Size,
            qp.GetExpandList(),
            qp.Email);

        var response = result.Items.Select(StudentMapper.ToResponse).ToList();

        var fields = qp.GetFieldList();
        var data = fields.Length == 0
            ? response.Cast<object>().ToList()
            : FieldSelector.ApplyFieldsList(response, fields);
        return Ok(new PagedApiResponse<List<object>>
        {
            Success = true,
            Message = "Request processed successfully",
            Data = data,
            Pagination = new PaginationMetadata
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromQuery] string? expand,
        [FromQuery] string? fields)
    {
        var expands = string.IsNullOrWhiteSpace(expand)
            ? Array.Empty<string>()
            : expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var fieldList = string.IsNullOrWhiteSpace(fields)
            ? Array.Empty<string>()
            : fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (expands.Length == 0)
        {
            expands = new[] { "enrollments", "course", "semester" };
        }

        var student = await _studentService.GetByIdAsync(id, expands);

        if (student == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Resource not found"
            });
        }

        var response = StudentMapper.ToResponse(student);

        object data = fieldList.Length == 0
            ? response
            : FieldSelector.ApplyFields(response, fieldList);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Get student successfully",
            Data = data
        });
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        CreateStudentRequest request)
    {
        var model = new StudentModel
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        var createdStudent = await _studentService.CreateAsync(model);

        var response = new StudentResponse
        {
            StudentId = createdStudent.StudentId,
            FullName = createdStudent.FullName,
            Email = createdStudent.Email,
            DateOfBirth = createdStudent.DateOfBirth
        };

        return StatusCode(201,
            new ApiResponse<StudentResponse>
            {
                Success = true,
                Message = "Student created successfully",
                Data = response
            });
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateStudentRequest request)
    {
        var model = new StudentModel
        {
            StudentId = id,
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        var updatedStudent =
            await _studentService.UpdateAsync(model);

        if (updatedStudent == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Student not found"
            });
        }

        var response = new StudentResponse
        {
            StudentId = updatedStudent.StudentId,
            FullName = updatedStudent.FullName,
            Email = updatedStudent.Email,
            DateOfBirth = updatedStudent.DateOfBirth
        };

        return Ok(new ApiResponse<StudentResponse>
        {
            Success = true,
            Message = "Student updated successfully",
            Data = response
        });
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _studentService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Student not found"
            });
        }

        return NoContent();
    }
}