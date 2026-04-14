# Sprint 1 - Tamamlama Özeti & Test Rehberi

## ✅ Yapılanlar

### Yeni Enum Dosyaları (6 dosya)
| Dosya | Açıklama |
|-------|----------|
| `VerificationType.cs` | None, Photo, Reflection, Timer, Quiz, PhotoAndReflection |
| `VerificationStatus.cs` | Pending, Approved, Rejected, NeedsReview |
| `GoalType.cs` | Daily, Weekly, Monthly, Custom |
| `NotificationType.cs` | 11 bildirim tipi (TaskAssigned → SystemMessage) |
| `AchievementType.cs` | 10 başarı tipi (TaskComplete → Consistent) |
| `FriendshipStatus.cs` | Pending, Accepted, Rejected, Blocked |

### Güncellenen Enum
| Dosya | Değişiklik |
|-------|------------|
| `AppTaskStatus.cs` | InProgress, Failed, Expired eklendi |

### Yeni Entity Dosyaları (9 dosya)
| Entity | Amaç |
|--------|------|
| `TaskVerification` | Görev doğrulama kanıtları (fotoğraf, metin, timer, quiz) |
| `UserGoal` | Kullanıcı hedef belirleme |
| `Achievement` | Başarı/milestone tanımları |
| `UserAchievement` | Kullanıcının başarı ilerlemesi |
| `Notification` | Uygulama içi bildirimler |
| `WeeklyReport` | Haftalık performans raporları |
| `Friendship` | Arkadaşlık ilişkileri |
| `Challenge` | Mücadele sistemi |
| `UserTaskInteraction` | ML eğitim verisi |

### Güncellenen Entity'ler
| Entity | Değişiklik |
|--------|------------|
| `AppTask` | +5 alan: VerificationType, MinReflectionWords, MinTimerMinutes, VerificationHint, ExpectedImageCategory |
| `AppUser` | +5 navigation property: Goals, UserAchievements, Notifications, WeeklyReports, TaskInteractions |

### Repository Dosyaları (36 dosya)
- **18 interface** (Application/Repositories altında: Read+Write x 9 entity)
- **18 implementasyon** (Persistence/Repositories altında: Read+Write x 9 entity)

### Altyapı Güncellemeleri
| Dosya | Değişiklik |
|-------|------------|
| `LifeQuestDbContext.cs` | 9 yeni DbSet, 10+ ilişki yapılandırması, unique indexler |
| `ServiceRegistration.cs` | 18 yeni repository DI kaydı |
| `SeedData.cs` | Doğrulama tipli görevler + 12 Achievement seed data |

### Build Durumu
```
Oluşturma başarılı oldu.
    0 Uyarı
    0 Hata
Geçen Süre 00:00:19.35
```

---

## 🧪 Test Rehberi

### 1. Migration Oluşturma

PostgreSQL'in çalıştığından emin olun, sonra şu komutu çalıştırın:

```powershell
cd c:\Users\mthng\OneDrive\Desktop\LifeQuest\LifeQuestAPI

dotnet ef migrations add BigExpansion_Sprint1 --project Infrastructure/LifeQuestAPI.Persistence --startup-project Presentation/LifeQuestAPI.API
```

> [!IMPORTANT]
> EF Core tools yüklü değilse önce şunu çalıştırın:
> ```powershell
> dotnet tool install --global dotnet-ef
> ```

### 2. Veritabanı Güncelleme

```powershell
dotnet ef database update --project Infrastructure/LifeQuestAPI.Persistence --startup-project Presentation/LifeQuestAPI.API
```

### 3. Veritabanı Doğrulama (psql ile)

PostgreSQL'e bağlanıp tabloların oluştuğunu doğrulayın:

```sql
-- Bağlan
psql -U postgres -d LifeQuestDB

-- Tüm tabloları listele
\dt

-- Yeni tabloları kontrol et
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
AND table_name IN (
    'TaskVerifications', 'UserGoals', 'Achievements', 
    'UserAchievements', 'Notifications', 'WeeklyReports', 
    'Friendships', 'Challenges', 'UserTaskInteractions'
);

-- AppTasks tablosundaki yeni kolonları kontrol et
\d "AppTasks"
-- VerificationType, MinReflectionWords, MinTimerMinutes, VerificationHint, ExpectedImageCategory görünmeli

-- Seed data'yı kontrol et
SELECT "Title", "Type", "RequiredCount", "BonusXP" FROM "Achievements";
SELECT "Title", "VerificationType", "MinReflectionWords", "MinTimerMinutes" FROM "AppTasks";
```

### 4. API'yi Çalıştırarak Test

```powershell
cd c:\Users\mthng\OneDrive\Desktop\LifeQuest\LifeQuestAPI\Presentation\LifeQuestAPI.API
dotnet run
```

Swagger'dan (`https://localhost:xxxx/swagger`) mevcut endpoint'lerin hala çalıştığını doğrulayın:
- `GET /api/Tasks/getall` → Görevlerde yeni alanlar (VerificationType vb.) görünmeli
- `POST /api/Auth/login` → Giriş çalışmalı
- `GET /api/Users/profile` → Profil çalışmalı

### 5. Veritabanını Sıfırdan Oluşturma (Opsiyonel)

Eğer mevcut veritabanınızda sorun çıkarsa, temiz başlangıç yapabilirsiniz:

```sql
-- PostgreSQL'de veritabanını sil ve yeniden oluştur
DROP DATABASE IF EXISTS "LifeQuestDB";
CREATE DATABASE "LifeQuestDB";
```

Sonra migration'ı tekrar uygulayın:
```powershell
dotnet ef database update --project Infrastructure/LifeQuestAPI.Persistence --startup-project Presentation/LifeQuestAPI.API
```

API'yi başlattığınızda SeedData otomatik çalışacak ve tüm örnek veriler oluşturulacak.

---

## 📊 Sprint 1 İstatistikleri

| Metrik | Değer |
|--------|-------|
| Oluşturulan yeni dosya | **51** |
| Güncellenen mevcut dosya | **5** |
| Yeni Entity | **9** |
| Yeni Enum | **6** |
| Yeni Repository Interface | **18** |
| Yeni Repository Implementasyon | **18** |
| Seed Achievement | **12** |
| Build hatası | **0** |
