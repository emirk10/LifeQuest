using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommandRequest : IRequest<DeleteTaskCommandResponse>
{
    public Guid Id { get; set; }
}

public sealed record DeleteTaskCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommandRequest, DeleteTaskCommandResponse>
{
    private readonly IAppTaskWriteRepository _taskWriteRepository;
    private readonly IAppTaskReadRepository _taskReadRepository;

    public DeleteTaskCommandHandler(IAppTaskWriteRepository taskWriteRepository, IAppTaskReadRepository taskReadRepository)
    {
        _taskWriteRepository = taskWriteRepository;
        _taskReadRepository = taskReadRepository;
    }

    public async Task<DeleteTaskCommandResponse> Handle(DeleteTaskCommandRequest request, CancellationToken cancellationToken)
    {
        var task = await _taskReadRepository.GetByIdAsync(request.Id.ToString(), tracking: true);
        if (task == null)
        {
            return new DeleteTaskCommandResponse { IsSuccess = false, Message = "Görev bulunamadı." };
        }

        _taskWriteRepository.Remove(task);
        await _taskWriteRepository.SaveAsync();

        return new DeleteTaskCommandResponse { IsSuccess = true, Message = "Görev başarıyla silindi." };
    }
}
