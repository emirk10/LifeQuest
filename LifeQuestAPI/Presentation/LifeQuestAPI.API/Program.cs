using System.Text;
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
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<LifeQuestAPI.Application.Abstractions.Notifications.IAppNotificationService, LifeQuestAPI.API.Services.AppNotificationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"] ?? "")),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();
app.UseMiddleware<LifeQuestAPI.API.Middlewares.GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
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
