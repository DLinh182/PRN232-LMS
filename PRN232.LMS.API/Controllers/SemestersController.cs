using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.RequestModels;
using PRN232.LMS.API.Models.ResponseModels;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
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

        var result = await _semesterService.GetPagedAsync(
            qp.Search,
            qp.Sort,
            qp.Page,
            qp.Size,
            qp.GetExpandList());

        var response = result.Items.Select(SemesterMapper.ToResponse).ToList();

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

        var semester = await _semesterService.GetByIdAsync(id, expands);

        if (semester == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Resource not found"
            });
        }

        var response = SemesterMapper.ToResponse(semester);

        object data = fieldList.Length == 0
            ? response
            : FieldSelector.ApplyFields(response, fieldList);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Get semester successfully",
            Data = data
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSemesterRequest request)
    {
        var model = new SemesterModel
        {
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var created = await _semesterService.CreateAsync(model);
        var response = SemesterMapper.ToResponse(created);

        return StatusCode(201, new ApiResponse<SemesterResponse>
        {
            Success = true,
            Message = "Semester created successfully",
            Data = response
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateSemesterRequest request)
    {
        var model = new SemesterModel
        {
            SemesterId = id,
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var updated = await _semesterService.UpdateAsync(model);

        if (updated == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Semester not found"
            });
        }

        var response = SemesterMapper.ToResponse(updated);

        return Ok(new ApiResponse<SemesterResponse>
        {
            Success = true,
            Message = "Semester updated successfully",
            Data = response
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _semesterService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Semester not found"
            });
        }

        return NoContent();
    }
}