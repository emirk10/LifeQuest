using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommandRequest : IRequest<UpdateCategoryCommandResponse>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
}

public sealed record UpdateCategoryCommandResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommandRequest, UpdateCategoryCommandResponse>
{
    private readonly ICategoryWriteRepository _categoryWriteRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;

    public UpdateCategoryCommandHandler(ICategoryWriteRepository categoryWriteRepository, ICategoryReadRepository categoryReadRepository)
    {
        _categoryWriteRepository = categoryWriteRepository;
        _categoryReadRepository = categoryReadRepository;
    }

    public async Task<UpdateCategoryCommandResponse> Handle(UpdateCategoryCommandRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryReadRepository.GetByIdAsync(request.Id.ToString(), tracking: true);
        if (category == null)
        {
            return new UpdateCategoryCommandResponse { IsSuccess = false, Message = "Kategori bulunamadı." };
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IconPath = request.IconPath;

        _categoryWriteRepository.Update(category);
        await _categoryWriteRepository.SaveAsync();

        return new UpdateCategoryCommandResponse { IsSuccess = true, Message = "Kategori başarıyla güncellendi." };
    }
}
