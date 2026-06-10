using LifeQuestAPI.Application.Common.Security;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LifeQuestAPI.Persistence.Contexts.SeedData;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var context = serviceProvider.GetRequiredService<LifeQuestDbContext>();

        
        context.Database.Migrate();

        
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

        
        var egitim = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Eğitim");
        if (egitim == null)
        {
            egitim = new Category { Id = Guid.NewGuid(), Name = "Eğitim", Description = "Akademik ve kişisel gelişim", IconPath = "📚" };
            context.Categories.Add(egitim);
        }

        var saglik = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sağlık");
        if (saglik == null)
        {
            saglik = new Category { Id = Guid.NewGuid(), Name = "Sağlık", Description = "Fiziksel ve zihinsel sağlık", IconPath = "💪" };
            context.Categories.Add(saglik);
        }

        var kariyer = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Kariyer");
        if (kariyer == null)
        {
            kariyer = new Category { Id = Guid.NewGuid(), Name = "Kariyer", Description = "İş hayatı ve profesyonel hedefler", IconPath = "💼" };
            context.Categories.Add(kariyer);
        }

        var sosyal = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sosyal Sorumluluk");
        if (sosyal == null)
        {
            sosyal = new Category { Id = Guid.NewGuid(), Name = "Sosyal Sorumluluk", Description = "Topluma fayda ve gönüllülük", IconPath = "🤝" };
            context.Categories.Add(sosyal);
        }

        var kisiselGelisim = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Kişisel Gelişim");
        if (kisiselGelisim == null)
        {
            kisiselGelisim = new Category { Id = Guid.NewGuid(), Name = "Kişisel Gelişim", Description = "Kişisel farkındalık ve gelişim", IconPath = "🌱" };
            context.Categories.Add(kisiselGelisim);
        }

        var yaraticilik = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Yaratıcılık");
        if (yaraticilik == null)
        {
            yaraticilik = new Category { Id = Guid.NewGuid(), Name = "Yaratıcılık", Description = "Yaratıcı projeler ve hobiler", IconPath = "🎨" };
            context.Categories.Add(yaraticilik);
        }

        await context.SaveChangesAsync();

        
        if (!await context.Badges.AnyAsync())
        {
            var badges = new List<Badge>
            {
                
                new() { Name = "Yeni Başlayan", Description = "İlk adımı attın! (Level 1)", IconPath = "🌱", Type = BadgeType.Level, RequiredValue = 1, BadgeColor = "#10B981", Rarity = "Common" },
                new() { Name = "Hırslı Kaşif", Description = "Artık durdurulamazsın! (Level 5)", IconPath = "⚡", Type = BadgeType.Level, RequiredValue = 5, BadgeColor = "#3B82F6", Rarity = "Common" },
                new() { Name = "Kararlı Savaşçı", Description = "Tutarlı ilerliyorsun. (Level 10)", IconPath = "🎯", Type = BadgeType.Level, RequiredValue = 10, BadgeColor = "#8B5CF6", Rarity = "Rare" },
                new() { Name = "Gelişim Uzmanı", Description = "Gerçek bir uzman oldun! (Level 15)", IconPath = "🏆", Type = BadgeType.Level, RequiredValue = 15, BadgeColor = "#F59E0B", Rarity = "Epic" },
                new() { Name = "Efsanevi Quest Master", Description = "Efsanevi seviyeye ulaştın! (Level 25)", IconPath = "👑", Type = BadgeType.Level, RequiredValue = 25, BadgeColor = "#EF4444", Rarity = "Legendary" },
                new() { Name = "Bilge Lider", Description = "Büyük bir zihinsel olgunluğa ulaştın (Level 35)", IconPath = "🧠", Type = BadgeType.Level, RequiredValue = 35, BadgeColor = "#EC4899", Rarity = "Legendary" },

                
                new() { Name = "Bilgi Avcısı", Description = "Eğitim görevlerinde sınır tanımıyorsun", IconPath = "📖", Type = BadgeType.CategoryMaster, RequiredValue = 10, CategoryId = egitim.Id, BadgeColor = "#3B82F6", Rarity = "Rare" },
                new() { Name = "Sağlık Gurusu", Description = "Sağlık kategorisinde uzmanlaştın", IconPath = "🥗", Type = BadgeType.CategoryMaster, RequiredValue = 10, CategoryId = saglik.Id, BadgeColor = "#10B981", Rarity = "Rare" },
                new() { Name = "Kariyer Mimarı", Description = "Profesyonel hayatını ilmek ilmek dokuyorsun", IconPath = "🏗️", Type = BadgeType.CategoryMaster, RequiredValue = 10, CategoryId = kariyer.Id, BadgeColor = "#6B7280", Rarity = "Rare" },
                new() { Name = "Gönüllü Kahraman", Description = "Başkalarına yardım etmek senin doğanda var", IconPath = "❤️", Type = BadgeType.CategoryMaster, RequiredValue = 10, CategoryId = sosyal.Id, BadgeColor = "#EF4444", Rarity = "Rare" },
                new() { Name = "Kişisel Gelişim Öncüsü", Description = "Her gün biraz daha iyileşiyorsun", IconPath = "🔝", Type = BadgeType.CategoryMaster, RequiredValue = 10, CategoryId = kisiselGelisim.Id, BadgeColor = "#8B5CF6", Rarity = "Rare" },
                new() { Name = "Sanat Dehası", Description = "Yaratıcılığınla dünyayı renklendiriyorsun", IconPath = "🎨", Type = BadgeType.CategoryMaster, RequiredValue = 10, CategoryId = yaraticilik.Id, BadgeColor = "#F59E0B", Rarity = "Rare" },

                
                new() { Name = "3 Günlük Ateş", Description = "3 günlük seri yakaladın", IconPath = "🔥", Type = BadgeType.Streak, RequiredValue = 3, BadgeColor = "#F59E0B", Rarity = "Common" },
                new() { Name = "Haftalık Kıvılcım", Description = "7 günlük seriyle düzenliliği yakaladın", IconPath = "⚡", Type = BadgeType.Streak, RequiredValue = 7, BadgeColor = "#EF4444", Rarity = "Common" },
                new() { Name = "Ay Tutulması", Description = "30 günlük seriyle imkansızı başardın", IconPath = "🌑", Type = BadgeType.Streak, RequiredValue = 30, BadgeColor = "#8B5CF6", Rarity = "Epic" },
                new() { Name = "Yıllık Abide", Description = "365 gün kesintisiz LifeQuest!", IconPath = "🏛️", Type = BadgeType.Streak, RequiredValue = 365, BadgeColor = "#EF4444", Rarity = "Legendary" },

                
                new() { Name = "İlk Adım", Description = "İlk görevini tamamladın", IconPath = "👣", Type = BadgeType.TaskCount, RequiredValue = 1, BadgeColor = "#10B981", Rarity = "Common" },
                new() { Name = "Görev İşçisi", Description = "Toplamda 25 görev tamamladın", IconPath = "🛠️", Type = BadgeType.TaskCount, RequiredValue = 25, BadgeColor = "#3B82F6", Rarity = "Common" },
                new() { Name = "Çalışkan Karınca", Description = "Toplamda 100 görev tamamladın", IconPath = "🐜", Type = BadgeType.TaskCount, RequiredValue = 100, BadgeColor = "#8B5CF6", Rarity = "Rare" },
                new() { Name = "Görev Canavarı", Description = "Toplamda 500 görev tamamladın", IconPath = "👹", Type = BadgeType.TaskCount, RequiredValue = 500, BadgeColor = "#EF4444", Rarity = "Epic" },

                
                new() { Name = "Sosyal Kelebek", Description = "3 arkadaş edindin", IconPath = "🦋", Type = BadgeType.Social, RequiredValue = 3, BadgeColor = "#EC4899", Rarity = "Common" },
                new() { Name = "Popüler Figür", Description = "10 arkadaş edindin", IconPath = "👥", Type = BadgeType.Social, RequiredValue = 10, BadgeColor = "#EC4899", Rarity = "Rare" },
                new() { Name = "Geri Dönüş Kahramanı", Description = "Uzun süre sonra uygulamaya dönüp görev tamamladın", IconPath = "💫", Type = BadgeType.Comeback, RequiredValue = 1, BadgeColor = "#6B7280", Rarity = "Rare" },
                new() { Name = "İstikrar Abidesi", Description = "10 gün üst üste her gün en az 2 görev yaptın", IconPath = "🛡️", Type = BadgeType.Consistency, RequiredValue = 10, BadgeColor = "#8B5CF6", Rarity = "Epic" },
                new() { Name = "Gizemli Yolcu", Description = "Gizli bir görevi başarıyla tamamladın", IconPath = "🕵️", Type = BadgeType.FirstTime, RequiredValue = 1, IsSecret = true, BadgeColor = "#111827", Rarity = "Epic" }
            };

            await context.Badges.AddRangeAsync(badges);
            await context.SaveChangesAsync();
        }

        
        if (!await context.Achievements.AnyAsync())
        {
            var achievements = new List<Achievement>
            {
                new() { Title = "İlk Adım", Description = "İlk görevini tamamla!", IconPath = "🎯", Type = AchievementType.TaskComplete, RequiredCount = 1, BonusXP = 50, IsHidden = false },
                new() { Title = "Çalışkan Arı", Description = "10 görev tamamla.", IconPath = "🐝", Type = AchievementType.TaskComplete, RequiredCount = 10, BonusXP = 200, IsHidden = false },
                new() { Title = "Görev Makinesi", Description = "50 görev tamamla.", IconPath = "⚙️", Type = AchievementType.TaskComplete, RequiredCount = 50, BonusXP = 500, IsHidden = false },
                new() { Title = "Efsane", Description = "100 görev tamamla.", IconPath = "🏆", Type = AchievementType.TaskComplete, RequiredCount = 100, BonusXP = 1000, IsHidden = false },
                new() { Title = "3 Günlük Seri", Description = "3 gün üst üste görev tamamla.", IconPath = "🔥", Type = AchievementType.StreakDays, RequiredCount = 3, BonusXP = 100, IsHidden = false },
                new() { Title = "Haftalık Seri", Description = "7 gün üst üste görev tamamla.", IconPath = "💥", Type = AchievementType.StreakDays, RequiredCount = 7, BonusXP = 300, IsHidden = false },
                new() { Title = "Aylık Seri", Description = "30 gün üst üste görev tamamla.", IconPath = "👑", Type = AchievementType.StreakDays, RequiredCount = 30, BonusXP = 1000, IsHidden = false },
                new() { Title = "Erken Kuş", Description = "Sabah 7'den önce bir görev tamamla.", IconPath = "🌅", Type = AchievementType.EarlyBird, RequiredCount = 1, BonusXP = 150, IsHidden = true },
                new() { Title = "Gece Kuşu", Description = "Gece 23'ten sonra bir görev tamamla.", IconPath = "🌙", Type = AchievementType.NightOwl, RequiredCount = 1, BonusXP = 150, IsHidden = true },
                new() { Title = "Koleksiyoncu", Description = "5 farklı rozet topla.", IconPath = "🎖️", Type = AchievementType.Collector, RequiredCount = 5, BonusXP = 300, IsHidden = false },
                new() { Title = "Sosyal Kelebek", Description = "3 arkadaş edin.", IconPath = "🦋", Type = AchievementType.SocialButterfly, RequiredCount = 3, BonusXP = 200, IsHidden = false },
                new() { Title = "Hız Koşucusu", Description = "Bir görevi 10 dakikadan kısa sürede tamamla.", IconPath = "⚡", Type = AchievementType.SpeedRunner, RequiredCount = 1, BonusXP = 100, IsHidden = true },
                new() { Title = "Kategori Ustası", Description = "Herhangi bir kategoride 10 görev tamamla.", IconPath = "🏅", Type = AchievementType.CategoryMaster, RequiredCount = 10, BonusXP = 250, IsHidden = false }
            };

            await context.Achievements.AddRangeAsync(achievements);
            await context.SaveChangesAsync();
        }

        
        if (!await context.AppTasks.AnyAsync())
        {
            var tasks = new List<AppTask>();

            
            AppTask CreateTask(string title, string desc, Guid categoryId, DifficultyLevel difficulty, int xp, int minutes, string why, List<DayTimeSlot> slots, List<string> weaknesses, List<string> goals, List<string> tags)
            {
                return new AppTask
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Description = desc,
                    CategoryId = categoryId,
                    Difficulty = difficulty,
                    BaseXP = xp,
                    MinLevel = 1,
                    MaxLevel = 99,
                    IsActive = true,
                    IsDaily = false,
                    EstimatedMinutes = minutes,
                    WhyThisTask = why,
                    RecommendedTimeSlots = slots,
                    TargetedWeaknesses = weaknesses,
                    TargetedGoals = goals,
                    Tags = tags,
                    IsChunkable = minutes > 30,
                    ChunkMinutes = minutes > 30 ? 15 : null,
                    MinEnergyLevel = difficulty == DifficultyLevel.Easy ? 1 : (difficulty == DifficultyLevel.Medium ? 2 : 4),
                    MaxEnergyLevel = 5,
                    BadgeHint = xp >= 150 ? "Bu zorlu görevi tamamlamak seni Gelişim Uzmanı rozetine yaklaştırır!" : null
                };
            }

            
            var eduId = egitim.Id;
            tasks.Add(CreateTask("Haber Analizi", "Gündemdeki önemli bir haberi en az 3 farklı kaynaktan okuyup karşılaştır.", eduId, DifficultyLevel.Easy, 40, 15, "Medya okuryazarlığını artırmak ve tarafsız bakış açısı kazanmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon }, new() { "bias", "distraction" }, new() { "intellectual_growth" }, new() { "analiz", "haber" }));
            tasks.Add(CreateTask("Online Kurs Videosu", "Devam ettiğin online eğitimden en az 1 ders videosu izle ve not al.", eduId, DifficultyLevel.Medium, 80, 30, "Sürekli öğrenme ve mesleki bilgiyi taze tutmak amacıyla önerilmiştir.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "procrastination" }, new() { "career_advancement", "learning" }, new() { "kurs", "video" }));
            tasks.Add(CreateTask("Teknik Makale Okuma", "Mesleğin veya ilgi alanınla ilgili teknik bir makale oku.", eduId, DifficultyLevel.Hard, 120, 45, "Sektöründeki derin teknik bilgi birikimini ve literatür takibini güçlendirmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "focus_loss" }, new() { "expert_knowledge" }, new() { "makale", "teknik" }));
            tasks.Add(CreateTask("Yeni Kelime Ezberleme", "Öğrenmekte olduğun yabancı dilde en az 5 yeni kelimeyi cümle içinde kullan.", eduId, DifficultyLevel.Easy, 50, 15, "Yabancı dil dağarcığını düzenli pratikle kalıcı kılmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Night }, new() { "memory_lapse" }, new() { "language_skills" }, new() { "dil", "kelime" }));
            tasks.Add(CreateTask("Bilimsel Podcast", "Bilim, felsefe veya tarih konulu en az 20 dakikalık bir podcast dinle.", eduId, DifficultyLevel.Easy, 45, 20, "Günlük rutin işleri yaparken (yolculuk, temizlik vb.) zihnini faydalı bilgilerle beslemek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "empty_time" }, new() { "general_knowledge" }, new() { "podcast", "bilim" }));
            tasks.Add(CreateTask("Hızlı Okuma Alıştırması", "Seçtiğin bir metin üzerinde 10 dakika boyunca hızlı okuma ve blok okuma yap.", eduId, DifficultyLevel.Easy, 50, 10, "Okuma hızını ve odaklanma yeteneğini artırmak için.", new() { DayTimeSlot.Morning }, new() { "slow_reading" }, new() { "efficiency" }, new() { "okuma", "pratik" }));
            tasks.Add(CreateTask("Felsefi Okuma", "Temel felsefe metinlerinden en az 5 sayfa oku ve ana fikrini düşün.", eduId, DifficultyLevel.Medium, 75, 20, "Eleştirel düşünme ve sorgulama yetilerini harekete geçirmek için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "shallow_thinking" }, new() { "critical_thinking" }, new() { "felsefe", "okuma" }));
            tasks.Add(CreateTask("Tarihte Bugün Araştırması", "Tarihte bugün gerçekleşmiş önemli bir olayı derinlemesine araştır.", eduId, DifficultyLevel.Easy, 40, 15, "Genel kültür seviyeni yükseltmek ve tarih bilincini tazelemek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "lack_of_culture" }, new() { "general_knowledge" }, new() { "tarih", "arastirma" }));
            tasks.Add(CreateTask("Çalışma Kartları (Flashcard)", "Öğrendiğin yeni bilgileri Anki veya fiziksel kartlara yazıp tekrar yap.", eduId, DifficultyLevel.Easy, 55, 15, "Aktif hatırlama yöntemiyle bilgileri uzun süreli belleğe kaydetmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "forgetting" }, new() { "retention" }, new() { "flashcard", "hafiza" }));
            tasks.Add(CreateTask("Belgesel İzleme", "Doğa, uzay ya da tarih konulu en az 30 dakikalık bir belgesel izle.", eduId, DifficultyLevel.Medium, 70, 35, "Görsel hafıza yoluyla yeni bilgiler edinmek ve ufkunu genişletmek için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "screentime_waste" }, new() { "general_knowledge" }, new() { "belgesel", "gorsel" }));
            tasks.Add(CreateTask("Yazım Kuralları Kontrolü", "Son yazdığın bir rapor veya makaledeki yazım/dilbilgisi hatalarını denetle.", eduId, DifficultyLevel.Easy, 40, 10, "Yazılı iletişim kaliteni ve profesyonelliğini en üst düzeye çıkarmak için.", new() { DayTimeSlot.Afternoon }, new() { "carelessness" }, new() { "communication" }, new() { "yazım", "kontrol" }));
            tasks.Add(CreateTask("Matematik/Mantık Sorusu", "Zihnini çalıştırmak için en az 3 adet zorlayıcı mantık veya matematik sorusu çöz.", eduId, DifficultyLevel.Medium, 80, 25, "Problem çözme ve analitik düşünme yeteneklerini zinde tutmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon }, new() { "brain_fog" }, new() { "analytical_skills" }, new() { "matematik", "mantik" }));
            tasks.Add(CreateTask("Sesli Kitap Dinleme", "İşe veya okula giderken en az 20 dakika sesli kitap dinle.", eduId, DifficultyLevel.Easy, 45, 20, "Gündelik seyahat sürelerini aktif birer öğrenme seansına dönüştürmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "commute_waste" }, new() { "intellectual_growth" }, new() { "sesli_kitap", "dinleme" }));
            tasks.Add(CreateTask("Akademik Bülten Takibi", "İlgi alanındaki akademik bültenleri tarayıp önemli gelişmeleri oku.", eduId, DifficultyLevel.Medium, 75, 25, "Bilim dünyasındaki en güncel keşifleri ve makaleleri kaçırmamak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "outdated_info" }, new() { "expert_knowledge" }, new() { "bulten", "akademik" }));
            tasks.Add(CreateTask("Kodlama Egzersizi", "LeetCode veya HackerRank benzeri bir platformda en az 1 algoritma sorusu çöz.", eduId, DifficultyLevel.Hard, 110, 40, "Algoritmik zekanı geliştirmek ve kodlama becerilerini pratik tutmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "lack_of_practice" }, new() { "coding_skills" }, new() { "kodlama", "algoritma" }));
            tasks.Add(CreateTask("Klavyede On Parmak Pratiği", "10 dakika boyunca hızlı yazma testi yap ve doğruluğunu artır.", eduId, DifficultyLevel.Easy, 35, 10, "Bilgisayar başında geçirdiğin sürede yazım hızını artırarak zaman kazanmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "slow_typing" }, new() { "productivity" }, new() { "klavye", "pratik" }));
            tasks.Add(CreateTask("Yeni Bir Konsepti Açıklama", "Feynman tekniğini kullanarak yeni öğrendiğin zor bir konsepti basitçe yazılı açıkla.", eduId, DifficultyLevel.Medium, 90, 30, "Bir konuyu gerçekten öğrenip öğrenmediğini test etmenin en iyi yoludur.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "superficial_learning" }, new() { "mastery" }, new() { "feynman", "ogrenme" }));
            tasks.Add(CreateTask("Ted Talk İzleme", "Ufuk açıcı bir konuda en az 1 TED konuşması izle ve not al.", eduId, DifficultyLevel.Easy, 45, 15, "Yenilikçi fikirlerden ilham almak ve sunum becerilerini gözlemlemek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "lack_of_inspiration" }, new() { "motivation" }, new() { "ted", "izleme" }));
            tasks.Add(CreateTask("Verimlilik Metodu Denemesi", "Pomodoro veya Kanban gibi yeni bir çalışma metodunu bugün 1 saat boyunca uygula.", eduId, DifficultyLevel.Medium, 70, 60, "Çalışma seanslarında odak süreni artırmak ve ertelemeyi önlemek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "procrastination" }, new() { "productivity" }, new() { "pomodoro", "verimlilik" }));
            tasks.Add(CreateTask("Coğrafya Bilgisi Geliştirme", "Dünya haritası üzerinden rastgele seçtiğin 5 ülkenin başkentini ve temel özelliklerini öğren.", eduId, DifficultyLevel.Easy, 40, 15, "Dünya vizyonunu geliştirmek ve genel coğrafi kültürünü pekiştirmek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "narrow_vision" }, new() { "general_knowledge" }, new() { "cografya", "harita" }));
            tasks.Add(CreateTask("Şiir Tahlili", "Klasik veya çağdaş bir şiiri okuyup şairin anlatmak istediği duygu dünyasını incele.", eduId, DifficultyLevel.Easy, 45, 15, "Sanatsal duyarlılığı geliştirmek ve kelimelerin gücünü hissetmek için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "lack_of_empathy" }, new() { "art_appreciation" }, new() { "siir", "tahlil" }));
            tasks.Add(CreateTask("Hukuk Okuryazarlığı", "Tüketici hakları veya iş kanunundaki temel haklarından birini okuyup öğren.", eduId, DifficultyLevel.Medium, 75, 20, "Gündelik hayattaki ve iş dünyasındaki yasal haklarını bilerek bilinçli kararlar almak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "legal_ignorance" }, new() { "life_skills" }, new() { "hukuk", "haklar" }));
            tasks.Add(CreateTask("Eğitim Planı Oluşturma", "Önümüzdeki 1 ay boyunca öğrenmek istediğin yeni bir konunun müfredatını çıkar.", eduId, DifficultyLevel.Medium, 85, 30, "Öğrenme sürecini yapılandırarak hedeflerine daha disiplinli ulaşman için.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "disorganization" }, new() { "career_advancement" }, new() { "mufredat", "plan" }));
            tasks.Add(CreateTask("Yabancı Dilde Dizi/Film", "Yabancı dildeki bir dizi veya filmin 1 bölümünü altyazısız izlemeye çalış.", eduId, DifficultyLevel.Hard, 100, 40, "Dinleme (listening) yeteneğini gerçekçi diyaloglarla en üst düzeye çekmek için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "poor_listening" }, new() { "language_skills" }, new() { "yabancidil", "film" }));
            tasks.Add(CreateTask("Seminer Notlarını Düzenleme", "Daha önce katıldığın bir seminer veya webinarda aldığın karmaşık notları temiz bir belgeye aktar.", eduId, DifficultyLevel.Medium, 65, 25, "Dağınık verileri bilgiye dönüştürerek kalıcılığını sağlamak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "disorder" }, new() { "organization" }, new() { "seminer", "notlar" }));
            tasks.Add(CreateTask("Sanat Tarihi Okuması", "Önemli bir sanat akımını ve en önemli 3 temsilcisini araştır.", eduId, DifficultyLevel.Easy, 40, 15, "Sanat dünyasına dair entelektüel bakış açını derinleştirmek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "lack_of_art" }, new() { "general_knowledge" }, new() { "sanattarihi", "kultur" }));

            
            var healthId = saglik.Id;
            tasks.Add(CreateTask("Sabah Esnemesi", "Yataktan kalktıktan sonra kaslarını gevşetmek için 10 dakika hafif esneme hareketleri yap.", healthId, DifficultyLevel.Easy, 35, 10, "Güne enerjik başlamak ve gece boyu kasılan vücudu rahatlatmak için.", new() { DayTimeSlot.Morning }, new() { "stiffness" }, new() { "flexibility" }, new() { "esneme", "sabah" }));
            tasks.Add(CreateTask("2 Litre Su Tüketimi", "Gün boyunca su şişeni yanından ayırmayarak en az 2 litre su tüket.", healthId, DifficultyLevel.Easy, 50, 5, "Vücut hidrasyonunu ideal düzeyde tutarak baş ağrılarını ve halsizliği önlemek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon, DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "dehydration" }, new() { "hydration" }, new() { "su", "detoks" }));
            tasks.Add(CreateTask("Masa Başı Egzersizi", "Çalışırken her saat başı kalkıp 2 dakika boyunca omuz, boyun ve sırt egzersizleri yap.", healthId, DifficultyLevel.Easy, 40, 10, "Hareketsiz yaşam tarzının omurgaya verdiği zararları engellemek amacıyla önerilmiştir.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "back_pain" }, new() { "posture" }, new() { "egzersiz", "ofis" }));
            tasks.Add(CreateTask("Dengeli Ev Yemeği", "İşlenmiş gıdalardan kaçınarak taze sebze ve protein içeren dengeli bir öğün hazırla.", healthId, DifficultyLevel.Medium, 75, 30, "Vücuda temiz enerji verip bağışıklık sistemini desteklemek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "junk_food" }, new() { "nutrition" }, new() { "yemek", "sagliklibeslenme" }));
            tasks.Add(CreateTask("Şekersiz Gün Çelıncı", "Rafine şeker içeren hiçbir yiyecek ve içecek tüketme.", healthId, DifficultyLevel.Medium, 90, 5, "Şeker dalgalanmalarını önleyerek gün boyu dengeli bir enerji seviyesine sahip olmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon, DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "sugar_addiction" }, new() { "nutrition" }, new() { "sekersiz", "diyet" }));
            tasks.Add(CreateTask("Göz Dinlendirme Egzersizi", "20-20-20 kuralını uygula: Her 20 dakikada bir 20 saniye boyunca 6 metre uzağa bak.", healthId, DifficultyLevel.Easy, 30, 5, "Dijital ekran yorgunluğunu azaltmak ve göz kuruluğunu önlemek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "eye_strain" }, new() { "wellbeing" }, new() { "goz", "dinlenme" }));
            tasks.Add(CreateTask("Açık Havada Yürüyüş", "Öğle arasında veya akşam üzeri temiz havada en az 30 dakika tempolu yürü.", healthId, DifficultyLevel.Medium, 60, 30, "Kardiyovasküler sağlığı güçlendirmek ve zihni boşaltmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "sedentary" }, new() { "cardio" }, new() { "yuruyus", "temizhava" }));
            tasks.Add(CreateTask("Yeşil Çay/Bitki Çayı", "Kafeinli kahve tüketimini azaltıp yerine 1 fincan bitki çayı iç.", healthId, DifficultyLevel.Easy, 35, 10, "Vücudu antioksidanlarla desteklemek ve sakinleşmek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "caffeine_crash" }, new() { "detox" }, new() { "cay", "bitkisel" }));
            tasks.Add(CreateTask("Uyku Öncesi Ekran Detoksu", "Uyumadan en az 45 dakika önce telefon, tablet ve televizyon ekranlarını kapat.", healthId, DifficultyLevel.Medium, 80, 45, "Melatonin hormonu salgılanmasını artırarak uyku kalitesini yükseltmek için.", new() { DayTimeSlot.Night }, new() { "insomnia" }, new() { "sleep_quality" }, new() { "uyku", "detoks" }));
            tasks.Add(CreateTask("Ev Yoğurdu veya Fermente Gıda", "Probiyotik desteği için bugün en az bir öğününde fermente gıda tüket.", healthId, DifficultyLevel.Easy, 40, 10, "Bağırsak sağlığını (ikinci beyin) ve sindirim florasını güçlendirmek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "poor_digestion" }, new() { "gut_health" }, new() { "probiyotik", "fermente" }));
            tasks.Add(CreateTask("Postür (Duruş) Kontrolü", "Gün boyu oturduğun sandalyede dik durduğundan emin ol, gerekirse destek kullan.", healthId, DifficultyLevel.Easy, 35, 5, "Kronik sırt ve boyun ağrılarını önleyici alışkanlık kazandırmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "slouching" }, new() { "posture" }, new() { "durus", "dik" }));
            tasks.Add(CreateTask("Ev Temizliği ile Kalori Yakma", "Evini detaylıca süpürüp silerek hem hijyen sağla hem de hareket et.", healthId, DifficultyLevel.Medium, 65, 40, "Ev temizliği yaparken nabzı yükselterek kalori harcamak ve düzen yaratmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "messiness" }, new() { "physical_activity" }, new() { "temizlik", "kalori" }));
            tasks.Add(CreateTask("Derin Nefes Seansı", "Sessiz bir köşede 5 dakika boyunca 4-7-8 nefes tekniğini uygula.", healthId, DifficultyLevel.Easy, 45, 5, "Sempatik sinir sistemini yatıştırarak stres ve anksiyeteyi düşürmek amacıyla.", new() { DayTimeSlot.Morning, DayTimeSlot.Night }, new() { "anxiety" }, new() { "stress_relief" }, new() { "nefes", "meditasyon" }));
            tasks.Add(CreateTask("Kardiyo Antrenmanı", "Evde veya spor salonunda nabzını yükseltecek 20 dakikalık HIIT antrenmanı yap.", healthId, DifficultyLevel.Hard, 100, 20, "Yağ yakımını hızlandırmak ve akciğer kapasitesini artırmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "poor_stamina" }, new() { "fitness" }, new() { "hiit", "kardiyo" }));
            tasks.Add(CreateTask("Meyve Tabağı Hazırlama", "C vitamini ve lif deposu olan taze meyvelerden oluşan bir tabak hazırla.", healthId, DifficultyLevel.Easy, 40, 10, "Mikro besin ve vitamin ihtiyacını doğal yollarla karşılamak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "poor_nutrition" }, new() { "vitamin" }, new() { "meyve", "bagisiklik" }));
            tasks.Add(CreateTask("Tuz Tüketimi Azaltma", "Bugün yemeklerine ekstra tuz ekleme ve sodyumu düşük gıdalar seç.", healthId, DifficultyLevel.Easy, 35, 5, "Tansiyon dengesini korumak ve ödem oluşumunu azaltmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "high_blood_pressure" }, new() { "cardiovascular" }, new() { "tuzsuz", "saglik" }));
            tasks.Add(CreateTask("Dış Fırçalama ve Diş İpi", "Ağız sağlığı için dişlerini fırçaladıktan sonra mutlaka diş ipi kullan.", healthId, DifficultyLevel.Easy, 40, 5, "Diş eti hastalıklarını ve plak oluşumunu profesyonelce engellemek için.", new() { DayTimeSlot.Night }, new() { "dental_decay" }, new() { "hygiene" }, new() { "dis", "hijyen" }));
            tasks.Add(CreateTask("Medikal Check-up Randevusu", "Ertelediğin doktor, diş veya göz randevularından biri için randevu al.", healthId, DifficultyLevel.Easy, 50, 10, "Erken teşhis ve önleyici sağlık tedbirlerini aksatmamak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon }, new() { "procrastination" }, new() { "prevention" }, new() { "doktor", "randevu" }));
            tasks.Add(CreateTask("Soğuk Duş Deneyimi", "Kan dolaşımını hızlandırmak için duşun son 1 dakikasını soğuk suyla tamamla.", healthId, DifficultyLevel.Medium, 70, 10, "Vücut direncini artırmak ve uyanıklık hissi sağlamak için.", new() { DayTimeSlot.Morning }, new() { "lethargy" }, new() { "immunity" }, new() { "sogukdus", "bagisiklik" }));
            tasks.Add(CreateTask("Kuruyemiş Porsiyon Kontrolü", "Sağlıklı yağlar almak için bir avuç içi kadar çiğ kuruyemiş tüket.", healthId, DifficultyLevel.Easy, 30, 5, "Hücre yenilenmesi için gerekli olan omega ve sağlıklı yağları almak için.", new() { DayTimeSlot.Afternoon }, new() { "craving" }, new() { "healthy_fats" }, new() { "kuruyemis", "snack" }));
            tasks.Add(CreateTask("Sırt ve Bel Güçlendirme", "Plank ve köprü egzersizleriyle merkez (core) kaslarını 10 dakika çalıştır.", healthId, DifficultyLevel.Medium, 60, 10, "Vücut dengesini artırmak ve bel fıtığı risklerini azaltmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "weak_core" }, new() { "strength" }, new() { "plank", "bel" }));
            tasks.Add(CreateTask("Doğal Işık Alma", "Sabah uyandıktan sonra ilk 1 saat içinde güneş ışığına en az 10 dakika maruz kal.", healthId, DifficultyLevel.Easy, 40, 10, "Sirkadiyen ritmini (biyolojik saat) ayarlamak ve D vitamini sentezlemek için.", new() { DayTimeSlot.Morning }, new() { "low_mood" }, new() { "circadian_rhythm" }, new() { "gunes", "dvitamini" }));
            tasks.Add(CreateTask("Yemekleri Yavaş Çiğneme", "Sindirim sistemini rahatlatmak için bugün her lokmanı en az 20 kez çiğneyerek ye.", healthId, DifficultyLevel.Easy, 45, 20, "Doyma hissini zamanında hissetmek ve şişkinlik problemlerini azaltmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "indigestion" }, new() { "mindful_eating" }, new() { "cigneme", "sindirim" }));
            tasks.Add(CreateTask("Bitki Temelli Gün", "Bugün tüm öğünlerinde hayvansal gıda tüketimini minimuma indir, sebze ağırlıklı beslen.", healthId, DifficultyLevel.Hard, 110, 45, "Sindirim sistemini dinlendirmek ve kolesterolü dengelemek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "heavy_stomach" }, new() { "vegan_friendly" }, new() { "sebze", "detoks" }));
            tasks.Add(CreateTask("Zihinsel Rahatlama (Güdümlü Meditasyon)", "10 dakikalık güdümlü uyku/stres meditasyonu yap.", healthId, DifficultyLevel.Easy, 50, 10, "Günün mental yorgunluğunu silmek ve derin dinlenme sağlamak amacıyla.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "overthinking" }, new() { "mindfulness" }, new() { "meditasyon", "stres" }));
            tasks.Add(CreateTask("10.000 Adım Hedefi", "Gün bitmeden adım sayarında 10.000 rakamına ulaşmak için ekstra hareket et.", healthId, DifficultyLevel.Hard, 130, 60, "Günlük aktif kalori yakımı ve bacak kaslarının güçlendirilmesi için harika bir hedeftir.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "inactivity" }, new() { "cardio" }, new() { "yuruyus", "adim" }));

            
            var careerId = kariyer.Id;
            tasks.Add(CreateTask("E-Posta Kutusunu Sıfırlama", "Gereksiz e-postaları sil, okunmamışları yanıtla ve gelen kutunu temizle.", careerId, DifficultyLevel.Easy, 40, 20, "Zihinsel dağınıklığı önlemek ve önemli iş kaçırmamak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "clutter" }, new() { "inbox_zero" }, new() { "eposta", "duzen" }));
            tasks.Add(CreateTask("LinkedIn Güncellemesi", "Profilindeki deneyim, yetenek veya sertifika alanlarından birini güncelle.", careerId, DifficultyLevel.Easy, 50, 15, "Profesyonel ağını güncel tutmak ve yeni fırsatlara hazır olmak için.", new() { DayTimeSlot.Afternoon }, new() { "outdated_profile" }, new() { "personal_brand" }, new() { "linkedin", "kariyer" }));
            tasks.Add(CreateTask("Günlük Plan Hazırlama", "Mesaiye başlamadan önce en önemli 3 önceliğini (MIT) belirle ve yaz.", careerId, DifficultyLevel.Easy, 45, 10, "Günün en değerli saatlerini en önemli işlere ayırarak odaklanmayı sağlamak amacıyla.", new() { DayTimeSlot.Morning }, new() { "unorganized" }, new() { "focus" }, new() { "plan", "oncelik" }));
            tasks.Add(CreateTask("Çalışma Masası Düzeni", "Masandaki tüm dağınıklığı gider, temizle.", careerId, DifficultyLevel.Easy, 35, 10, "Dışsal düzenin içsel odaklanmayı artırdığı bilimsel olarak kanıtlanmıştır.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "distraction" }, new() { "productivity" }, new() { "masa", "temizlik" }));
            tasks.Add(CreateTask("Sektörel Bülten Okuma", "Sektöründeki son trendleri içeren en az bir profesyonel bülteni oku.", careerId, DifficultyLevel.Easy, 45, 15, "Sektörel değişimlerden ve teknolojilerden geri kalmamak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "stagnation" }, new() { "industry_knowledge" }, new() { "bulten", "trend" }));
            tasks.Add(CreateTask("Yeni Bir Araç Keşfi", "İş akışını hızlandırabilecek yeni bir yazılımı veya yapay zeka aracını 15 dakika incele.", careerId, DifficultyLevel.Medium, 70, 20, "Teknolojik yenilikleri iş süreçlerine entegre edip verim sağlamak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "inefficient_workflow" }, new() { "innovation" }, new() { "arac", "ai" }));
            tasks.Add(CreateTask("Özgeçmiş (CV) İnceleme", "Özgeçmişini güncel trendlere göre gözden geçir, eksik anahtar kelimeleri ekle.", careerId, DifficultyLevel.Medium, 75, 30, "Kariyer geçişlerinde veya terfi taleplerinde her an hazır olmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "unpreparedness" }, new() { "career_growth" }, new() { "cv", "resume" }));
            tasks.Add(CreateTask("Meslektaşla Bağlantı Kurma", "LinkedIn üzerinden aynı sektörde çalışan ve tanımadığın 3 kişiye bağlantı isteği gönder.", careerId, DifficultyLevel.Easy, 40, 10, "Profesyonel çevreni genişletmek ve network oluşturmak için.", new() { DayTimeSlot.Afternoon }, new() { "isolation" }, new() { "networking" }, new() { "linkedin", "network" }));
            tasks.Add(CreateTask("Toplantı Notu Çıkarma", "Katıldığın son toplantının kararlarını ve aksiyon maddelerini netleştirip ekiple paylaş.", careerId, DifficultyLevel.Easy, 50, 15, "Ekip içi hizalanmayı sağlamak ve sorumlulukları netleştirmek amacıyla.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "miscommunication" }, new() { "teamwork" }, new() { "toplanti", "notlar" }));
            tasks.Add(CreateTask("Portfolyo Güncelleme", "Yaptığın son başarılı projeyi portfolyona veya GitHub/Behance hesabına ekle.", careerId, DifficultyLevel.Medium, 80, 40, "Yaptığın işleri somut olarak kanıtlamak ve görünür kılmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "low_visibility" }, new() { "personal_brand" }, new() { "portfolyo", "github" }));
            tasks.Add(CreateTask("Zaman Takibi (Time Tracking)", "Bugün çalıştığın saatleri ve hangi işe ne kadar süre ayırdığını detaylıca kaydet.", careerId, DifficultyLevel.Easy, 45, 10, "Hangi görevlerin vaktini çaldığını görerek zaman yönetimini optimize etmek amacıyla.", new() { DayTimeSlot.Evening }, new() { "time_wasting" }, new() { "time_management" }, new() { "zaman", "analiz" }));
            tasks.Add(CreateTask("Zor Bir Görevi Bitirme", "Günlerdir ertelediğin en karmaşık kariyer görevine odaklanıp onu tamamla.", careerId, DifficultyLevel.Hard, 150, 60, "Kurbağayı Ye! (Eat the Frog) kuralıyla en zor işi ilk bitirerek zihni rahatlatmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "procrastination" }, new() { "milestone_achievement" }, new() { "odak", "zor" }));
            tasks.Add(CreateTask("Geri Bildirim İsteme", "Yöneticinden veya çalışma arkadaşından son yaptığın iş hakkında yapıcı geri bildirim talep et.", careerId, DifficultyLevel.Medium, 70, 15, "Kör noktalarını keşfedip iş kalitesini sürekli yukarı çekmek için.", new() { DayTimeSlot.Afternoon }, new() { "lack_of_growth" }, new() { "professional_improvement" }, new() { "feedback", "gelisim" }));
            tasks.Add(CreateTask("Topluluk Önünde Konuşma Pratiği", "Ayna karşısında veya kameraya karşı 5 dakikalık bir iş sunumu provası yap.", careerId, DifficultyLevel.Medium, 65, 15, "Hitabet gücünü artırarak heyecanını kontrol altına almak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "stage_fright" }, new() { "presentation_skills" }, new() { "sunum", "hitabet" }));
            tasks.Add(CreateTask("Sektörel Kitap/Makale", "Profesyonel gelişimine katkı sağlayacak bir kitaptan en az 10 sayfa oku.", careerId, DifficultyLevel.Medium, 75, 20, "Teorik altyapını zenginleştirip işteki problem çözme ufkunu genişletmek için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "shallow_thinking" }, new() { "industry_knowledge" }, new() { "kitap", "sektorel" }));
            tasks.Add(CreateTask("İletişim Becerilerini Geliştirme", "E-yazışmalarda daha profesyonel ve diplomatik bir dil kullanmak için şablonları incele.", careerId, DifficultyLevel.Easy, 40, 15, "Yazılı iletişimde net, saygın ve ikna edici olmak kariyerde kapıları açar.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "misunderstanding" }, new() { "communication" }, new() { "yazisma", "iletisim" }));
            tasks.Add(CreateTask("Yeni Bir Metot Öğrenme", "İşinde kullanabileceğin yeni bir formül, tasarım deseni veya yönetim tekniği araştır.", careerId, DifficultyLevel.Medium, 80, 30, "Mevcut işleri daha kısa sürede ve daha az hatayla çözebilecek yolları aramak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "inefficiency" }, new() { "technical_skills" }, new() { "metot", "arastirma" }));
            tasks.Add(CreateTask("Kariyer Hedeflerini Yazma", "Önümüzdeki 1 yıl ve 5 yıl için kariyerinde ulaşmak istediğin pozisyonları ve planları yaz.", careerId, DifficultyLevel.Easy, 55, 20, "Hedef belirlemek, rotanı çizmeni sağlar ve motivasyonunu diri tutar.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "purposelessness" }, new() { "career_goals" }, new() { "hedef", "vizyon" }));
            tasks.Add(CreateTask("Mentor Arama", "Kariyerinde sana yol gösterebilecek potansiyel mentor adaylarını belirle.", careerId, DifficultyLevel.Medium, 70, 30, "Başkalarının tecrübelerinden yararlanarak hata yapma payını azaltmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "lack_of_direction" }, new() { "career_networking" }, new() { "mentor", "rehber" }));
            tasks.Add(CreateTask("İş Başvurusu İnceleme", "Hedeflediğin şirketlerin açık pozisyon ilanlarını inceleyerek aradıkları yetkinlikleri listele.", careerId, DifficultyLevel.Easy, 45, 15, "Piyasa taleplerini görerek kendini hangi alanlarda eğitmen gerektiğini anlamak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "out_of_sync" }, new() { "market_knowledge" }, new() { "ilan", "yetkinlik" }));
            tasks.Add(CreateTask("Kişisel Marka Çalışması", "Uzman olduğun bir konuda LinkedIn veya blogunda paylaşmak üzere kısa bir bilgilendirici metin yaz.", careerId, DifficultyLevel.Medium, 85, 30, "Sektörde bilinen ve fikirlerine danışılan bir profesyonel olma yolunda adım atmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "anonymity" }, new() { "thought_leadership" }, new() { "blog", "linkedin" }));
            tasks.Add(CreateTask("Proje Yönetim Panosu Güncelleme", "Trello, Jira veya Notion panondaki kartların durumlarını güncelle ve arşivle.", careerId, DifficultyLevel.Easy, 40, 15, "İşlerin takibini kolaylaştırarak son teslim tarihlerine uymak amacıyla.", new() { DayTimeSlot.Evening }, new() { "disorganization" }, new() { "agile" }, new() { "pano", "notion" }));
            tasks.Add(CreateTask("Hızlı Klavye Yazımı", "İş yazışmalarını hızlandırmak için klavye kısayollarını öğren ve kullan.", careerId, DifficultyLevel.Easy, 30, 10, "Zaman tasarrufu ve verimli işletim sistemi kullanımı için ufak pratikler değerlidir.", new() { DayTimeSlot.Morning }, new() { "slow_operation" }, new() { "efficiency" }, new() { "klavye", "kisayol" }));
            tasks.Add(CreateTask("Finansal Planlama (Bütçe)", "Kariyer gelirlerini ve harcamalarını listeleyip aylık tasarruf oranını hesapla.", careerId, DifficultyLevel.Medium, 75, 30, "Finansal özgürlük ve kariyer kararlarında esneklik kazanmak için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "poor_savings" }, new() { "finance" }, new() { "butce", "finans" }));
            tasks.Add(CreateTask("Sertifika Programı Araştırması", "Sektöründe geçerliliği yüksek olan profesyonel bir sertifika programını araştır.", careerId, DifficultyLevel.Easy, 50, 20, "Uzmanlığını uluslararası geçerliliği olan belgelerle taçlandırmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "undocumented_skills" }, new() { "credentials" }, new() { "sertifika", "arastirma" }));
            tasks.Add(CreateTask("Networking Buluşması", "Eski bir iş arkadaşınla kahve içmek veya online görüşmek için plan yap.", careerId, DifficultyLevel.Medium, 80, 15, "Bağlantıları koparmamak ve gelecekteki olası iş birliklerine kapı açmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "weak_network" }, new() { "social_capital" }, new() { "networking", "kahve" }));

            
            var socialId = sosyal.Id;
            tasks.Add(CreateTask("Geri Dönüşüm Ayrıştırma", "Evindeki kağıt, plastik ve cam atıkları ayrı poşetlerde toplayıp geri dönüşüm kutusuna at.", socialId, DifficultyLevel.Easy, 45, 10, "Doğal kaynakların korunması ve çöp miktarının azaltılması için.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "wastefulness" }, new() { "ecology" }, new() { "geridonusum", "cevre" }));
            tasks.Add(CreateTask("Su/Yiyecek Tasarrufu", "Bugün tabakta yemek bırakmayarak ve gereksiz su akıtmayarak kaynak tasarrufu yap.", socialId, DifficultyLevel.Easy, 40, 5, "Dünyadaki kıtlık ve su stresiyle mücadelede bireysel sorumluluk almak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "extravagance" }, new() { "sustainability" }, new() { "tasarruf", "su" }));
            tasks.Add(CreateTask("Sokak Hayvanlarını Besleme", "Kapının önüne veya sokaktaki bir köşeye bir kap temiz su ve kuru mama bırak.", socialId, DifficultyLevel.Easy, 50, 10, "Sokaktaki canların yaşam mücadelesine merhamet ve sevgiyle ortak olmak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Night }, new() { "indifference" }, new() { "animal_welfare" }, new() { "hayvanlar", "mama" }));
            tasks.Add(CreateTask("Gereksiz Eşya Bağışı", "Kullanmadığın ama temiz durumda olan giysi veya eşyaları bağışlamak için ayır.", socialId, DifficultyLevel.Medium, 70, 30, "Minimalizm anlayışıyla evde yer açmak ve ihtiyaç sahiplerini mutlu etmek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "hoarding" }, new() { "charity" }, new() { "bagis", "giysi" }));
            tasks.Add(CreateTask("Dijital Karbon Ayak İzi Azaltma", "Gereksiz e-postaları silerek ve aboneliklerden çıkarak sunucu yükünü azalt.", socialId, DifficultyLevel.Easy, 35, 15, "Veri merkezlerinin tükettiği muazzam elektriği ve dolaylı karbon salınımını düşürmek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "digital_clutter" }, new() { "carbon_footprint" }, new() { "dijital", "karbon" }));
            tasks.Add(CreateTask("Enerji Tasarrufu", "Bugün kullanmadığın tüm elektronik aletlerin fişini çek ve gereksiz lambaları kapat.", socialId, DifficultyLevel.Easy, 40, 5, "Fosil yakıt tüketimini azaltıp doğayı korurken aile/ülke ekonomisine destek olmak için.", new() { DayTimeSlot.Night }, new() { "negligence" }, new() { "energy_efficiency" }, new() { "enerji", "tasarruf" }));
            tasks.Add(CreateTask("Fidan Bağışı", "Geleceğe nefes olması için en az 1 fidan bağışla.", socialId, DifficultyLevel.Easy, 60, 10, "İklim değişikliği ve erozyonla mücadeleye somut bir katkıda bulunmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "carbon_emission" }, new() { "forestation" }, new() { "fidan", "tema" }));
            tasks.Add(CreateTask("Toplu Taşıma veya Yürüyüş", "Bugün özel aracın yerine toplu taşıma kullan veya kısa mesafeleri yürü.", socialId, DifficultyLevel.Medium, 65, 30, "Kentsel hava kirliliğini ve trafik yoğunluğunu azaltmak amacıyla.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "excess_driving" }, new() { "green_transport" }, new() { "otobus", "yuruyus" }));
            tasks.Add(CreateTask("Yerel Esnafa Destek", "Alışverişini büyük zincir marketler yerine mahalledeki yerel esnaftan yap.", socialId, DifficultyLevel.Easy, 40, 15, "Mahalle kültürünü korumak ve yerel mikro ekonomiyi canlı tutmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "corporatization" }, new() { "local_economy" }, new() { "esnaf", "yerel" }));
            tasks.Add(CreateTask("Kitap Bağışı Hazırlığı", "Okuduğun ve ihtiyacın olmayan kitapları bir köy okuluna göndermek üzere kutula.", socialId, DifficultyLevel.Medium, 80, 45, "Bilginin paylaşarak çoğalmasına ve çocukların eğitimine destek olmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "selfishness" }, new() { "education_support" }, new() { "kitap", "bagis" }));
            tasks.Add(CreateTask("Çevre Temizliği", "Yürüyüş yaparken gördüğün plastik ve çöpleri toplayıp en yakın çöp kutusuna at.", socialId, DifficultyLevel.Medium, 75, 20, "Çevredeki kirliliğin mikroplastiklere dönüşüp doğaya karışmasını önlemek için.", new() { DayTimeSlot.Afternoon }, new() { "passive_behavior" }, new() { "clean_nature" }, new() { "cop", "temizlik" }));
            tasks.Add(CreateTask("Sosyal Sorumluluk Derneği Araştırması", "İlgini çeken bir STK'nın gönüllülük şartlarını oku.", socialId, DifficultyLevel.Easy, 45, 15, "Toplumsal sorunlara çözüm üreten toplulukların bir parçası olma adımı atmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "isolation" }, new() { "active_citizenship" }, new() { "stk", "gonulluluk" }));
            tasks.Add(CreateTask("Kan Bağışı Bilgilendirmesi", "Kızılay kan bağışı noktalarını kontrol et ve uygun bir güne takviminde yer ayır.", socialId, DifficultyLevel.Easy, 50, 10, "Kan bağışının bir insanın hayatını kurtarabileceği bilinciyle hareket etmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon }, new() { "unawareness" }, new() { "health_advocacy" }, new() { "kan", "bagis" }));
            tasks.Add(CreateTask("Plastik Torba Kullanmama", "Bugün alışverişe giderken yanında bez çanta götür, plastik poşet satın alma.", socialId, DifficultyLevel.Easy, 40, 5, "Doğada yüzyıllarca yok olmayan plastik atık salınımını sıfırlamak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "plastic_pollution" }, new() { "eco_friendly" }, new() { "bezcanta", "plastiksiz" }));
            tasks.Add(CreateTask("Su Şişesi Taşıma", "Tek kullanımlık pet şişeler yerine termos veya matara kullanmaya başla.", socialId, DifficultyLevel.Easy, 45, 5, "Plastik tüketimini azaltıp sağlıklı malzemelerle su tüketmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon }, new() { "single_use_plastics" }, new() { "sustainability" }, new() { "termos", "matara" }));
            tasks.Add(CreateTask("Yaşlı Ziyareti veya Arama", "Yalnız yaşayan yaşlı bir yakını telefonla ara ya da ziyaret et.", socialId, DifficultyLevel.Medium, 75, 20, "Yaşlılarımızın yalnızlık hissini hafifletmek ve vefa borcunu ödemek amacıyla.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "neglect" }, new() { "social_cohesion" }, new() { "vefa", "ziyaret" }));
            tasks.Add(CreateTask("Bilinçli Tüketim", "Bugün satın alacağın bir ürünün çevre dostu veya adil ticaret sertifikalı olmasına dikkat et.", socialId, DifficultyLevel.Easy, 40, 15, "Üreticinin ve doğanın hakkını gözeten sürdürülebilir pazarlara destek olmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "blind_consumerism" }, new() { "ethical_trade" }, new() { "tuketim", "etik" }));
            tasks.Add(CreateTask("Çocuk Esirgeme Kurumu Araştırması", "Devlet korumasındaki çocuklar için yapılabilecek gönüllü faaliyetleri öğren.", socialId, DifficultyLevel.Medium, 70, 30, "Kimsesiz ve dezavantajlı çocukların hayata tutunmasına nasıl destek olabileceğini görmek için.", new() { DayTimeSlot.Afternoon }, new() { "ignorance" }, new() { "child_protection" }, new() { "cocuklar", "gonullu" }));
            tasks.Add(CreateTask("Sürdürülebilir Ulaşım", "Bisiklet sürerek veya yürüyerek bugün en az 3 kilometrelik yolu motorsuz tamamla.", socialId, DifficultyLevel.Hard, 110, 45, "Fosil yakıtsız ulaşımın hem sağlığına hem de dünyaya faydasını deneyimlemek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "lazy_habits" }, new() { "clean_air" }, new() { "bisiklet", "karbon" }));
            tasks.Add(CreateTask("Zorba Karşıtı Duruş", "Sosyal medyada veya günlük hayatta haksızlığa uğrayan birine destek ol.", socialId, DifficultyLevel.Medium, 85, 20, "Toplumsal huzur, eşitlik ve güven ortamının inşasına katkı sağlamak amacıyla.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "bystander_effect" }, new() { "social_justice" }, new() { "destek", "empati" }));
            tasks.Add(CreateTask("Sokak Hayvanı Kulübesi Kontrolü", "Mahalledeki kedi evlerinin durumunu kontrol et, kırık veya ıslak olanları düzelt.", socialId, DifficultyLevel.Easy, 45, 15, "Zorlu hava koşullarında sokak hayvanlarının barınma kalitesini korumak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "passivity" }, new() { "animal_care" }, new() { "kedievi", "kontrol" }));
            tasks.Add(CreateTask("Sıfır Atık Günü", "Bugün ürettiğin çöp miktarını minimumda tutmak için ambalajsız ürünler tercih et.", socialId, DifficultyLevel.Hard, 120, 10, "Sıfır atık felsefesini hayatının merkezine alarak tüketim çılgınlığını sınırlamak için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "excess_packaging" }, new() { "zero_waste" }, new() { "sifiratik", "cevre" }));
            tasks.Add(CreateTask("Organik Atık Değerlendirme", "Meyve ve sebze kabuklarını kompost yapmak üzere ayır veya bitki toprağına karıştır.", socialId, DifficultyLevel.Easy, 40, 15, "Çöp sahalarına giden organik atık yükünü azaltıp toprağı beslemek için.", new() { DayTimeSlot.Evening }, new() { "waste_of_organic" }, new() { "soil_care" }, new() { "kompost", "sebze" }));
            tasks.Add(CreateTask("Hayır Kurumu Kampanyası Paylaşımı", "Güvenilir bir sivil toplum kuruluşunun bağış kampanyasını sosyal medyada paylaş.", socialId, DifficultyLevel.Easy, 35, 10, "Kendi bütçen elvermese bile başkalarının bağış yapabilmesine aracı olmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "low_reach" }, new() { "awareness" }, new() { "stk", "paylasim" }));
            tasks.Add(CreateTask("Çevre Dostu Temizlik Maddesi", "Ev temizliğinde kimyasal yerine sirke, karbonat gibi doğal alternatifleri kullanmayı dene.", socialId, DifficultyLevel.Medium, 60, 20, "Su kaynaklarının kimyasallarla zehirlenmesini ve ev içi hava kalitesinin bozulmasını önlemek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "chemical_exposure" }, new() { "healthy_home" }, new() { "sirke", "dogaltemizlik" }));
            tasks.Add(CreateTask("Engelli Farkındalığı", "Engelli bireylerin erişebilirliği hakkında yazılmış bir kılavuzu oku veya video izle.", socialId, DifficultyLevel.Easy, 40, 15, "Engelsiz bir yaşam tasarımı ve empati duygusunun gelişmesi için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "ableism" }, new() { "inclusion" }, new() { "engelli", "farkindalik" }));

            
            var growthId = kisiselGelisim.Id;
            tasks.Add(CreateTask("Günlük Yazma Seansı", "Günün nasıl geçtiğini, hislerini ve öğrendiklerini içeren en az 1 sayfalık günlük yaz.", growthId, DifficultyLevel.Easy, 45, 15, "Zihinsel netlik kazanmak, duygularını regüle etmek ve anıları saklamak için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "emotional_fog" }, new() { "self_reflection" }, new() { "gunluk", "yazma" }));
            tasks.Add(CreateTask("Şükran Günlüğü", "Hayatında sahip olduğun ve minnettar olduğun 3 şeyi detaylarıyla yaz.", growthId, DifficultyLevel.Easy, 40, 10, "Zihni olumluluklara odaklayarak kaygı ve tatminsizlik seviyelerini düşürmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Night }, new() { "pessimism" }, new() { "gratitude" }, new() { "sukran", "pozitif" }));
            tasks.Add(CreateTask("Güdümlü Meditasyon", "Zihnini sakinleştirmek için 10 dakikalık nefes ve odaklanma meditasyonu yap.", growthId, DifficultyLevel.Easy, 50, 10, "Beyindeki kortizol (stres) hormonunu azaltıp odaklanmayı kolaylaştırmak amacıyla.", new() { DayTimeSlot.Morning, DayTimeSlot.Night }, new() { "restlessness" }, new() { "mindfulness" }, new() { "meditasyon", "nefes" }));
            tasks.Add(CreateTask("Bütçe Analizi", "Bu ayki tüm harcamalarını kategorize et ve bütçe aşımını denetle.", growthId, DifficultyLevel.Medium, 75, 30, "Para harcama disiplini edinerek finansal gelecek kaygısını sonlandırmak için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "impulsive_buying" }, new() { "wealth_building" }, new() { "finans", "butce" }));
            tasks.Add(CreateTask("Geleceğe Mektup", "Gelecekteki kendine (1 yıl sonra) hedeflerini ve o anki duygularını anlatan bir mektup yaz.", growthId, DifficultyLevel.Medium, 80, 30, "Kişisel vizyonunun zaman içerisindeki değişimini izlemek ve motive olmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Night }, new() { "short_term_thinking" }, new() { "life_planning" }, new() { "mektup", "hedef" }));
            tasks.Add(CreateTask("Konfor Alanı Testi", "Normalde çekindiğin bir şeyi yap (örneğin tanımadığın biriyle kısa sohbet başlat).", growthId, DifficultyLevel.Hard, 130, 20, "Kişisel sınırları aşmak ve özgüveni artırmak amacıyla tasarlanmıştır.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "social_anxiety" }, new() { "confidence" }, new() { "ozguven", "konforalani" }));
            tasks.Add(CreateTask("Dijital Detoks", "Bugün 4 saat boyunca sosyal medya uygulamalarına hiç giriş yapma.", growthId, DifficultyLevel.Medium, 85, 240, "Dopamin reseptörlerini dinlendirip odaklanma süreni ve gerçek hayata ilgini artırmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "screen_addiction" }, new() { "focus" }, new() { "detoks", "sosyalmedya" }));
            tasks.Add(CreateTask("Erken Uyanma", "Güne erken başlamak için alarmını her zamankinden 30 dakika erkene kur ve uyan.", growthId, DifficultyLevel.Easy, 45, 30, "Günün geri kalanını daha sakin ve planlı koordine edebilmek için ekstra zaman kazan.", new() { DayTimeSlot.Morning }, new() { "sleeping_in" }, new() { "time_surplus" }, new() { "sabah", "rutin" }));
            tasks.Add(CreateTask("Günün Sözü Üzerine Düşünme", "Ufuk açıcı felsefi bir söz seç ve bu sözün hayatındaki karşılığı üzerine 5 dakika düşün.", growthId, DifficultyLevel.Easy, 35, 5, "Hızlı koşan dünyada 5 dakika durarak derinlik kazanmak ve perspektif edinmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon }, new() { "shallow_thinking" }, new() { "philosophy" }, new() { "dusunce", "felsefe" }));
            tasks.Add(CreateTask("Zaman Yönetimi Analizi", "Günün sonunda vaktini neye harcadığını analiz et, verimsiz saatleri belirle.", growthId, DifficultyLevel.Easy, 45, 10, "Zamanın nereye gittiğini görselleştirerek daha efektif planlama yapmak için.", new() { DayTimeSlot.Evening }, new() { "time_leakage" }, new() { "efficiency" }, new() { "zaman", "analiz" }));
            tasks.Add(CreateTask("Hobi Saati", "Uzun zamandır vakit ayıramadığın hobinle (örneğin pul koleksiyonu) 30 dakika ilgilen.", growthId, DifficultyLevel.Easy, 50, 30, "İş dışındaki ilgi alanlarını besleyerek zihinsel tatmini ve mutluluğu artırmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "workaholism" }, new() { "life_balance" }, new() { "hobi", "eglence" }));
            tasks.Add(CreateTask("Hayır Deme Pratiği", "Kendi sınırlarını korumak adına bugün seni zorlayacak bir isteğe nezaketle 'hayır' de.", growthId, DifficultyLevel.Medium, 70, 5, "İnsanları memnun etme dürtüsü yerine kendi önceliklerini öne almak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "people_pleasing" }, new() { "boundaries" }, new() { "hayir", "sinirlar" }));
            tasks.Add(CreateTask("Kişisel SWOT Analizi", "Güçlü ve zayıf yönlerini listeleyen bir SWOT tablosu yap.", growthId, DifficultyLevel.Medium, 90, 30, "Kendini tarafsızca tanımak ve gelecek kararlarını bu verilere göre almak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "lack_of_self_awareness" }, new() { "strategy" }, new() { "swot", "analiz" }));
            tasks.Add(CreateTask("Yavaşlama Egzersizi", "Yemek yerken, yürürken veya çay içerken acele etmeden, ana odaklanarak hareket et.", growthId, DifficultyLevel.Easy, 40, 15, "Modern çağın acelecilik sendromuna (hurry sickness) karşı dinginliği yaşamak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "hurriedness" }, new() { "mindfulness" }, new() { "mindful", "yavasla" }));
            tasks.Add(CreateTask("Öfke Kontrolü Teknikleri", "Öfke veya stres anında sakinleşmeni sağlayacak 3 psikolojik tekniği araştır.", growthId, DifficultyLevel.Easy, 45, 15, "Duygusal zekanı yükselterek kriz anlarını akıllıca yönetebilmek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "temper" }, new() { "emotional_intelligence" }, new() { "ofke", "psikoloji" }));
            tasks.Add(CreateTask("Girişimcilik Hikayesi", "Başarılı olmuş bir girişimcinin biyografisini veya başarı hikayesini oku.", growthId, DifficultyLevel.Easy, 50, 25, "Hatalardan ders çıkarma ve sıfırdan inşa etme cesaretini aşılamak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "risk_aversion" }, new() { "entrepreneurship" }, new() { "girisim", "basari" }));
            tasks.Add(CreateTask("Kişisel Vizyon Beyanı", "Hayattaki temel amacını ve değerlerini özetleyen 3 cümlelik bir vizyon beyanı yaz.", growthId, DifficultyLevel.Medium, 75, 20, "Hangi kararların senin öz değerlerinle uyumlu olduğunu hızlıca filtreleyebilmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Evening }, new() { "indecision" }, new() { "values" }, new() { "vizyon", "degerler" }));
            tasks.Add(CreateTask("Ev Düzenleme (KonMarie)", "Çekmecelerinden birini aç, artık işine yaramayan şeyleri ayıkla ve düzenle.", growthId, DifficultyLevel.Easy, 40, 20, "Fiziksel alanlardaki sadeleşmenin zihinsel sadeleşmeye katkı sağlaması için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "clutter" }, new() { "minimalism" }, new() { "konmarie", "sadelik" }));
            tasks.Add(CreateTask("Korkularla Yüzleşme", "Seni korkutan durumların listesini yap ve en küçüğü için çözüm planla.", growthId, DifficultyLevel.Medium, 95, 25, "Korkularının üzerine giderek onların senin hayatını yönetmesine izin vermemek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "fear" }, new() { "courage" }, new() { "korku", "plan" }));
            tasks.Add(CreateTask("Empati Egzersizi", "Fikirlerinin uyuşmadığı birinin yerine kendini koyarak onun argümanlarını anlamaya çalış.", growthId, DifficultyLevel.Medium, 60, 15, "Tartışmaları çatışmaya dönüştürmeden, farklı bakış açılarını olgunlukla dinlemek amacıyla.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "intolerance" }, new() { "empathy" }, new() { "empati", "iletisim" }));
            tasks.Add(CreateTask("Yeni Rutin Belirleme", "Güne daha enerjik başlamak için sabahları uygulayabileceğin 3 maddelik yeni rutin oluştur.", growthId, DifficultyLevel.Easy, 50, 15, "Sabah enerjisini ve odağını artırarak günün geri kalanını garantilemek için.", new() { DayTimeSlot.Morning }, new() { "inertia" }, new() { "routines" }, new() { "rutin", "sabah" }));
            tasks.Add(CreateTask("Finansal Okuryazarlık", "Yatırım araçları, borsa veya kişisel finans yönetimi hakkında temel bir makale oku.", growthId, DifficultyLevel.Medium, 80, 20, "Paranın zaman değerini anlayarak birikimlerini enflasyona karşı korumak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "financial_illiteracy" }, new() { "money_management" }, new() { "yatirim", "para" }));
            tasks.Add(CreateTask("Kötü Alışkanlık Takibi", "Kurtulmak istediğin bir alışkanlığın gün içindeki tetikleyicilerini kaydet.", growthId, DifficultyLevel.Easy, 40, 10, "Alışkanlık döngüsünün (ipucu, rutin, ödül) ilk aşamasını teşhis etmek için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "bad_habits" }, new() { "self_control" }, new() { "aliskanlik", "takip" }));
            tasks.Add(CreateTask("Kendini Ödüllendirme", "Bu hafta başardığın zor bir iş için kendine küçük ama anlamlı bir ödül ver.", growthId, DifficultyLevel.Easy, 35, 15, "Başarı hissini pekiştirerek içsel motivasyonu ve beynin dopamin ödül yolunu beslemek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "burnout" }, new() { "self_love" }, new() { "odul", "kutlama" }));
            tasks.Add(CreateTask("Beden Dili Analizi", "İletişimde etkili beden dili kullanımı hakkında 15 dakikalık eğitici bir video izle.", growthId, DifficultyLevel.Easy, 45, 15, "Karşındaki insanlarda güven uyandırmak ve beden dilini doğru okumak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "insecurity" }, new() { "body_language" }, new() { "bedendili", "iletisim" }));
            tasks.Add(CreateTask("Aktif Dinleme", "Bugün bir arkadaşınla konuşurken sözünü kesmeden, sadece onu anlamaya odaklan.", growthId, DifficultyLevel.Easy, 40, 15, "Karşındakine saygı göstermek ve yüzeysel olmayan güçlü bağlar kurmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "selfish_talk" }, new() { "active_listening" }, new() { "dinleme", "empati" }));

            
            var creativeId = yaraticilik.Id;
            tasks.Add(CreateTask("Serbest Karalama (Doodling)", "Boş bir kağıda aklına gelen şekilleri 10 dakika boyunca çiz.", creativeId, DifficultyLevel.Easy, 30, 10, "Zihindeki baskıyı azaltmak ve bilinçdışı yaratıcılığı tetiklemek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "perfectionism" }, new() { "stress_relief" }, new() { "cizim", "karalama" }));
            tasks.Add(CreateTask("Yeni Bir Şarkı Keşfi", "Daha önce hiç dinlemediğin tarzda bir albüm bul ve dinle.", creativeId, DifficultyLevel.Easy, 40, 15, "Müzikal ufkunu genişletmek ve beyinde yeni nöral bağlantılar kurmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "cliche_choices" }, new() { "aesthetic_sense" }, new() { "muzik", "kesif" }));
            tasks.Add(CreateTask("Kısa Hikaye/Şiir Yazımı", "Verilen 3 kelimeyi (bulut, anahtar, kedi) içeren kısa bir hikaye yaz.", creativeId, DifficultyLevel.Medium, 70, 20, "Kelimeleri esnek kullanma yeteneğini ve hayal gücünü geliştirmek amacıyla.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "lack_of_words" }, new() { "creative_writing" }, new() { "yazma", "hikaye" }));
            tasks.Add(CreateTask("Yemek Tarifi Denemesi", "Mutfakta daha önce hiç pişirmediğin, yaratıcı bir tarifi dene.", creativeId, DifficultyLevel.Medium, 80, 45, "Malzemeleri birleştirirken duyularını çalıştırmak ve el becerini geliştirmek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "routine_cooking" }, new() { "culinary_skills" }, new() { "yemek", "mutfak" }));
            tasks.Add(CreateTask("Doğa Fotoğrafçılığı", "Dışarı çıkıp perspektif ve gölge oyunlarını kullanan 5 sanatsal fotoğraf çek.", creativeId, DifficultyLevel.Easy, 45, 20, "Etrafındaki dünyayı sıradan bir gözle değil, estetik bir kadrajla görmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "dull_vision" }, new() { "photography" }, new() { "fotograf", "doga" }));
            tasks.Add(CreateTask("Origami Tasarımı", "Kağıt katlama sanatı (origami) ile bir hayvan veya çiçek figürü yap.", creativeId, DifficultyLevel.Easy, 40, 15, "Dikkat süresini uzatmak ve ince motor becerilerini güçlendirmek için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "impatience" }, new() { "handcraft" }, new() { "origami", "kağıt" }));
            tasks.Add(CreateTask("Zihin Haritası (Mind Mapping)", "Aklındaki yaratıcı bir proje fikrini dallara ayıran renkli bir zihin haritası çiz.", creativeId, DifficultyLevel.Easy, 50, 15, "Karmaşık düşünceleri görsel olarak organize edip fikirler arası bağ kurmak amacıyla.", new() { DayTimeSlot.Morning, DayTimeSlot.Afternoon }, new() { "disorganized_thoughts" }, new() { "ideation" }, new() { "mindmap", "plan" }));
            tasks.Add(CreateTask("Yeni Akor/Melodi Pratiği", "Çaldığın bir enstrümanda daha önce bilmediğin bir akor düzeni çalış.", creativeId, DifficultyLevel.Medium, 75, 30, "Müzikal kas hafızasını zorlayarak gelişimi tetiklemek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "repetitive_practice" }, new() { "instrument_mastery" }, new() { "enstruman", "muzik" }));
            tasks.Add(CreateTask("El Yapımı Hediye", "Bir yakının için tamamen kendi emeğinle oluşturduğun küçük bir kart tasarla.", creativeId, DifficultyLevel.Easy, 50, 20, "Karşı tarafa değer verdiğini hissettirirken kendi sanatsal ruhunu beslemek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "materialism" }, new() { "relationships" }, new() { "hediye", "kart" }));
            tasks.Add(CreateTask("Sergi/Müze Sanal Turu", "Dünyaca ünlü bir sanat müzesini online sanal tur aracılığıyla gez.", creativeId, DifficultyLevel.Easy, 45, 25, "Kültürel mirasa hızlıca erişmek ve görsel ilham toplamak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "lack_of_culture" }, new() { "art_appreciation" }, new() { "muze", "sanat" }));
            tasks.Add(CreateTask("Yaratıcı Blog Yazısı", "İlgi duyduğun bir konuda yayınlanmak üzere 300 kelimelik taslak blog yazısı yaz.", creativeId, DifficultyLevel.Medium, 80, 30, "Kendi fikirlerini ve uzmanlığını dış dünyaya estetik bir dille aktarmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "unclear_expression" }, new() { "blogging" }, new() { "blog", "yazi" }));
            tasks.Add(CreateTask("Renk Kombinasyonları", "Bir tasarım projesinde kullanmak üzere uyumlu 5 renkten oluşan palet oluştur.", creativeId, DifficultyLevel.Easy, 40, 15, "Görsel sanatlarda ve tasarımda en kritik aşama olan renk teorisini pratik etmek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "poor_color_sense" }, new() { "design_skills" }, new() { "renk", "palet" }));
            tasks.Add(CreateTask("Geri Dönüştürülmüş Sanat", "Evdeki atık malzemeleri (karton vb.) kullanarak dekoratif bir eşya yap.", creativeId, DifficultyLevel.Medium, 75, 40, "Atıklara can vererek hem sürdürülebilirliği desteklemek hem de sınırları zorlamak amacıyla.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "wastefulness" }, new() { "eco_art" }, new() { "upcycling", "sanat" }));
            tasks.Add(CreateTask("Ses Kaydı / Günlük", "Aklına gelen melodiyi veya hikaye fikrini unutmamak için ses kaydediciye kaydet.", creativeId, DifficultyLevel.Easy, 35, 5, "Uçucu yaratıcı anları yakalayarak onları daha sonra işlenecek fikirlere dönüştürmek için.", new() { DayTimeSlot.Morning, DayTimeSlot.Night }, new() { "forgetfulness" }, new() { "idea_catch" }, new() { "ses", "kesif" }));
            tasks.Add(CreateTask("Tasarım İncelemesi", "Ödüllü bir web sitesinin arayüz tasarımını inceleyip notlar al.", creativeId, DifficultyLevel.Medium, 70, 20, "Kullanıcı deneyimi (UX) ve modern tasarım trendlerini kavramak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Afternoon }, new() { "outdated_aesthetic" }, new() { "ux_ui" }, new() { "tasarim", "ux" }));
            tasks.Add(CreateTask("Karakter Analizi", "İleride yazabileceğin bir hikaye için ana karakter kartı oluştur.", creativeId, DifficultyLevel.Medium, 80, 25, "Kurgu yazımında insan davranışlarını derinlemesine modellemek ve empatiyi artırmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "flat_writing" }, new() { "fiction" }, new() { "karakter", "yazma" }));
            tasks.Add(CreateTask("Enstrümantal Doğaçlama", "Metronom eşliğinde enstrümanınla 15 dakika doğaçlama çal.", creativeId, DifficultyLevel.Medium, 70, 15, "Notaların ötesine geçerek kendi müziğini hissetmek ve zihni serbest bırakmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "stiff_playing" }, new() { "musical_flow" }, new() { "enstruman", "dogaclama" }));
            tasks.Add(CreateTask("Oda Dekorasyonu Değişimi", "Odandaki bazı tabloların veya mobilyaların yerini değiştirerek yeni enerji yarat.", creativeId, DifficultyLevel.Easy, 50, 30, "Yaşam alanındaki minör değişikliklerin motivasyon ve yaratıcı zihne pozitif etkisi büyüktür.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "dull_surroundings" }, new() { "feng_shui" }, new() { "oda", "dekorasyon" }));
            tasks.Add(CreateTask("Kollajen Çalışması", "Eski dergi ve gazetelerden kestiğin resimleri birleştirerek kolaj oluştur.", creativeId, DifficultyLevel.Easy, 45, 20, "Birbiriyle alakasız görsellerden yepyeni bir hikaye ve estetik yaratmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "poor_synthesis" }, new() { "collage" }, new() { "kolaj", "resim" }));
            tasks.Add(CreateTask("Hat Sanatı / Kaligrafi", "Adını kaligrafik olarak yazmayı dene.", creativeId, DifficultyLevel.Easy, 40, 15, "Yazıyı sanata dönüştürürken odaklanmayı ve el-göz koordinasyonunu artırmak için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "poor_handwriting" }, new() { "calligraphy" }, new() { "kaligrafi", "yazi" }));
            tasks.Add(CreateTask("Beyin Fırtınası Seansı", "Rastgele seçtiğin bir probleme 10 dakikada 20 çözüm üret.", creativeId, DifficultyLevel.Medium, 60, 15, "Yargılayıcı zihni kapatıp fikir akışını serbest bırakarak yanal düşünmeyi (lateral thinking) tetikler.", new() { DayTimeSlot.Morning, DayTimeSlot.Noon }, new() { "linear_thinking" }, new() { "lateral_thinking" }, new() { "problem", "ideation" }));
            tasks.Add(CreateTask("Tiyatro/Sinema Metni Okuma", "Ünlü bir tiyatro oyunundan bir sahneyi ses tonlamalarına dikkat ederek oku.", creativeId, DifficultyLevel.Easy, 50, 15, "Sesini esnek kullanabilmek, vurguları kavramak ve empati kurmak amacıyla.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "monotone_voice" }, new() { "acting_skills" }, new() { "tiyatro", "senaryo" }));
            tasks.Add(CreateTask("Koku Duyusu Egzersizi", "Gözlerini kapatıp evdeki 5 farklı baharatı sadece koklayarak tahmin et.", creativeId, DifficultyLevel.Easy, 35, 10, "Göz ardı edilen koku duyusunu keskinleştirip zihindeki koku kütüphanesini canlandırmak için.", new() { DayTimeSlot.Noon, DayTimeSlot.Evening }, new() { "sensory_dulness" }, new() { "sensory_awareness" }, new() { "koku", "baharat" }));
            tasks.Add(CreateTask("Çizim Eğitimi", "Temel çizim teknikleri hakkında 10 dakikalık bir eğitim videosu izle.", creativeId, DifficultyLevel.Easy, 40, 15, "Görsel algılama, gölgeleme ve 3 boyutlu düşünebilme refleksini güçlendirmek için.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "lack_of_technique" }, new() { "drawing_skills" }, new() { "cizim", "video" }));
            tasks.Add(CreateTask("Masal/Kısa Kurgu Uydurma", "Çevrendeki nesneleri konuşturduğun yaratıcı bir çocuk masalı uydur.", creativeId, DifficultyLevel.Medium, 65, 15, "Kurgusal düşünmeyi geliştirmek ve eğlenceli, kalıpların dışında anlatım kurmak için.", new() { DayTimeSlot.Evening, DayTimeSlot.Night }, new() { "cliche_thinking" }, new() { "storytelling" }, new() { "masal", "kurgu" }));
            tasks.Add(CreateTask("Kişisel Logo Tasarımı", "Kendi isminin baş harflerini kullanarak minimalist bir logo taslağı çiz.", creativeId, DifficultyLevel.Medium, 75, 20, "Soyut fikirleri tek bir amblemde birleştirme ve kurumsal kimlik bakışını pratik etme.", new() { DayTimeSlot.Afternoon, DayTimeSlot.Evening }, new() { "poor_abstraction" }, new() { "logo_design" }, new() { "logo", "minimalist" }));

            await context.AppTasks.AddRangeAsync(tasks);
            await context.SaveChangesAsync();
        }

        
        if (!await context.AppUsers.AnyAsync(u => u.Email == "admin@lifequest.com"))
        {
            var users = new List<AppUser>();

            AppUser CreateMockUser(string name, string surname, string username, string email, Guid roleId, double level, int xp, int streak)
            {
                byte[] passwordHash, passwordSalt;
                HashingHelper.CreatePasswordHash("Password123!", out passwordHash, out passwordSalt);

                return new AppUser
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Surname = surname,
                    UserName = username,
                    Email = email,
                    IsEmailVerified = true,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    AppRoleId = roleId,
                    GeneralLevel = level,
                    TotalXP = xp,
                    CurrentStreak = streak,
                    LongestStreak = streak + 5,
                    DailyAvailableMinutes = 60,
                    PreferredTasksPerWeek = 15,
                    DifficultyPreference = "mixed",
                    MotivationFactor = "leveling",
                    Bio = $"Merhaba, ben {name}. Hayatımı optimize etmeyi ve her gün yeni hedefler peşinde koşmayı seviyorum!",
                    Location = "İstanbul, TR",
                    Interests = new() { "technology", "fitness", "productivity", "reading" },
                    UserGoals = new() { "sağlıklı_yaşam", "kariyer_gelişimi", "dil_öğrenme" },
                    Strengths = new() { "discipline", "organization", "technical_skills" },
                    Weaknesses = new() { "perfectionism", "stress_management" },
                    DailyTaskLimit = 3,
                    WeeklyTaskLimit = 15,
                    OnboardingCompleted = true,
                    OnboardingCompletedAt = DateTime.UtcNow.AddDays(-30),
                    ProfileCompleted = true,
                    ProfileCompletedAt = DateTime.UtcNow.AddDays(-30),
                    AvatarId = new Random().Next(1, 20)
                };
            }

            var adminUser = new AppUser
            {
                Id = Guid.NewGuid(),
                Name = "Sistem",
                Surname = "Yöneticisi",
                UserName = "system_admin",
                Email = "admin@lifequest.com",
                IsEmailVerified = true,
                PasswordHash = Array.Empty<byte>(),
                PasswordSalt = Array.Empty<byte>(),
                AppRoleId = adminRole.Id,
                GeneralLevel = 99.0,
                TotalXP = 99999,
                Bio = "LifeQuest Sistem Yöneticisi.",
                OnboardingCompleted = true,
                ProfileCompleted = true
            };
            byte[] adminHash, adminSalt;
            HashingHelper.CreatePasswordHash("Admin123!", out adminHash, out adminSalt);
            adminUser.PasswordHash = adminHash;
            adminUser.PasswordSalt = adminSalt;

            users.Add(adminUser);

            var ahmet = CreateMockUser("Ahmet", "Yılmaz", "ahmet_yilmaz", "ahmet@lifequest.com", userRole.Id, 12.5, 12500, 5);
            var ayse = CreateMockUser("Ayşe", "Demir", "ayse_demir", "ayse@lifequest.com", userRole.Id, 8.2, 8200, 3);
            var mehmet = CreateMockUser("Mehmet", "Kaya", "mehmet_kaya", "mehmet@lifequest.com", userRole.Id, 4.0, 4000, 0);
            var elif = CreateMockUser("Elif", "Şahin", "elif_sahin", "elif@lifequest.com", userRole.Id, 15.8, 15800, 12);

            users.Add(ahmet);
            users.Add(ayse);
            users.Add(mehmet);
            users.Add(elif);

            await context.AppUsers.AddRangeAsync(users);
            await context.SaveChangesAsync();

            
            var allDbTasks = await context.AppTasks.ToListAsync();
            
            
            foreach (var user in users.Where(u => u.UserName != "system_admin"))
            {
                
                var randomTasks = allDbTasks.OrderBy(x => Guid.NewGuid()).Take(8).ToList();
                foreach (var task in randomTasks)
                {
                    var completedDate = DateTime.UtcNow.AddDays(new Random().Next(-15, -1));
                    var ut = new UserTask
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = user.Id,
                        AppTaskId = task.Id,
                        Status = AppTaskStatus.Completed,
                        EarnedXp = task.BaseXP,
                        AssignedAt = completedDate.AddHours(-2),
                        CompletedAt = completedDate,
                        DueDate = completedDate.Date,
                        DayTimeSlot = (DayTimeSlot)new Random().Next(0, 5),
                        UserEnergyAtStart = new Random().Next(3, 6),
                        UserMoodAtStart = (MoodMode)new Random().Next(0, 3),
                        ReflectionText = $"{task.Title} görevini başarıyla tamamladım. Kendimi harika hissediyorum ve bu alışkanlığı sürdürmek istiyorum!",
                        CustomTags = new() { "seeding", "completed", task.Title.ToLower().Replace(" ", "_") }
                    };
                    context.UserTasks.Add(ut);
                }

                
                var activeTasks = allDbTasks.OrderBy(x => Guid.NewGuid()).Take(2).ToList();
                
                var activeUt1 = new UserTask
                {
                    Id = Guid.NewGuid(),
                    AppUserId = user.Id,
                    AppTaskId = activeTasks[0].Id,
                    Status = AppTaskStatus.InProgress,
                    AssignedAt = DateTime.UtcNow.AddMinutes(-30),
                    DueDate = DateTime.UtcNow.Date,
                    DayTimeSlot = DayTimeSlotExtensions.FromHour(DateTime.UtcNow.Hour),
                    UserEnergyAtStart = 4,
                    UserMoodAtStart = MoodMode.Normal
                };
                
                var activeUt2 = new UserTask
                {
                    Id = Guid.NewGuid(),
                    AppUserId = user.Id,
                    AppTaskId = activeTasks[1].Id,
                    Status = AppTaskStatus.Selected,
                    AssignedAt = DateTime.UtcNow.AddHours(-1),
                    DueDate = DateTime.UtcNow.Date,
                    DayTimeSlot = DayTimeSlotExtensions.FromHour(DateTime.UtcNow.Hour)
                };
                context.UserTasks.Add(activeUt1);
                context.UserTasks.Add(activeUt2);

                
                var suggestedTasks = allDbTasks.OrderBy(x => Guid.NewGuid()).Take(4).ToList();
                foreach (var task in suggestedTasks)
                {
                    var sugUt = new UserTask
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = user.Id,
                        AppTaskId = task.Id,
                        Status = AppTaskStatus.Suggested,
                        AssignedAt = DateTime.UtcNow.AddHours(-3),
                        DueDate = DateTime.UtcNow.Date,
                        DayTimeSlot = DayTimeSlotExtensions.FromHour(DateTime.UtcNow.Hour)
                    };
                    context.UserTasks.Add(sugUt);
                }

                
                foreach (var category in new[] { egitim, saglik, kariyer, sosyal, kisiselGelisim, yaraticilik })
                {
                    var progress = new UserCategoryProgress
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = user.Id,
                        CategoryId = category.Id,
                        Level = new Random().Next(1, 6),
                        CurrentXp = new Random().Next(100, 500)
                    };
                    context.UserCategoryProgresses.Add(progress);
                }

                
                for (int d = -6; d <= 0; d++)
                {
                    var moodDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(d));
                    var mood = new UserDailyMood
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = user.Id,
                        Date = moodDate,
                        MoodMode = (MoodMode)new Random().Next(0, 5),
                        AvailableMinutes = new Random().Next(30, 120),
                        EnergyLevel = new Random().Next(4, 9),
                        SleepQuality = new Random().Next(3, 6),
                        JournalEntry = $"Bugün {moodDate} için günlük tutuyorum. Planlarımı büyük oranda tamamladım ve keyifli bir gündü.",
                        RecordedAt = DateTime.UtcNow.AddDays(d).Date.AddHours(22)
                    };
                    context.UserDailyMoods.Add(mood);
                }

                
                var randomBadges = await context.Badges.OrderBy(x => Guid.NewGuid()).Take(3).ToListAsync();
                foreach (var badge in randomBadges)
                {
                    var ub = new UserBadge
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = user.Id,
                        BadgeId = badge.Id,
                        EarnedAt = DateTime.UtcNow.AddDays(-10)
                    };
                    context.UserBadges.Add(ub);
                }

                
                var randomAchievements = await context.Achievements.OrderBy(x => Guid.NewGuid()).Take(2).ToListAsync();
                foreach (var ach in randomAchievements)
                {
                    var ua = new UserAchievement
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = user.Id,
                        AchievementId = ach.Id,
                        EarnedAt = DateTime.UtcNow.AddDays(-12),
                        IsCompleted = true
                    };
                    context.UserAchievements.Add(ua);
                }
            }

            await context.SaveChangesAsync();

            
            var ahmetUser = users.First(u => u.UserName == "ahmet_yilmaz");
            var ayseUser = users.First(u => u.UserName == "ayse_demir");
            var mehmetUser = users.First(u => u.UserName == "mehmet_kaya");
            var elifUser = users.First(u => u.UserName == "elif_sahin");

            var friendships = new List<Friendship>
            {
                new() { Id = Guid.NewGuid(), RequesterId = ahmetUser.Id, AddresseeId = ayseUser.Id, Status = FriendshipStatus.Accepted, AcceptedAt = DateTime.UtcNow.AddDays(-20) },
                new() { Id = Guid.NewGuid(), RequesterId = mehmetUser.Id, AddresseeId = elifUser.Id, Status = FriendshipStatus.Accepted, AcceptedAt = DateTime.UtcNow.AddDays(-15) },
                new() { Id = Guid.NewGuid(), RequesterId = ahmetUser.Id, AddresseeId = mehmetUser.Id, Status = FriendshipStatus.Accepted, AcceptedAt = DateTime.UtcNow.AddDays(-10) },
                new() { Id = Guid.NewGuid(), RequesterId = ayseUser.Id, AddresseeId = elifUser.Id, Status = FriendshipStatus.Pending, AcceptedAt = null }
            };

            await context.Friendships.AddRangeAsync(friendships);
            await context.SaveChangesAsync();

            
            
            var challengeActive = new Challenge
            {
                Id = Guid.NewGuid(),
                ChallengerId = ahmetUser.Id,
                OpponentId = ayseUser.Id,
                ChallengeType = ChallengeType.TaskCount,
                Status = ChallengeStatus.Active,
                Title = "Haftalık Kitap Kurdu Yarışı",
                Description = "Eğitim kategorisinde kim daha fazla görev tamamlayacak?",
                TargetCategoryId = egitim.Id,
                TargetValue = 5,
                ChallengerProgress = 3,
                OpponentProgress = 2,
                WinnerXPBonus = 150,
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(5),
                AcceptedAt = DateTime.UtcNow.AddDays(-2),
                ChallengerMessage = "Hadi bakalım, kim daha çok okuyacak?"
            };

            
            var challengeCompleted = new Challenge
            {
                Id = Guid.NewGuid(),
                ChallengerId = mehmetUser.Id,
                OpponentId = elifUser.Id,
                ChallengeType = ChallengeType.TaskCount,
                Status = ChallengeStatus.Completed,
                Title = "10.000 Adım Meydan Okuması",
                Description = "Sağlık kategorisinde en çok yürüyen kazansın!",
                TargetCategoryId = saglik.Id,
                TargetValue = 7,
                ChallengerProgress = 5,
                OpponentProgress = 7,
                WinnerId = elifUser.Id,
                WinnerXPBonus = 200,
                StartDate = DateTime.UtcNow.AddDays(-8),
                EndDate = DateTime.UtcNow.AddDays(-1),
                AcceptedAt = DateTime.UtcNow.AddDays(-8),
                ChallengerMessage = "Yürüyüş ayakkabılarını hazırla!"
            };

            await context.Challenges.AddRangeAsync(new[] { challengeActive, challengeCompleted });
            await context.SaveChangesAsync();

            
            var activeParticipant1 = new ChallengeParticipant { Id = Guid.NewGuid(), ChallengeId = challengeActive.Id, UserId = ahmetUser.Id, Role = 0, CurrentProgress = 3, IsWinner = false };
            var activeParticipant2 = new ChallengeParticipant { Id = Guid.NewGuid(), ChallengeId = challengeActive.Id, UserId = ayseUser.Id, Role = 1, CurrentProgress = 2, IsWinner = false };
            var compParticipant1 = new ChallengeParticipant { Id = Guid.NewGuid(), ChallengeId = challengeCompleted.Id, UserId = mehmetUser.Id, Role = 0, CurrentProgress = 5, IsWinner = false };
            var compParticipant2 = new ChallengeParticipant { Id = Guid.NewGuid(), ChallengeId = challengeCompleted.Id, UserId = elifUser.Id, Role = 1, CurrentProgress = 7, IsWinner = true };

            await context.ChallengeParticipants.AddRangeAsync(new[] { activeParticipant1, activeParticipant2, compParticipant1, compParticipant2 });
            await context.SaveChangesAsync();

            
            var post1 = new Post
            {
                Id = Guid.NewGuid(),
                AppUserId = ahmetUser.Id,
                Content = "Bugün 10.000 adım atma hedefimi gerçekleştirdim! Süper bir his, bacaklarım ağrıyor ama değdi. 💪🚶‍♂️",
                CreatedAt = DateTime.UtcNow.AddHours(-18)
            };

            var post2 = new Post
            {
                Id = Guid.NewGuid(),
                AppUserId = elifUser.Id,
                Content = "Yeni bir programlama diline veya zor bir konsepti öğrenmeye çalışırken Feynman tekniğini kullanmak inanılmaz etkili. Basitçe anlatabiliyorsan öğrenmişsin demektir! 🧠📚",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            };

            var post3 = new Post
            {
                Id = Guid.NewGuid(),
                AppUserId = ayseUser.Id,
                Content = "Haftalık planlama rutinim tamamlandı. Trello panom tertemiz ve yeni haftaya tamamen odaklanmış durumdayım. Başarı planlamayla başlar! 💼🎯",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            };

            await context.Posts.AddRangeAsync(new[] { post1, post2, post3 });
            await context.SaveChangesAsync();

            
            var like1 = new PostLike { Id = Guid.NewGuid(), PostId = post1.Id, AppUserId = ayseUser.Id, CreatedAt = DateTime.UtcNow.AddHours(-17) };
            var like2 = new PostLike { Id = Guid.NewGuid(), PostId = post1.Id, AppUserId = elifUser.Id, CreatedAt = DateTime.UtcNow.AddHours(-16) };
            var like3 = new PostLike { Id = Guid.NewGuid(), PostId = post2.Id, AppUserId = ahmetUser.Id, CreatedAt = DateTime.UtcNow.AddHours(-10) };
            var like4 = new PostLike { Id = Guid.NewGuid(), PostId = post3.Id, AppUserId = elifUser.Id, CreatedAt = DateTime.UtcNow.AddHours(-3) };

            await context.PostLikes.AddRangeAsync(new[] { like1, like2, like3, like4 });

            
            var comment1 = new PostComment
            {
                Id = Guid.NewGuid(),
                PostId = post1.Id,
                AppUserId = elifUser.Id,
                Content = "Tebrikler Ahmet! Harika bir istikrar.",
                CreatedAt = DateTime.UtcNow.AddHours(-16)
            };

            var comment2 = new PostComment
            {
                Id = Guid.NewGuid(),
                PostId = post2.Id,
                AppUserId = mehmetUser.Id,
                Content = "Ben de deneyeceğim Elif, gerçekten harika bir yöntem.",
                CreatedAt = DateTime.UtcNow.AddHours(-9)
            };

            var comment3 = new PostComment
            {
                Id = Guid.NewGuid(),
                PostId = post3.Id,
                AppUserId = ahmetUser.Id,
                Content = "Bu haftaki planını beğendim Ayşe, hedefleri gerçekleştirelim!",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            };

            await context.PostComments.AddRangeAsync(new[] { comment1, comment2, comment3 });
            await context.SaveChangesAsync();
        }
    }
}