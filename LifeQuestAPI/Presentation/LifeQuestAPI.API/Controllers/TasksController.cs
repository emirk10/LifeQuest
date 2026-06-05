using LifeQuestAPI.Application.Features.Tasks.Commands.CreateBulkTasks;
using LifeQuestAPI.Application.Features.Tasks.Commands.CreateTask;
using LifeQuestAPI.Application.Features.Tasks.Queries.GetAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeQuestAPI.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("getall")]
    public async Task<IActionResult> GetAllTasks()
    {
        var response = await _mediator.Send(new GetAllTasksQueryRequest());
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById([FromRoute] Guid id)
    {
        var response = await _mediator.Send(new LifeQuestAPI.Application.Features.Tasks.Queries.GetById.GetTaskByIdQueryRequest { Id = id });
        if (response == null) return NotFound(new { success = false, message = "Görev bulunamadı." });
        return Ok(response);
    }

    [HttpPost("create-task")]
    public async Task<IActionResult> CreateTask(CreateTaskCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("create-bulk-tasks")]
    public async Task<IActionResult> CreateBulkTasks(CreateBulkTasksCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask([FromRoute] Guid id, [FromBody] LifeQuestAPI.Application.Features.Tasks.Commands.UpdateTask.UpdateTaskCommandRequest request)
    {
        if (id != request.Id) return BadRequest(new { success = false, message = "ID eşleşmiyor." });
        var response = await _mediator.Send(request);
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid id)
    {
        var response = await _mediator.Send(new LifeQuestAPI.Application.Features.Tasks.Commands.DeleteTask.DeleteTaskCommandRequest { Id = id });
        if (!response.IsSuccess) return NotFound(response);
        return Ok(response);
    }
}