using FluentValidation;

namespace LifeQuestAPI.Application.Features.UserTasks.Commands.PostponeTask;

public class PostponeTaskCommandValidator : AbstractValidator<PostponeTaskCommandRequest>
{
    public PostponeTaskCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");

        RuleFor(x => x.UserTaskId)
            .NotEmpty().WithMessage("Görev kimliği belirtilmelidir.");

        RuleFor(x => x.PostponeTo)
            .Must(date => date > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Erteleme tarihi bugünden ileri bir tarih olmalıdır.");

        RuleFor(x => x.SkipReason)
            .MaximumLength(500).WithMessage("Erteleme nedeni en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.SkipReason));
    }
}
