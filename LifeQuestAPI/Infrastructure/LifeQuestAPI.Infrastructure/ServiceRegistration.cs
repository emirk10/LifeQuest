using LifeQuestAPI.Application.Abstractions;
using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Application.Abstractions.DailyQuest;
using LifeQuestAPI.Application.Abstractions.Email;
using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Social;
using LifeQuestAPI.Application.Abstractions.Token;
using LifeQuestAPI.Infrastructure.Services.AI;
using LifeQuestAPI.Infrastructure.Services.BehaviorAnalysis;
using LifeQuestAPI.Infrastructure.Services.DailyQuest;
using LifeQuestAPI.Infrastructure.Services.Email;
using LifeQuestAPI.Infrastructure.Services.Gamification;
using LifeQuestAPI.Infrastructure.Services.Social;
using LifeQuestAPI.Infrastructure.Services.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace LifeQuestAPI.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Mevcut servisler
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IGamificationService, GamificationService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IDailyQuestService, DailyQuestService>();
        services.AddScoped<IChallengeService, ChallengeService>();

        // SP-01: Bağlam-duyarlı görev öneri servisi (explicit namespace - isim çakışması önleme)
        services.AddScoped<LifeQuestAPI.Application.Abstractions.ITaskRecommendationService,
            LifeQuestAPI.Infrastructure.Services.DailyQuest.TaskRecommendationService>();

        // SP-03: Davranış analizi servisi
        services.AddScoped<IUserBehaviorAnalysisService, UserBehaviorAnalysisService>();

        // AI Chat servisi — Python mikro servise bağlanır
        // MISS-010: 5s timeout + 3 retry (exponential backoff) + circuit breaker
        var aiServiceBaseUrl = configuration["AIService:BaseUrl"] ?? "http://localhost:5001";

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // 2s, 4s, 8s

        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));

        services.AddHttpClient<IAIChatService, AIChatService>(client =>
        {
            client.BaseAddress = new Uri(aiServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10); // MISS-010: 5s yerine 10s (3 deneme için)
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);
    }
}
