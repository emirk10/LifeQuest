using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommandRequest : IRequest<UpdateTaskCommandResponse>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public int BaseXP { get; set; }
    public int MinLevel { get; set; }
    public int Difficulty { get; set; }
    public bool IsActive { get; set; }
}

public sealed record UpdateTaskCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommandRequest, UpdateTaskCommandResponse>
{
    private readonly IAppTaskWriteRepository _taskWriteRepository;
    private readonly IAppTaskReadRepository _taskReadRepository;

    public UpdateTaskCommandHandler(IAppTaskWriteRepository taskWriteRepository, IAppTaskReadRepository taskReadRepository)
    {
        _taskWriteRepository = taskWriteRepository;
        _taskReadRepository = taskReadRepository;
    }

    public async Task<UpdateTaskCommandResponse> Handle(UpdateTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var task = await _taskReadRepository.GetByIdAsync(request.Id.ToString(), tracking: true);
        if (task == null)
        {
            return new UpdateTaskCommandResponse { IsSuccess = false, Message = "Görev bulunamadı." };
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.CategoryId = request.CategoryId;
        task.BaseXP = request.BaseXP;
        task.MinLevel = request.MinLevel;
        task.Difficulty = (DifficultyLevel)request.Difficulty;
        task.IsActive = request.IsActive;

        _taskWriteRepository.Update(task);
        await _taskWriteRepository.SaveAsync();

        return new UpdateTaskCommandResponse { IsSuccess = true, Message = "Görev başarıyla güncellendi." };
    }
}
