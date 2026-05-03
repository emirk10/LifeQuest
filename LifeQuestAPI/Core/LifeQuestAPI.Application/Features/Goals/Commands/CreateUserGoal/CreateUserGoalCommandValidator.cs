using FluentValidation;

namespace LifeQuestAPI.Application.Features.Goals.Commands.CreateUserGoal;

public class CreateUserGoalCommandValidator : AbstractValidator<CreateUserGoalCommandRequest>
{
    public CreateUserGoalCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Hedef başlığı boş olamaz.")
            .MaximumLength(150).WithMessage("Hedef başlığı en fazla 150 karakter olabilir.");

        RuleFor(x => x.TargetValue)
            .GreaterThan(0).WithMessage("Hedef değeri 0'dan büyük olmalıdır.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi belirtilmelidir.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi belirtilmelidir.")
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.BonusXP)
            .GreaterThanOrEqualTo(0).WithMessage("Bonus XP negatif olamaz.");
    }
}
