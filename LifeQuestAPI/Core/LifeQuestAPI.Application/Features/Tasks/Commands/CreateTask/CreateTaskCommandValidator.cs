using FluentValidation;

namespace LifeQuestAPI.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommandRequest>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Görev başlığı boş olamaz.")
            .MaximumLength(200).WithMessage("Görev başlığı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Görev açıklaması en fazla 1000 karakter olabilir.");

        RuleFor(x => x.BaseXP)
            .GreaterThan(0).WithMessage("BaseXP 0'dan büyük olmalıdır.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seçilmelidir.");

        RuleFor(x => x.MinLevel)
            .GreaterThanOrEqualTo(1).WithMessage("MinLevel en az 1 olmalıdır.");
    }
}
