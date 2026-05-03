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

        context.Database.Migrate();

        // ---- KATEGORİLER ----
        var egitim = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Eğitim");
        if (egitim == null)
        {
            egitim = new Category { Name = "Eğitim", Description = "Akademik ve kişisel gelişim", IconPath = "📚" };
            context.Categories.Add(egitim);
        }

        var saglik = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sağlık");
        if (saglik == null)
        {
            saglik = new Category { Name = "Sağlık", Description = "Fiziksel ve zihinsel sağlık", IconPath = "💪" };
            context.Categories.Add(saglik);
        }

        var kariyer = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Kariyer");
        if (kariyer == null)
        {
            kariyer = new Category { Name = "Kariyer", Description = "İş hayatı ve profesyonel hedefler", IconPath = "💼" };
            context.Categories.Add(kariyer);
        }

        var sosyal = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sosyal Sorumluluk");
        if (sosyal == null)
        {
            sosyal = new Category { Name = "Sosyal Sorumluluk", Description = "Topluma fayda ve gönüllülük", IconPath = "🤝" };
            context.Categories.Add(sosyal);
        }

        await context.SaveChangesAsync();

        // ---- ROZETLER ----
        if (!await context.Badges.AnyAsync())
        {
            var badges = new List<Badge>
            {
                new Badge { Name = "Yeni Başlayan", Description = "İlk adımı attın! (Level 1)", IconPath = "🌱", RequiredLevel = 1, CategoryId = null },
                new Badge { Name = "Hırslı", Description = "Artık durdurulamazsın! (Level 2)", IconPath = "⚡", RequiredLevel = 2, CategoryId = null },
                new Badge { Name = "Kararlı", Description = "Tutarlı ilerliyorsun. (Level 5)", IconPath = "🎯", RequiredLevel = 5, CategoryId = null },
                new Badge { Name = "Uzman", Description = "Gerçek bir uzman oldun! (Level 10)", IconPath = "🏆", RequiredLevel = 10, CategoryId = null },
                new Badge { Name = "Efsane", Description = "Efsanevi seviyeye ulaştın! (Level 20)", IconPath = "👑", RequiredLevel = 20, CategoryId = null },
            };
            await context.Badges.AddRangeAsync(badges);
            await context.SaveChangesAsync();
        }

        // ---- GÖREVLER ----
        if (!await context.AppTasks.AnyAsync())
        {
            var tasks = new List<AppTask>
            {
                // Eğitim Görevleri
                new AppTask { Title = "Kitap Oku", Description = "En az 20 sayfa kitap oku. Sevdiğin bir konudan başlayabilirsin.", CategoryId = egitim.Id, BaseXP = 50, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = true, IsActive = true },
                new AppTask { Title = "Makale İncele", Description = "Alanında bir adet akademik veya popüler makale oku.", CategoryId = egitim.Id, BaseXP = 100, MinLevel = 2, MaxLevel = 99, Difficulty = DifficultyLevel.Medium, IsDaily = false, IsActive = true },
                new AppTask { Title = "Online Kurs Dersi", Description = "Online bir eğitim platformunda en az bir ders tamamla.", CategoryId = egitim.Id, BaseXP = 120, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Medium, IsDaily = true, IsActive = true },
                new AppTask { Title = "Dil Çalışması", Description = "Yabancı dil uygulamasında 15 dakika çalış.", CategoryId = egitim.Id, BaseXP = 40, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = true, IsActive = true },
                new AppTask { Title = "Günlük Tutma", Description = "Bugünün değerlendirmesini yaz. Öğrendiklerini ve hedeflerini not al.", CategoryId = egitim.Id, BaseXP = 40, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = true, IsActive = true },
                new AppTask { Title = "Podcast Dinle", Description = "Kişisel gelişim veya eğitim temalı bir podcast dinle.", CategoryId = egitim.Id, BaseXP = 60, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = false, IsActive = true },

                // Sağlık Görevleri
                new AppTask { Title = "Su İç", Description = "Günde 2 litre su hedefini tamamla. Sağlıklı bir vücut için su şart!", CategoryId = saglik.Id, BaseXP = 20, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = true, IsActive = true },
                new AppTask { Title = "Yürüyüş Yap", Description = "En az 5000 adım at. Hafif tempo yeterli!", CategoryId = saglik.Id, BaseXP = 80, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Medium, IsDaily = true, IsActive = true },
                new AppTask { Title = "Spor Yap", Description = "En az 45 dakika egzersiz yap. Kardiyo veya ağırlık çalışması.", CategoryId = saglik.Id, BaseXP = 150, MinLevel = 2, MaxLevel = 99, Difficulty = DifficultyLevel.Hard, IsDaily = false, IsActive = true },
                new AppTask { Title = "Meditasyon Yap", Description = "30 dakika meditasyon veya nefes egzersizi yap.", CategoryId = saglik.Id, BaseXP = 120, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Medium, IsDaily = true, IsActive = true },
                new AppTask { Title = "Sağlıklı Yemek Pişir", Description = "Evde sağlıklı bir öğün hazırla. Sebze ağırlıklı tercih et.", CategoryId = saglik.Id, BaseXP = 90, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = false, IsActive = true },
                new AppTask { Title = "Erken Uyan", Description = "Sabah 7'den önce uyan ve güne enerjik başla.", CategoryId = saglik.Id, BaseXP = 60, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Medium, IsDaily = true, IsActive = true },

                // Kariyer Görevleri
                new AppTask { Title = "LinkedIn Güncelle", Description = "LinkedIn profilini veya portföyünü güncelle.", CategoryId = kariyer.Id, BaseXP = 80, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Medium, IsDaily = false, IsActive = true },
                new AppTask { Title = "Haftalık Plan Yap", Description = "Bu haftaki hedef ve görevlerini belirle, önceliklendir.", CategoryId = kariyer.Id, BaseXP = 60, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = false, IsActive = true },
                new AppTask { Title = "Networking Yap", Description = "Alanından biriyle iletişime geç veya yeni bir bağlantı kur.", CategoryId = kariyer.Id, BaseXP = 100, MinLevel = 3, MaxLevel = 99, Difficulty = DifficultyLevel.Medium, IsDaily = false, IsActive = true },
                new AppTask { Title = "Proje Çalışması", Description = "Kişisel veya profesyonel bir projen üzerinde en az 1 saat çalış.", CategoryId = kariyer.Id, BaseXP = 150, MinLevel = 2, MaxLevel = 99, Difficulty = DifficultyLevel.Hard, IsDaily = true, IsActive = true },

                // Sosyal Sorumluluk Görevleri
                new AppTask { Title = "Sokak Hayvanlarını Besle", Description = "Sokak hayvanları için bir kap mama veya su bırak.", CategoryId = sosyal.Id, BaseXP = 150, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = false, IsActive = true },
                new AppTask { Title = "Gönüllü Çalışma", Description = "Bir sivil toplum kuruluşuna destek ver veya gönüllü çalışmaya katıl.", CategoryId = sosyal.Id, BaseXP = 300, MinLevel = 3, MaxLevel = 99, Difficulty = DifficultyLevel.Hard, IsDaily = false, IsActive = true },
                new AppTask { Title = "Komşuna Yardım Et", Description = "Bir komşuna veya aile büyüğüne yardım et.", CategoryId = sosyal.Id, BaseXP = 100, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Easy, IsDaily = false, IsActive = true },
                new AppTask { Title = "Çevre Temizliği", Description = "Bulunduğun çevrenin temizliğine katkı sağla.", CategoryId = sosyal.Id, BaseXP = 120, MinLevel = 1, MaxLevel = 99, Difficulty = DifficultyLevel.Medium, IsDaily = false, IsActive = true },
            };
            await context.AppTasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
        }

        // ---- BAŞARIMLAR ----
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

        // ---- ROLLER ----
        var adminRole = await context.AppRoles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new AppRole { Id = Guid.NewGuid(), Name = "Admin" };
            context.AppRoles.Add(adminRole);
        }

        var userRole = await context.AppRoles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            userRole = new AppRole { Id = Guid.NewGuid(), Name = "User" };
            context.AppRoles.Add(userRole);
        }

        await context.SaveChangesAsync();

        // ---- ADMIN KULLANICI ----
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