using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappers;
using PRN232.LMS.API.Models.RequestModels;
using PRN232.LMS.API.Models.ResponseModels;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
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

        var result = await _subjectService.GetPagedAsync(
            qp.Search,
            qp.Sort,
            qp.Page,
            qp.Size,
            qp.GetExpandList());

        var response = result.Items.Select(SubjectMapper.ToResponse).ToList();

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

        var subject = await _subjectService.GetByIdAsync(id, expands);

        if (subject == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Subject not found"
            });
        }

        var response = SubjectMapper.ToResponse(subject);

        object data = fieldList.Length == 0
            ? response
            : FieldSelector.ApplyFields(response, fieldList);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Get subject successfully",
            Data = data
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSubjectRequest request)
    {
        var model = new SubjectModel
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };

        var created = await _subjectService.CreateAsync(model);
        var response = SubjectMapper.ToResponse(created);

        return StatusCode(201, new ApiResponse<SubjectResponse>
        {
            Success = true,
            Message = "Subject created successfully",
            Data = response
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateSubjectRequest request)
    {
        var model = new SubjectModel
        {
            SubjectId = id,
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };

        var updated = await _subjectService.UpdateAsync(model);

        if (updated == null)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Subject not found"
            });
        }

        var response = SubjectMapper.ToResponse(updated);

        return Ok(new ApiResponse<SubjectResponse>
        {
            Success = true,
            Message = "Subject updated successfully",
            Data = response
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _subjectService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Subject not found"
            });
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Subject deleted successfully"
        });
    }
}