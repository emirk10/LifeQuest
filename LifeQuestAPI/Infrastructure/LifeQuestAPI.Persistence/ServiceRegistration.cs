using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Persistence.Contexts;
using LifeQuestAPI.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LifeQuestAPI.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LifeQuestDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions => 
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            }));

        services.AddScoped<IAppUserReadRepository, AppUserReadRepository>();
        services.AddScoped<IAppUserWriteRepository, AppUserWriteRepository>();
        services.AddScoped<IAppRoleReadRepository, AppRoleReadRepository>();
        services.AddScoped<IAppRoleWriteRepository, AppRoleWriteRepository>();
        services.AddScoped<IAppTaskReadRepository, AppTaskReadRepository>();
        services.AddScoped<IAppTaskWriteRepository, AppTaskWriteRepository>();
        services.AddScoped<IUserTaskReadRepository, UserTaskReadRepository>();
        services.AddScoped<IUserTaskWriteRepository, UserTaskWriteRepository>();
        services.AddScoped<IBadgeReadRepository, BadgeReadRepository>();
        services.AddScoped<IBadgeWriteRepository, BadgeWriteRepository>();
        services.AddScoped<IUserBadgeReadRepository, UserBadgeReadRepository>();
        services.AddScoped<IUserBadgeWriteRepository, UserBadgeWriteRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<ICategoryWriteRepository, CategoryWriteRepository>();
        services.AddScoped<IUserCategoryProgressReadRepository, UserCategoryProgressReadRepository>();
        services.AddScoped<IUserCategoryProgressWriteRepository, UserCategoryProgressWriteRepository>();
        services.AddScoped<IUserGoalReadRepository, UserGoalReadRepository>();
        services.AddScoped<IUserGoalWriteRepository, UserGoalWriteRepository>();
        services.AddScoped<IAchievementReadRepository, AchievementReadRepository>();
        services.AddScoped<IAchievementWriteRepository, AchievementWriteRepository>();
        services.AddScoped<IUserAchievementReadRepository, UserAchievementReadRepository>();
        services.AddScoped<IUserAchievementWriteRepository, UserAchievementWriteRepository>();
        services.AddScoped<INotificationReadRepository, NotificationReadRepository>();
        services.AddScoped<INotificationWriteRepository, NotificationWriteRepository>();
        services.AddScoped<IWeeklyReportReadRepository, WeeklyReportReadRepository>();
        services.AddScoped<IWeeklyReportWriteRepository, WeeklyReportWriteRepository>();
        services.AddScoped<IFriendshipReadRepository, FriendshipReadRepository>();
        services.AddScoped<IFriendshipWriteRepository, FriendshipWriteRepository>();
        services.AddScoped<IChallengeReadRepository, ChallengeReadRepository>();
        services.AddScoped<IChallengeWriteRepository, ChallengeWriteRepository>();
        services.AddScoped<IUserTaskInteractionReadRepository, UserTaskInteractionReadRepository>();
        services.AddScoped<IUserTaskInteractionWriteRepository, UserTaskInteractionWriteRepository>();
    }
}