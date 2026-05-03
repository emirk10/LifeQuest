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

    [HttpPost("create-category")]
    [Authorize]
    public async Task<IActionResult> CreateCategory(CreateCategoryCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }
    [HttpPost("create-bulk-categories")]
    [Authorize]
    public async Task<IActionResult> CreateBulkCategories([FromBody] CreateBulkCategoriesCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }
}