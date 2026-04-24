using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Application.Abstractions.DailyQuest;
using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Social;
using LifeQuestAPI.Application.Abstractions.Token;
using LifeQuestAPI.Infrastructure.Services.AI;
using LifeQuestAPI.Infrastructure.Services.DailyQuest;
using LifeQuestAPI.Infrastructure.Services.Gamification;
using LifeQuestAPI.Infrastructure.Services.Social;
using LifeQuestAPI.Infrastructure.Services.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LifeQuestAPI.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Mevcut servisler
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IGamificationService, GamificationService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IDailyQuestService, DailyQuestService>();
        services.AddScoped<IChallengeService, ChallengeService>();

        // SP-01: Bağlam-duyarlı görev öneri servisi (explicit namespace - isim çakışması önleme)
        services.AddScoped<LifeQuestAPI.Application.Abstractions.ITaskRecommendationService,
            LifeQuestAPI.Infrastructure.Services.DailyQuest.TaskRecommendationService>();

        // AI Chat servisi — Python mikro servise bağlanır
        var aiServiceBaseUrl = configuration["AIService:BaseUrl"] ?? "http://localhost:5001";

        services.AddHttpClient<IAIChatService, AIChatService>(client =>
        {
            client.BaseAddress = new Uri(aiServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        });
    }
}