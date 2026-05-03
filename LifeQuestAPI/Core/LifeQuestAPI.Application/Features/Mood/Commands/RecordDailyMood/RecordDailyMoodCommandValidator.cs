using FluentValidation;

namespace LifeQuestAPI.Application.Features.Mood.Commands.RecordDailyMood;

public class RecordDailyMoodCommandValidator : AbstractValidator<RecordDailyMoodCommandRequest>
{
    public RecordDailyMoodCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği belirtilmelidir.");

        RuleFor(x => x.EnergyLevel)
            .InclusiveBetween(1, 5).WithMessage("Enerji seviyesi 1 ile 5 arasında olmalıdır.");

        RuleFor(x => x.AvailableMinutes)
            .InclusiveBetween(5, 480).WithMessage("Müsait süre 5 ile 480 dakika arasında olmalıdır.");
    }
}
