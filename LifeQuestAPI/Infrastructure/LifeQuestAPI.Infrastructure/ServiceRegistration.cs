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
        
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IGamificationService, GamificationService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IDailyQuestService, DailyQuestService>();
        services.AddScoped<IChallengeService, ChallengeService>();

        
        services.AddScoped<LifeQuestAPI.Application.Abstractions.ITaskRecommendationService,
            LifeQuestAPI.Infrastructure.Services.DailyQuest.TaskRecommendationService>();

        
        services.AddSingleton<LifeQuestAPI.Application.Abstractions.AI.ITaskRecommendationService,
            LifeQuestAPI.Infrastructure.Services.AI.TaskRecommendationService>();

        
        services.AddScoped<IUserBehaviorAnalysisService, UserBehaviorAnalysisService>();

        
        
        var aiServiceBaseUrl = configuration["AIService:BaseUrl"] ?? "http://localhost:5001";

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); 

        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));

        services.AddHttpClient<IAIChatService, AIChatService>(client =>
        {
            client.BaseAddress = new Uri(aiServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(10); 
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => 
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (env == "Development")
                {
                    return true;
                }
                return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
            }
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);
    }
}

