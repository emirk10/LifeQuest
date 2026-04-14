# Sprint 1: Temel Altyapı & Veritabanı Genişletme

## Domain Layer - Enums
- [x] 1.1 VerificationType enum
- [x] 1.2 VerificationStatus enum
- [x] 1.3 GoalType enum
- [x] 1.4 NotificationType enum
- [x] 1.5 AchievementType enum
- [x] 1.6 FriendshipStatus enum
- [x] 1.7 AppTaskStatus enum güncelle (InProgress, Failed, Expired ekle)

## Domain Layer - Entities
- [x] 2.1 TaskVerification entity
- [x] 2.2 UserGoal entity
- [x] 2.3 Achievement entity
- [x] 2.4 UserAchievement entity
- [x] 2.5 Notification entity
- [x] 2.6 WeeklyReport entity
- [x] 2.7 Friendship entity
- [x] 2.8 Challenge entity
- [x] 2.9 UserTaskInteraction entity (ML eğitim verisi)
- [x] 2.10 AppTask entity güncelle (doğrulama alanları ekle)
- [x] 2.11 AppUser entity güncelle (yeni navigation property'ler)

## Application Layer - Repository Interfaces
- [x] 3.1 TaskVerification (Read/Write)
- [x] 3.2 UserGoal (Read/Write)
- [x] 3.3 Achievement (Read/Write)
- [x] 3.4 UserAchievement (Read/Write)
- [x] 3.5 Notification (Read/Write)
- [x] 3.6 WeeklyReport (Read/Write)
- [x] 3.7 Friendship (Read/Write)
- [x] 3.8 Challenge (Read/Write)
- [x] 3.9 UserTaskInteraction (Read/Write)

## Persistence Layer - Repository Implementations
- [x] 4.1 Tüm yeni entity'ler için Read/Write repository sınıfları
- [x] 4.2 DbContext güncelle (DbSet'ler + OnModelCreating)
- [x] 4.3 ServiceRegistration güncelle (DI kayıtları)

## Seed Data
- [x] 5.1 Achievement seed data (12 başarı)
- [x] 5.2 Doğrulama tipli örnek görevler (Photo, Reflection, Timer, PhotoAndReflection)

## Test & Doğrulama
- [x] 6.1 Build başarılı (0 Uyarı, 0 Hata)
- [ ] 6.2 Migration oluşturma (kullanıcı PostgreSQL çalıştırmalı)
- [ ] 6.3 Veritabanı güncelleme (kullanıcı PostgreSQL çalıştırmalı)
