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

        var kisiselGelisim = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Kişisel Gelişim");
        if (kisiselGelisim == null)
        {
            kisiselGelisim = new Category { Name = "Kişisel Gelişim", Description = "Kişisel farkındalık ve gelişim", IconPath = "🌱" };
            context.Categories.Add(kisiselGelisim);
        }

        var yaraticilik = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Yaratıcılık");
        if (yaraticilik == null)
        {
            yaraticilik = new Category { Name = "Yaratıcılık", Description = "Yaratıcı projeler ve hobiler", IconPath = "🎨" };
            context.Categories.Add(yaraticilik);
        }

        await context.SaveChangesAsync();

        // ---- ROZETLER (Min 20) ----
        if (await context.Badges.CountAsync() < 20)
        {
            var existingNames = await context.Badges.Select(b => b.Name).ToListAsync();
            var newBadges = new List<Badge>();

            void AddBadge(string name, string desc, string icon, int level, Guid? catId = null) {
                if (!existingNames.Contains(name)) newBadges.Add(new Badge { Name = name, Description = desc, IconPath = icon, RequiredLevel = level, CategoryId = catId });
            }

            AddBadge("Yeni Başlayan", "İlk adımı attın! (Level 1)", "🌱", 1);
            AddBadge("Hırslı", "Artık durdurulamazsın! (Level 2)", "⚡", 2);
            AddBadge("Kararlı", "Tutarlı ilerliyorsun. (Level 5)", "🎯", 5);
            AddBadge("Uzman", "Gerçek bir uzman oldun! (Level 10)", "🏆", 10);
            AddBadge("Efsane", "Efsanevi seviyeye ulaştın! (Level 20)", "👑", 20);
            AddBadge("Bilge", "Büyük bir zihinsel olgunluğa ulaştın (Level 12)", "🧠", 12);
            AddBadge("Şampiyon", "Kendi sınırlarını aştın (Level 15)", "🏅", 15);
            AddBadge("Demir İrade", "Eğilmez, bükülmez bir disiplin sahibisin (Level 25)", "🛡️", 25);
            AddBadge("Vizyoner", "Hayallerini gerçeğe dönüştürdün (Level 30)", "👁️‍🗨️", 30);
            
            // Kategori Spesifik Rozetler
            AddBadge("Sağlık Gurusu", "Sağlık kategorisinde uzmanlaştın", "🥗", 10, saglik.Id);
            AddBadge("Bilgi Avcısı", "Eğitim görevlerinde sınır tanımıyorsun", "📖", 10, egitim.Id);
            AddBadge("Kariyer Mimarı", "Profesyonel hayatını ilmek ilmek dokuyorsun", "🏗️", 10, kariyer.Id);
            AddBadge("Hayırsever", "Başkalarına yardım etmek senin doğanda var", "❤️", 10, sosyal.Id);
            AddBadge("Sanatçı", "Yaratıcılığınla dünyayı renklendiriyorsun", "🎭", 10, yaraticilik.Id);
            AddBadge("Gelişim Öncüsü", "Her gün biraz daha iyileşiyorsun", "🔝", 10, kisiselGelisim.Id);

            // Seri Rozetleri
            AddBadge("7 Günlük Ateş", "7 günlük seri yakaladın", "🔥", 3);
            AddBadge("Ay Tutulması", "30 günlük seriyle imkansızı başardın", "🌑", 15);
            AddBadge("Yıllık Abide", "365 gün kesintisiz LifeQuest!", "🏛️", 50);

            // Sosyal Rozetler
            AddBadge("Popüler", "Geniş bir arkadaş çevresine sahipsin", "👥", 5);
            AddBadge("Lider", "Başkalarına ilham veriyorsun", "📣", 15);

            if (newBadges.Any()) {
                await context.Badges.AddRangeAsync(newBadges);
                await context.SaveChangesAsync();
            }
        }

        // ---- GÖREVLER (Min 150) ----
        if (await context.AppTasks.CountAsync() < 150)
        {
            var tasks = new List<AppTask>();
            
            // Sabit görevler (Bazı örnekler)
            tasks.Add(new AppTask { Title = "Kişisel Vizyon Panosu", Description = "Gelecekteki hedeflerini görselleştiren bir pano oluştur.", CategoryId = kisiselGelisim.Id, BaseXP = 200, MinLevel = 2, Difficulty = DifficultyLevel.Medium, IsActive = true });
            tasks.Add(new AppTask { Title = "Yeni Bir Hobiyi Araştır", Description = "Her zaman yapmak istediğin o hobi için ilk araştırmanı yap.", CategoryId = yaraticilik.Id, BaseXP = 50, MinLevel = 1, Difficulty = DifficultyLevel.Easy, IsActive = true });
            
            // Dinamik Görev Üretimi (Tüm kategoriler için)
            var categoryList = new[] { egitim, saglik, kariyer, sosyal, kisiselGelisim, yaraticilik };
            
            for (int i = 1; i <= 25; i++)
            {
                foreach (var cat in categoryList)
                {
                    tasks.Add(new AppTask 
                    { 
                        Title = $"{cat.Name} Görevi #{i}", 
                        Description = $"{cat.Name} alanında gelişmeni sağlayacak {i}. adım görevi.", 
                        CategoryId = cat.Id, 
                        BaseXP = 40 + (i % 5) * 15, 
                        MinLevel = 1 + (i % 7), 
                        MaxLevel = 99, 
                        Difficulty = (DifficultyLevel)(i % 3), 
                        IsDaily = i % 2 == 0, 
                        IsActive = true,
                        EstimatedMinutes = 10 + (i % 4) * 10
                    });
                }
            }

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