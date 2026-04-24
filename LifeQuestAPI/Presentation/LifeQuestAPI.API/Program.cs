using System.Text;
using System.Threading.RateLimiting;
using LifeQuestAPI.API.BackgroundServices;
using LifeQuestAPI.Application;
using LifeQuestAPI.Infrastructure;
using LifeQuestAPI.Persistence;
using LifeQuestAPI.Persistence.Contexts.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHostedService<DailyQuestWorker>();
builder.Services.AddHostedService<LifeQuestAPI.API.BackgroundServices.WeeklyReportWorker>();
builder.Services.AddHostedService<NudgeWorker>(); // SP-03
builder.Services.AddHostedService<GoalRevisionWorker>(); // SP-04
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<LifeQuestAPI.Application.Abstractions.Notifications.IAppNotificationService, LifeQuestAPI.API.Services.AppNotificationService>();

// CORS Politikası
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true) // Tüm originlere izin ver
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // SignalR için gerekli
        });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 500,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = 429;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"] ?? "bu_proje_icin_cok_gizli_ve_cok_uzun_bir_anahtar_olusturdum_en_az_64_karakter_olmali_ki_sha512_calissin_lifequest_2025")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();
app.UseMiddleware<LifeQuestAPI.API.Middlewares.GlobalExceptionMiddleware>();

// app.UseHttpsRedirection(); // Dev ortamında (Docker) localhost mixed-content ve sertifika sorunlarını önlemek için kapatıldı
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Statik dosyalar (uploads klasoru icin)
app.UseStaticFiles();

app.MapControllers();
app.MapHub<LifeQuestAPI.API.Hubs.NotificationHub>("/hubs/notification");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanina seed data eklenirken bir hata olustu.");
    }
}

app.Run();
