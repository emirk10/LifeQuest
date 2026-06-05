using LifeQuestAPI.Application.Features.Categories.Commands.CreateBulkCategories;
using LifeQuestAPI.Application.Features.Categories.Commands.CreateCategory;
using LifeQuestAPI.Application.Features.Categories.Queries.GetAllCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeQuestAPI.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _mediator.Send(new GetAllCategoriesQueryRequest());
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var response = await _mediator.Send(new LifeQuestAPI.Application.Features.Categories.Queries.GetById.GetCategoryByIdQueryRequest { Id = id });
        if (response == null) return NotFound(new { success = false, message = "Kategori bulunamadı." });
        return Ok(response);
    }

    [HttpPost("create-category")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory(CreateCategoryCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("create-bulk-categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBulkCategories([FromBody] CreateBulkCategoriesCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromBody] LifeQuestAPI.Application.Features.Categories.Commands.UpdateCategory.UpdateCategoryCommandRequest request)
    {
        if (id != request.Id) return BadRequest(new { success = false, message = "ID eşleşmiyor." });
        var response = await _mediator.Send(request);
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
    {
        var response = await _mediator.Send(new LifeQuestAPI.Application.Features.Categories.Commands.DeleteCategory.DeleteCategoryCommandRequest { Id = id });
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }
}