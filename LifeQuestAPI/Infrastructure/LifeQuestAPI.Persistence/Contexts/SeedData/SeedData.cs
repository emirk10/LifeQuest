using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Common.Security;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LifeQuestAPI.Persistence.Contexts.SeedData;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<LifeQuestDbContext>();

        context.Database.EnsureCreated();

        var egitim = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Eğitim");
        if (egitim == null)
        {
            egitim = new Category { Name = "Eğitim", Description = "Akademik ve kişisel gelişim", IconPath = "education.png" };
            context.Categories.Add(egitim);
        }

        var saglik = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sağlık");
        if (saglik == null)
        {
            saglik = new Category { Name = "Sağlık", Description = "Fiziksel ve zihinsel sağlık", IconPath = "health.png" };
            context.Categories.Add(saglik);
        }

        var kariyer = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Kariyer");
        if (kariyer == null)
        {
            kariyer = new Category { Name = "Kariyer", Description = "İş hayatı ve profesyonel hedefler", IconPath = "career.png" };
            context.Categories.Add(kariyer);
        }

        var sosyal = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sosyal Sorumluluk");
        if (sosyal == null)
        {
            sosyal = new Category { Name = "Sosyal Sorumluluk", Description = "Topluma fayda ve gönüllülük", IconPath = "social.png" };
            context.Categories.Add(sosyal);
        }

        await context.SaveChangesAsync();

        if (!await context.Badges.AnyAsync())
        {
            var badges = new List<Badge>
            {
                new Badge { Name = "Yeni Başlayan", Description = "İlk adımı attın! (Level 1)", IconPath = "rookie.png", RequiredLevel = 1, CategoryId = null },
                new Badge { Name = "Hırslı", Description = "Artık durdurulamazsın! (Level 2)", IconPath = "ambitious.png", RequiredLevel = 2, CategoryId = null }
            };
            await context.Badges.AddRangeAsync(badges);
            await context.SaveChangesAsync();
        }

        if (!await context.AppTasks.AnyAsync())
        {
            var tasks = new List<AppTask>
            {
                // Doğrulama gerektirmeyen görevler (mevcut)
                new AppTask { Title = "Kitap Oku", Description = "En az 20 sayfa kitap oku.", CategoryId = egitim.Id, BaseXP = 50, MinLevel = 1, Difficulty = DifficultyLevel.Easy, IsActive = true, VerificationType = VerificationType.Reflection, MinReflectionWords = 20, VerificationHint = "Okuduğun bölüm hakkında kısa bir yansıma yaz." },
                new AppTask { Title = "Makale İncele", Description = "Alanında bir adet akademik makale oku.", CategoryId = egitim.Id, BaseXP = 100, MinLevel = 2, Difficulty = DifficultyLevel.Medium, IsActive = true, VerificationType = VerificationType.Reflection, MinReflectionWords = 30, VerificationHint = "Makale hakkında öğrendiklerini özetle." },
                new AppTask { Title = "Su İç", Description = "Günde 2 litre su hedefini tamamla.", CategoryId = saglik.Id, BaseXP = 20, MinLevel = 1, Difficulty = DifficultyLevel.Easy, IsActive = true, VerificationType = VerificationType.None },
                new AppTask { Title = "Yürüyüş Yap", Description = "5000 adım at.", CategoryId = saglik.Id, BaseXP = 80, MinLevel = 1, Difficulty = DifficultyLevel.Medium, IsActive = true, VerificationType = VerificationType.Photo, ExpectedImageCategory = "outdoor_exercise", VerificationHint = "Yürüyüş sırasında bir fotoğraf çek." },
                new AppTask { Title = "Sokak Hayvanlarını Besle", Description = "Bir kap mama veya su bırak.", CategoryId = sosyal.Id, BaseXP = 150, MinLevel = 1, Difficulty = DifficultyLevel.Medium, IsActive = true, VerificationType = VerificationType.Photo, ExpectedImageCategory = "social", VerificationHint = "Mama bıraktığın anı fotoğrafla." },

                // Yeni doğrulama tipli görevler
                new AppTask { Title = "30 Dakika Meditasyon", Description = "Sessiz bir ortamda 30 dakika meditasyon yap.", CategoryId = saglik.Id, BaseXP = 120, MinLevel = 1, Difficulty = DifficultyLevel.Medium, IsActive = true, VerificationType = VerificationType.Timer, MinTimerMinutes = 30, VerificationHint = "Zamanlayıcıyı başlat ve meditasyonunu tamamla." },
                new AppTask { Title = "Günlük Tutma", Description = "Bugünün değerlendirmesini yaz.", CategoryId = egitim.Id, BaseXP = 40, MinLevel = 1, Difficulty = DifficultyLevel.Easy, IsActive = true, VerificationType = VerificationType.Reflection, MinReflectionWords = 50, VerificationHint = "Bugün neler yaptığını ve nasıl hissettiğini yaz." },
                new AppTask { Title = "Spor Yap", Description = "En az 45 dakika egzersiz yap.", CategoryId = saglik.Id, BaseXP = 150, MinLevel = 2, Difficulty = DifficultyLevel.Hard, IsActive = true, VerificationType = VerificationType.PhotoAndReflection, MinReflectionWords = 15, ExpectedImageCategory = "indoor_exercise", VerificationHint = "Egzersiz sırasında/sonrasında fotoğraf çek ve deneyimini yaz." },
                new AppTask { Title = "Yeni Bir Beceri Öğren", Description = "Online bir eğitim videosu izle ve not al.", CategoryId = kariyer.Id, BaseXP = 100, MinLevel = 1, Difficulty = DifficultyLevel.Medium, IsActive = true, VerificationType = VerificationType.Reflection, MinReflectionWords = 40, VerificationHint = "Öğrendiğin konuyu ve aldığın notları paylaş." },
                new AppTask { Title = "Sağlıklı Yemek Pişir", Description = "Evde sağlıklı bir öğün hazırla.", CategoryId = saglik.Id, BaseXP = 90, MinLevel = 1, Difficulty = DifficultyLevel.Easy, IsActive = true, VerificationType = VerificationType.Photo, ExpectedImageCategory = "cooking", VerificationHint = "Hazırladığın yemeğin fotoğrafını çek." },
            };
            await context.AppTasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
        }

        // Achievement seed data
        if (!await context.Achievements.AnyAsync())
        {
            var achievements = new List<Achievement>
            {
                new Achievement { Title = "İlk Adım", Description = "İlk görevini tamamla!", IconPath = "🎯", Type = AchievementType.TaskComplete, RequiredCount = 1, BonusXP = 50, IsHidden = false },
                new Achievement { Title = "Çalışkan Arı", Description = "10 görev tamamla.", IconPath = "🐝", Type = AchievementType.TaskComplete, RequiredCount = 10, BonusXP = 200, IsHidden = false },
                new Achievement { Title = "Görev Makinesi", Description = "50 görev tamamla.", IconPath = "⚙️", Type = AchievementType.TaskComplete, RequiredCount = 50, BonusXP = 500, IsHidden = false },
                new Achievement { Title = "Efsane", Description = "100 görev tamamla.", IconPath = "🏆", Type = AchievementType.TaskComplete, RequiredCount = 100, BonusXP = 1000, IsHidden = false },
                new Achievement { Title = "3 Günlük Seri", Description = "3 gün üst üste görev tamamla.", IconPath = "🔥", Type = AchievementType.StreakDays, RequiredCount = 3, BonusXP = 100, IsHidden = false },
                new Achievement { Title = "Haftalık Seri", Description = "7 gün üst üste görev tamamla.", IconPath = "💥", Type = AchievementType.StreakDays, RequiredCount = 7, BonusXP = 300, IsHidden = false },
                new Achievement { Title = "Aylık Seri", Description = "30 gün üst üste görev tamamla.", IconPath = "👑", Type = AchievementType.StreakDays, RequiredCount = 30, BonusXP = 1000, IsHidden = false },
                new Achievement { Title = "Erken Kuş", Description = "Sabah 7'den önce bir görev tamamla.", IconPath = "🌅", Type = AchievementType.EarlyBird, RequiredCount = 1, BonusXP = 150, IsHidden = true },
                new Achievement { Title = "Gece Kuşu", Description = "Gece 23'ten sonra bir görev tamamla.", IconPath = "🌙", Type = AchievementType.NightOwl, RequiredCount = 1, BonusXP = 150, IsHidden = true },
                new Achievement { Title = "Koleksiyoncu", Description = "5 farklı rozet topla.", IconPath = "🎖️", Type = AchievementType.Collector, RequiredCount = 5, BonusXP = 300, IsHidden = false },
                new Achievement { Title = "Sosyal Kelebek", Description = "3 arkadaş edin.", IconPath = "🦋", Type = AchievementType.SocialButterfly, RequiredCount = 3, BonusXP = 200, IsHidden = false },
                new Achievement { Title = "Hız Koşucusu", Description = "Bir görevi 10 dakikadan kısa sürede tamamla.", IconPath = "⚡", Type = AchievementType.SpeedRunner, RequiredCount = 1, BonusXP = 100, IsHidden = true },
            };
            await context.Achievements.AddRangeAsync(achievements);
            await context.SaveChangesAsync();
        }

        var adminRole = await context.AppRoles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new AppRole { Id = Guid.NewGuid(), Name = "Admin"};
            context.AppRoles.Add(adminRole);
        }

        var userRole = await context.AppRoles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            userRole = new AppRole { Id = Guid.NewGuid(), Name = "User"};
            context.AppRoles.Add(userRole);
        }

        await context.SaveChangesAsync();

        if (!await context.AppUsers.AnyAsync(u => u.Email == "admin@lifequest.com"))
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash("Admin123!", out passwordHash, out passwordSalt);

            var adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                Name = "System",
                Surname = "Admin",
                UserName = "system_admin",
                Email = "admin@lifequest.com",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                TotalXP = 99999,
                GeneralLevel = 99,
                AppRoleId = adminRole.Id,
            };

            await context.AppUsers.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}