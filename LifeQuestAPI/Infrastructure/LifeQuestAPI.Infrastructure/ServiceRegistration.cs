using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Application.Abstractions.DailyQuest;
using LifeQuestAPI.Application.Abstractions.Gamification;
using LifeQuestAPI.Application.Abstractions.Token;
using LifeQuestAPI.Infrastructure.Services.AI;
using LifeQuestAPI.Infrastructure.Services.DailyQuest;
using LifeQuestAPI.Infrastructure.Services.Gamification;
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

        // AI mikro servis base URL (appsettings.json'dan okunuyor)
        var aiServiceBaseUrl = configuration["AIService:BaseUrl"] ?? "https://localhost:5001";

        // AI servisleri — Named HttpClient factory ile (SSL bypass eklendi - self-signed certs için)
        services.AddHttpClient<IAIChatService, AIChatService>(client =>
        {
            client.BaseAddress = new Uri(aiServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        });

        services.AddHttpClient<IImageVerificationService, ImageVerificationService>(client =>
        {
            client.BaseAddress = new Uri(aiServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(60); // Görüntü işleme daha uzun sürebilir
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        });

        services.AddHttpClient<IReflectionVerificationService, ReflectionVerificationService>(client =>
        {
            client.BaseAddress = new Uri(aiServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        });

        // ML.NET Öneri servisi (Singleton olabilir, MLContext thread-safe'tir model yuklendikten sonra)
        services.AddSingleton<ITaskRecommendationService, TaskRecommendationService>();
    }
}