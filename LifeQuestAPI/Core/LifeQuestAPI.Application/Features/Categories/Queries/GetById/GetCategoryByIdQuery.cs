using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Categories.Queries.GetById;

public sealed record GetCategoryByIdQueryRequest : IRequest<GetCategoryByIdQueryResponse?>
{
    public Guid Id { get; set; }
}

public sealed record GetCategoryByIdQueryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
}

public sealed class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQueryRequest, GetCategoryByIdQueryResponse?>
{
    private readonly ICategoryReadRepository _categoryReadRepository;

    public GetCategoryByIdQueryHandler(ICategoryReadRepository categoryReadRepository)
    {
        _categoryReadRepository = categoryReadRepository;
    }

    public async Task<GetCategoryByIdQueryResponse?> Handle(GetCategoryByIdQueryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryReadRepository.GetByIdAsync(request.Id.ToString(), tracking: false);
        if (category == null) return null;

        return new GetCategoryByIdQueryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconPath = category.IconPath
        };
    }
}
