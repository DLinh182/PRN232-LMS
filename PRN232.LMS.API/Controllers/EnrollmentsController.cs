using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.RequestModels;
using PRN232.LMS.API.Models.ResponseModels;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
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

        var result = await _enrollmentService.GetPagedAsync(
            qp.Search,
            qp.Sort,
            qp.Page,
            qp.Size,
            qp.GetExpandList());

        var response = result.Items.Select(EnrollmentMapper.ToResponse).ToList();

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

    [HttpGet("{id}")]
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

        var enrollment = await _enrollmentService.GetByIdAsync(id, expands);

        if (enrollment == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Enrollment not found"
            });
        }

        var response = EnrollmentMapper.ToResponse(enrollment);

        object data = fieldList.Length == 0
            ? response
            : FieldSelector.ApplyFields(response, fieldList);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Get enrollment successfully",
            Data = data
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEnrollmentRequest request)
    {
        var model = new EnrollmentModel
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = request.Status
        };

        var created = await _enrollmentService.CreateAsync(model);
        var response = EnrollmentMapper.ToResponse(created);

        return StatusCode(201, new ApiResponse<EnrollmentResponse>
        {
            Success = true,
            Message = "Enrollment created successfully",
            Data = response
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEnrollmentRequest request)
    {
        var model = new EnrollmentModel
        {
            EnrollmentId = id,
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = request.Status
        };

        var updated = await _enrollmentService.UpdateAsync(model);

        if (updated == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Enrollment not found"
            });
        }

        var response = EnrollmentMapper.ToResponse(updated);

        return Ok(new ApiResponse<EnrollmentResponse>
        {
            Success = true,
            Message = "Enrollment updated successfully",
            Data = response
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _enrollmentService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Enrollment not found"
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Enrollment deleted successfully"
        });
    }
}