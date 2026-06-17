using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.RequestModels;
using PRN232.LMS.API.Models.ResponseModels;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
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

        var result = await _courseService.GetPagedAsync(
            qp.Search,
            qp.Sort,
            qp.Page,
            qp.Size,
            qp.GetExpandList(),
            qp.SemesterId);

        var response = result.Items.Select(CourseMapper.ToResponse).ToList();

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

        var course = await _courseService.GetByIdAsync(id, expands);

        if (course == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Resource not found"
            });
        }

        var response = CourseMapper.ToResponse(course);

        object data = fieldList.Length == 0
            ? response
            : FieldSelector.ApplyFields(response, fieldList);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Get course successfully",
            Data = data
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCourseRequest request)
    {
        var model = new CourseModel
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId ?? 1
        };

        var created = await _courseService.CreateAsync(model);
        var response = CourseMapper.ToResponse(created);

        return StatusCode(201, new ApiResponse<CourseResponse>
        {
            Success = true,
            Message = "Course created successfully",
            Data = response
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCourseRequest request)
    {
        var model = new CourseModel
        {
            CourseId = id,
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId ?? 1
        };

        var updated = await _courseService.UpdateAsync(model);

        if (updated == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Course not found"
            });
        }

        var response = CourseMapper.ToResponse(updated);

        return Ok(new ApiResponse<CourseResponse>
        {
            Success = true,
            Message = "Course updated successfully",
            Data = response
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _courseService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Course not found"
            });
        }

        return NoContent();
    }
}