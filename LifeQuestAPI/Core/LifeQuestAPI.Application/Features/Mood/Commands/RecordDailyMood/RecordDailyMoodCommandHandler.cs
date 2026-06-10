using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Mood.Commands.RecordDailyMood;

public class RecordDailyMoodCommandHandler : IRequestHandler<RecordDailyMoodCommandRequest, UserDailyMoodDto>
{
    private readonly IUserDailyMoodReadRepository _moodReadRepository;
    private readonly IUserDailyMoodWriteRepository _moodWriteRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;
    private readonly IUserTaskWriteRepository _userTaskWriteRepository;

    public RecordDailyMoodCommandHandler(
        IUserDailyMoodReadRepository moodReadRepository,
        IUserDailyMoodWriteRepository moodWriteRepository,
        IAppUserWriteRepository userWriteRepository,
        IAppUserReadRepository userReadRepository,
        IUserTaskReadRepository userTaskReadRepository,
        IUserTaskWriteRepository userTaskWriteRepository)
    {
        _moodReadRepository = moodReadRepository;
        _moodWriteRepository = moodWriteRepository;
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _userTaskReadRepository = userTaskReadRepository;
        _userTaskWriteRepository = userTaskWriteRepository;
    }

    public async Task<UserDailyMoodDto> Handle(RecordDailyMoodCommandRequest request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        
        var existing = await _moodReadRepository
            .GetWhere(m => m.AppUserId == request.UserId && m.Date == today, tracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
        {
            existing.MoodMode = request.MoodMode;
            existing.AvailableMinutes = request.AvailableMinutes;
            existing.EnergyLevel = request.EnergyLevel;
            existing.JournalEntry = request.JournalEntry;
            existing.SleepQuality = request.SleepQuality;
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
                JournalEntry = request.JournalEntry,
                SleepQuality = request.SleepQuality,
                RecordedAt = DateTime.UtcNow
            };
            await _moodWriteRepository.AddAsync(existing);
        }

        
        var suggestedTasks = await _userTaskReadRepository
            .GetWhere(ut => ut.AppUserId == request.UserId 
                && ut.Status == AppTaskStatus.Suggested
                && ut.DueDate.HasValue
                && DateOnly.FromDateTime(ut.DueDate.Value) == today, tracking: true)
            .ToListAsync(cancellationToken);

        foreach (var task in suggestedTasks)
        {
            _userTaskWriteRepository.Remove(task);
        }

        
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
            JournalEntry = existing.JournalEntry,
            SleepQuality = existing.SleepQuality,
            RecordedAt = existing.RecordedAt
        };
    }
}
