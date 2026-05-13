using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Application.DTOs;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.User.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQueryRequest, GetUserProfileQueryResponse>
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IUserBadgeReadRepository _userBadgeReadRepository;
    private readonly IUserTaskReadRepository _userTaskReadRepository;

    public GetUserProfileQueryHandler(
        IAppUserReadRepository userReadRepository,
        IUserBadgeReadRepository userBadgeReadRepository,
        IUserTaskReadRepository userTaskReadRepository)
    {
        _userReadRepository = userReadRepository;
        _userBadgeReadRepository = userBadgeReadRepository;
        _userTaskReadRepository = userTaskReadRepository;
    }

    public async Task<GetUserProfileQueryResponse> Handle(GetUserProfileQueryRequest request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.Table
            .Include(u => u.CategoryProgresses)
            .ThenInclude(cp => cp.Category)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) throw new Exception("Kullanıcı bulunamadı.");

        var userBadges = await _userBadgeReadRepository
            .GetWhere(ub => ub.AppUserId == request.UserId, tracking: false)
            .Include(ub => ub.Badge)
            .ToListAsync(cancellationToken);

        // ─── Timezone Adjusted Lazy Reset Logic for UI ───────────────────────
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
        }
        catch (TimeZoneNotFoundException)
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
        }
        var nowInLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        var today = DateOnly.FromDateTime(nowInLocal);

        // Eğer bugün için henüz sıfırlama yapılmamışsa UI'da 0 göster
        var displayTodayTaskCount = (!user.LastCounterResetDate.HasValue || user.LastCounterResetDate.Value < today)
            ? 0
            : user.TodayTaskCount;

        var completedTaskCount = await _userTaskReadRepository
            .GetWhere(ut => ut.AppUserId == request.UserId && ut.CompletedAt.HasValue, tracking: false)
            .CountAsync(cancellationToken);

        return new GetUserProfileQueryResponse
        {
            Name = user.Name,
            Surname = user.Surname,
            UserName = user.UserName,
            TotalXP = user.TotalXP,
            GeneralLevel = user.GeneralLevel,
            CreatedAt = user.CreatedAt,
            CompletedTaskCount = completedTaskCount,
            DailyTaskLimit = user.DailyTaskLimit,
            TodayTaskCount = displayTodayTaskCount,
            ActiveTaskCount = user.ActiveTaskCount,

            EarnedBadges = userBadges.Select(ub => new UserBadgeDto
            {
                BadgeName = ub.Badge.Name,
                IconPath = ub.Badge.IconPath,
                Description = ub.Badge.Description,
                EarnedAt = ub.EarnedAt
            }).ToList(),

            CategoryStats = user.CategoryProgresses.Select(cp => new CategoryProgressDto
            {
                CategoryName = cp.Category.Name,
                IconPath = cp.Category.IconPath,
                Level = cp.Level,
                CurrentXp = cp.CurrentXp,
                XpToNextLevel = cp.XpToNextLevel
            }).ToList()
        };
    }
}