using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Mood.Commands.RecordDailyMood;

public class RecordDailyMoodCommandHandler : IRequestHandler<RecordDailyMoodCommandRequest, UserDailyMoodDto>
{
    private readonly IUserDailyMoodReadRepository _moodReadRepository;
    private readonly IUserDailyMoodWriteRepository _moodWriteRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;

    public RecordDailyMoodCommandHandler(
        IUserDailyMoodReadRepository moodReadRepository,
        IUserDailyMoodWriteRepository moodWriteRepository,
        IAppUserWriteRepository userWriteRepository,
        IAppUserReadRepository userReadRepository)
    {
        _moodReadRepository = moodReadRepository;
        _moodWriteRepository = moodWriteRepository;
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
    }

    public async Task<UserDailyMoodDto> Handle(RecordDailyMoodCommandRequest request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // UPSERT — bugün zaten kayıt varsa güncelle
        var existing = await _moodReadRepository
            .GetWhere(m => m.AppUserId == request.UserId && m.Date == today, tracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
        {
            existing.MoodMode = request.MoodMode;
            existing.AvailableMinutes = request.AvailableMinutes;
            existing.EnergyLevel = request.EnergyLevel;
            existing.RecordedAt = DateTime.UtcNow;
            _moodWriteRepository.Update(existing);
        }
        else
        {
            existing = new UserDailyMood
            {
                AppUserId = request.UserId,
                Date = today,
                MoodMode = request.MoodMode,
                AvailableMinutes = request.AvailableMinutes,
                EnergyLevel = request.EnergyLevel,
                RecordedAt = DateTime.UtcNow
            };
            await _moodWriteRepository.AddAsync(existing);
        }

        // Kullanıcının LastActiveAt alanını güncelle
        var user = await _userReadRepository.GetByIdAsync(request.UserId.ToString(), tracking: true);
        if (user != null)
        {
            user.LastActiveAt = DateTime.UtcNow;
            _userWriteRepository.Update(user);
        }

        await _moodWriteRepository.SaveAsync();

        return new UserDailyMoodDto
        {
            Id = existing.Id,
            AppUserId = existing.AppUserId,
            Date = existing.Date,
            MoodMode = existing.MoodMode,
            AvailableMinutes = existing.AvailableMinutes,
            EnergyLevel = existing.EnergyLevel,
            RecordedAt = existing.RecordedAt
        };
    }
}
