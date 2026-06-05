using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommandRequest : IRequest<DeleteCategoryCommandResponse>
{
    public Guid Id { get; set; }
}

public sealed record DeleteCategoryCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommandRequest, DeleteCategoryCommandResponse>
{
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;

    public DeleteCategoryCommandHandler(ICategoryWriteRepository categoryWriteRepository, ICategoryReadRepository categoryReadRepository)
    {
        _categoryWriteRepository = categoryWriteRepository;
        _categoryReadRepository = categoryReadRepository;
    }

    public async Task<DeleteCategoryCommandResponse> Handle(DeleteCategoryCommandRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryReadRepository.GetByIdAsync(request.Id.ToString(), tracking: true);
        if (category == null)
        {
            return new DeleteCategoryCommandResponse { IsSuccess = false, Message = "Kategori bulunamadı." };
        }

        _categoryWriteRepository.Remove(category);
        await _categoryWriteRepository.SaveAsync();

        return new DeleteCategoryCommandResponse { IsSuccess = true, Message = "Kategori başarıyla silindi." };
    }
}
