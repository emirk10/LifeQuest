# LifeQuest — Tam Uygulama Planı

> **Kaynak:** `lifequest_roadmap.json` + `lifequest_roadmap_implementation_details.json`  
> **Toplam item:** 47 (roadmap) + 11 yeni (impl. details) = 58 madde  
> **Critical path:** P0-T1 → P0-T5 → P1-T3 → P2-T1 → P2-T2

---

## ⚡ PHASE 0 — Kritik Bug Fix Sprint *(1-2 gün)*

> [!CAUTION]
> Bu phase'deki maddeler atlanırsa proje derlenmez veya ciddi güvenlik açıkları devam eder.

### P0-T1 · BUG-001 · AppUser `Goals` property isim çakışması (derleme hatası)
- **Dosya:** `LifeQuestAPI.Domain/Entities/Identity/AppUser.cs`
- `List<string>? Goals` → **`UserGoals`** olarak yeniden adlandır
- `ICollection<UserGoal> Goals` → **`UserGoalsList`** olarak yeniden adlandır
- `DbContext OnModelCreating`'e `HasColumnName("UserGoals")` mapping ekle
- Migration: `20260423_RenameGoalsColumn.cs` oluştur
- Tüm handler ve query'lerde `user.Goals` referanslarını güncelle

### P0-T2 · BUG-002 · JWT Secret hardcoded (CWE-798)
- **Dosya:** `LifeQuestAPI.API/appsettings.json`
- `appsettings.json`'daki Token değerini temizle: `"Token": ""`
- `.env` dosyasına `JWT_SECRET=<min 64 char random>` ekle
- `docker-compose.yml`'e `environment: JWT_SECRET=${JWT_SECRET}` ekle
- `Program.cs`'de `Environment.GetEnvironmentVariable("JWT_SECRET")` kullan
- `.gitignore`'a `.env` ve `appsettings.Production.json` ekle

### P0-T3 · MISS-009 · Admin role authorization eksik (CWE-285)
- **Dosya:** `LifeQuestAPI.API/Controllers/TasksController.cs`
- `[Authorize(Roles = "Admin")]` attribute → `TasksController` class level
- `RolesController`'a da aynısını ekle
- `SeedData`'da Admin rolü ve default admin user oluştur
- `frontend/AdminPage.jsx`'te rol kontrolü ve redirect ekle

### P0-T4 · P0-T4 · OnboardingController /status endpoint bug (yeni tespit)
- **Dosya:** `LifeQuestAPI.API/Controllers/OnboardingController.cs`
- Şu an `return Ok(new { OnboardingCompleted = true })` — her zaman true dönüyor
- `GetOnboardingStatusQueryRequest` ve handler oluştur
- Handler: `_userRepo.GetByIdAsync(userId)` → `OnboardingCompleted` + `IsEmailVerified` döndür
- **Yeni dosya:** `Application/Features/Onboarding/Queries/GetOnboardingStatus/GetOnboardingStatusQueryRequest.cs`

### P0-T5 · BUG-007 · AI Service CORS allow_origins='*' (CWE-942)
- **Dosya:** `ai_service/main.py`
- `allow_origins=["http://lifequest-api:5020"]` olarak daralt
- `docker-compose.yml`'de AI servis `ports` → `expose` olarak değiştir
- `.NET` backend'den AI çağrılarına `X-Internal-Key` header ekle
- `ALLOWED_ORIGINS` env variable üzerinden konfigüre et

### P0-T6 · P0-T6 · DailyQuestWorker reflection kullanımı kaldır
- **Dosya:** `LifeQuestAPI.API/BackgroundServices/DailyQuestWorker.cs`
- `user.GetType().GetProperty("IsOnVacation")?.GetValue(user)` → `user.IsOnVacation` ile değiştir
- *(P0-T1'de `IsOnVacation` property eklendikten sonra uygulanmalı)*

### P0-T7 · P0-T9 · Program.cs CORS policy — SetIsOriginAllowed(true) açığı
- **Dosya:** `LifeQuestAPI.API/Program.cs`
- `policy.SetIsOriginAllowed(origin => true)` kaldır
- `appsettings.json`'dan `AllowedOrigins` array oku
- `appsettings.json`'a ekle: `"AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]`

---

## 🔐 PHASE 1 — Güvenlik & Auth Tamamlama *(2-3 gün)*

### P1-T1 · BUG-004 · Frontend JWT localStorage → Memory (CWE-359)
- **Dosya:** `frontend/src/api/axiosClient.js`
- `accessToken` değişkenini module-level state'de tut (`setAccessToken`, `getAccessToken`, `clearAccessToken`)
- `axios.create({ withCredentials: true })` ile cookie desteği aç
- Request interceptor: `Authorization: Bearer ${accessToken}` header ekle
- Response interceptor (401): `/auth/refresh-token` çağır → yeni token al → failed queue'yu temizle → retry
- **Backend:** `Response.Cookies.Append("refreshToken", ..., HttpOnly=true, Secure=true, SameSite=Strict)`
- `AuthContext`: `login()` → `setAccessToken(token)` | `logout()` → `clearAccessToken()` + POST `/auth/revoke`

### P1-T2 · MISS-001 · Refresh Token mekanizması
- **Yeni entity:** `RefreshToken { Id, UserId, Token(hashed), ExpiresAt, CreatedAt, RevokedAt, CreatedByIp }`
- **Migration:** `20260503_AddRefreshTokenTable.cs`
- **Yeni endpoint'ler:** `POST /api/auth/refresh` · `POST /api/auth/revoke`
- Access token: **15 dakika** | Refresh token: **7 gün** (httpOnly cookie)
- Token rotation: her refresh'te eski token revoke, yeni token üret

### P1-T3 · MISS-006 · FluentValidation validator'lar eksik
- **Paket zaten kurulu**, sadece sınıflar yazılmamış
- Oluşturulacak validator'lar:
  - `RegisterUserCommandValidator` — email format, şifre min 8, name/surname max 100
  - `LoginUserCommandValidator` — email + password NotEmpty
  - `CreateTaskCommandValidator` — BaseXP > 0, MinLevel ≥ 1, MaxLevel ≥ MinLevel
  - `RecordDailyMoodCommandValidator` — EnergyLevel 1-5 arası
  - `CreateGoalCommandValidator` — Deadline gelecekte olmalı
- **ValidationBehavior pipeline:** `Application/Common/Behaviors/ValidationBehavior.cs`
- `Program.cs`'e kayıt: `AddValidatorsFromAssembly` + `AddTransient(IPipelineBehavior<,>, ValidationBehavior<,>)`

### P1-T4 · Auth Rate Limiting — Brute force koruması
- **Dosya:** `Program.cs`
- `"AuthRateLimit"` policy: IP bazlı, **5 istek/dakika** (FixedWindowLimiter)
- `AuthController` register ve login action'larına `[EnableRateLimiting("AuthRateLimit")]` ekle

### P1-T5 · MISS-002 · Email doğrulama
- **AppUser'a yeni field'lar:** `IsEmailVerified bool`, `EmailVerificationToken string?`, `EmailVerificationTokenExpiry DateTime?`
- **Flow:** Register → mail gönder → `PATCH /api/auth/verify-email?token=xyz` → `IsEmailVerified = true`
- Email doğrulanmadan sosyal özellikler kısıtlansın

### P1-T6 · MISS-003 · Şifre sıfırlama
- **Yeni endpoint'ler:** `POST /api/auth/forgot-password` · `POST /api/auth/reset-password`
- Token: UUID, **1 saat geçerli**, tek kullanımlık

### P1-T7 · Email Service implementasyonu
- **Dosya:** `Infrastructure/Services/EmailService.cs`
- `IEmailService` interface: `SendEmailVerificationAsync`, `SendPasswordResetAsync`, `SendWeeklyReportAsync`
- `SmtpEmailService` implementasyonu (Gmail SMTP / SendGrid uyumlu)
- `appsettings.json`'a `EmailSettings` bloğu ekle
- `.env`'e `SMTP_USER` ve `SMTP_PASS` ekle
- `services.AddScoped<IEmailService, SmtpEmailService>()`

---

## 🏗️ PHASE 2 — Core Feature Tamamlama *(5-7 gün)*

### P2-T1 · MISS-007 · SignalR frontend hook
- **Yeni dosya:** `frontend/src/hooks/useSignalR.js`
- `HubConnectionBuilder` → `/hubs/notification` URL
- `accessTokenFactory: () => getAccessToken()`
- `withAutomaticReconnect([0, 2000, 5000, 10000])`
- Event'ler: `ReceiveNotification`, `ReceiveProactiveMessage`
- `ReceiveProactiveMessage` → `openChatBot` custom event dispatch et
- `App.jsx` veya `MainLayout.jsx`'te hook'u kullan

### P2-T2 · IMP-004 · React Query kurulumu
- `npm install @tanstack/react-query @tanstack/react-query-devtools`
- `main.jsx`'e `QueryClientProvider` ve `ReactQueryDevtools` ekle
- **Query hook'ları oluştur:**
  - `useUserProfile(userId)` — staleTime: 5 dk
  - `useDailyPool(count)` — staleTime: 10 dk
  - `useUserTasks(filters)` — staleTime: 30 sn
  - `useNotifications()` — staleTime: 1 dk, refetchOnWindowFocus: true
  - `useLeaderboard(category)` — staleTime: 5 dk
  - `useWeeklyReport(userId)` — staleTime: 1 saat
- **Mutation hook'ları:** `useCompleteTask`, `usePostponeTask`, `useSelectTask`

### P2-T3 · MISS-004 · Streak sistemi
- **AppUser'a yeni field'lar:** `CurrentStreak int`, `LongestStreak int`, `LastTaskCompletedDate DateOnly?`
- **Migration:** `20260503_AddStreakFields.cs`
- **Handler:** `CompleteUserTaskCommandHandler`'a streak mantığı ekle
  - Dün tamamlandıysa: `CurrentStreak++`
  - Daha önceyse veya null: `CurrentStreak = 1`
  - Bugün zaten tamamlandıysa: değişmez
  - `LongestStreak = Math.Max(LongestStreak, CurrentStreak)`
- **DailyQuestWorker:** Dünkü görevi tamamlamayanların streak'ini sıfırla

### P2-T4 · MISS-005 · Pagination implementasyonu
- **Yeni dosya:** `Application/Common/Extensions/PaginationExtensions.cs`
- `ToPaginatedListAsync<T>(pageNumber, pageSize)` extension method
- pageSize clamp: 1-100 arası
- Uygulanacak endpoint'ler: Leaderboard, user-tasks, chat/history, Notifications, Reports/weekly

### P2-T5 · BUG-003 · DailyQuestWorker UTC/Timezone hatası
- `DateTime.Today` → `DateTime.UtcNow` + Türkiye timezone dönüşümü
- `TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul")`
- `appsettings.json`'a `AppTimezone: "Europe/Istanbul"` ekle
- `Dockerfile`'a `ENV TZ=Europe/Istanbul` ve `tzdata` ekle

### P2-T6 · IMP-001 · GamificationService XP zenginleştirme
- **Dosya:** `Infrastructure/Services/GamificationService.cs`
- Streak multiplier: `>7 → *1.1` | `>30 → *1.25`
- Günlük ilk görev bonus: `+20 XP`
- Comeback bonus: 3+ gün aradan sonra `+50 XP`
- Weekly completion bonus: `+200 XP`
- Yeni level eşikleri: L1=0, L2=500, L3=1500, L5=4000, L10=15000, her 10+ level için `+5000`

### P2-T7 · IMP-002 · Badge sistemi genişletme
- **Yeni enum:** `BadgeType { Level, Streak, TaskCount, CategoryMaster, FirstTime, Social, Consistency, Comeback }`
- **Badge entity'e yeni field'lar:** `BadgeType`, `RequiredValue`, `CategoryId?`, `IsSecret`, `BadgeColor`, `Rarity`
- **AchievementService:** `CheckAndGrantBadgesAsync` — tüm kazanılmamış rozet'leri kontrol et, kazanılınca `UserBadge` ekle + bildirim gönder

### P2-T8 · BUG-005 · TaskRecommendationService Null Reference
- **Dosya:** `Infrastructure/Services/TaskRecommendationService.cs`
- `task.Description?.Contains()` — null-safe
- `task.EstimatedMinutes ?? int.MaxValue`
- `task.Category?.Name?.ToLower() ?? string.Empty`
- Metod başına `ArgumentNullException.ThrowIfNull` kontrolleri

### P2-T9 · IMP-003 · Öneri algoritması geliştirme
- Abandon cezası: önceden bırakılan görevlere `score -= 0.3`
- Variety bonus: aynı kategoriden 2+ öneri varsa 3.'yü farklı kategoriden seç
- Recency penalty: son 7 günde tamamlananları tekrar önerme
- Streak güvenlik bonusu: streak varsa besleyen görevlere `+0.05`
- Level check: `task.MinLevel <= userCategoryLevel <= task.MaxLevel`

### P2-T10 · BUG-006 · AI Service model persistence
- **Dosya:** `ai_service/main.py`
- `IntentClassifier.__init__`: `models/intent_clf.pkl` varsa yükle, yoksa eğit ve kaydet
- Aynısı `SentimentAnalyzer` için
- Docker volume: `ai_models:/app/models`
- FastAPI startup event ile model kontrolü
- `POST /api/train` endpoint'i sadece admin için bırak

---

## ⚡ PHASE 3 — Performans & Altyapı *(2-3 gün)*

### P3-T1 · IMP-006 · Eksik database index'leri
- **Migration:** `20260503_AddMissingIndexes.cs`
- Eklenecek index'ler:
  - `IX_UserTask_UserId_Status`
  - `IX_UserTask_UserId_DueDate`
  - `IX_Notification_UserId_IsRead`
  - `IX_ChatBotMessage_UserId_CreatedAt DESC`
  - `IX_UserCategoryProgress_UserId`
  - `IX_UserDailyMood_UserId_Date`
  - `IX_AppTask_IsActive_CategoryId`

### P3-T2 · IMP-008 · Serilog structured logging
- `dotnet add package Serilog.AspNetCore Serilog.Sinks.Console Serilog.Sinks.File`
- `Program.cs`'e `UseSerilog` konfigürasyonu
- Rolling file: `logs/lifequest-.log`
- Correlation ID middleware: `X-Correlation-ID` header
- Kritik log olayları: `TaskCompleted`, `LevelUp`, `BadgeEarned`, `AIServiceError`, `RecommendationGenerated`, `DailyWorkerCompleted`

### P3-T3 · MISS-010 · AI Service fallback mekanizması
- **Dosya:** `Infrastructure/Services/AIChatService.cs`
- HttpClient timeout: **8 saniye**
- Polly StandardResilienceHandler: retry MaxRetryAttempts=2, circuit breaker failureRatio=0.5
- **Yeni sınıf:** `AiFallbackResponses.cs` — intent bazlı Türkçe sabit cevaplar
- `try-catch` → fallback response dön, `IsFallback: true` flag

### P3-T4 · BUG-008 · DailyQuestWorker ölçeklenemez seri foreach
- Kısa vade: `Parallel.ForEachAsync` batch=500, parallelism=10
- EF Core bulk insert ile tek seferde kaydet

### P3-T5 · P3-T5 · Soft delete pattern
- **Dosya:** `Domain/Entities/Common/BaseEntity.cs`
- `IsDeleted bool = false` ve `DeletedAt DateTime?` ekle
- `DbContext OnModelCreating`'e global query filter: `AppUser`, `AppTask`, `UserTask`
- `dotnet ef migrations add AddSoftDelete`

### P3-T6 · P3-T6 · Database constraint'leri
- `AppTask`: `BaseXP > 0`, `MinLevel >= 1 AND MaxLevel >= MinLevel`
- `UserTask`: `XpGained >= 0`
- `dotnet ef migrations add AddDbConstraints`

### P3-T7 · IMP-007 · API versioning
- `dotnet add package Microsoft.AspNetCore.Mvc.Versioning`
- Header-based: `api-version: 1.0`
- Mevcut endpoint'ler v1, breaking change'de v2

### P3-T8 · IMP-014 · UserTask.DueDate — expire mekanizması
- `DueDate = DateTime.Today.AddDays(1).AddTicks(-1)` (bugün 23:59:59)
- `DailyQuestWorker`: dünkü Pending/Selected görevleri → `Expired`
- Postponed: `DueDate = PostponedTo date + 23:59:59`
- Frontend `TaskList`'te expire görevleri kırmızı göster

---

## ✨ PHASE 4 — Yeni Özellikler *(5-7 gün)*

### P4-T1 · FEAT-007 · Onboarding Turu (Frontend + Backend entegrasyonu)
- **AppUser'a:** `OnboardingCompleted bool = false`
- **Yeni dosya:** `frontend/src/pages/OnboardingTour.jsx`
- DashBoard.jsx'te `useQuery(['onboardingStatus'])` → tamamlanmamışsa `/onboarding`'e yönlendir
- **Adımlar:** Hoşgeldin → Günlük mod → İlk görev seç → Timer başlat → Tamamla ve XP kazan → Rozetler
- Son adımda `POST /api/onboarding/complete` çağır

### P4-T2 · MISS-008 · Challenge sistemi implementasyonu
- **Entity'e eklenecek field'lar:** `CategoryId?`, `TargetValue`, `MetricType enum (TaskCount/XpEarned/StreakDays)`, `StartDate/EndDate`, `Status enum`, `WinnerId?`
- **Yeni endpoint'ler:** create, accept, decline, active
- `ChallengeCompletionWorker` — EndDate geçince kazananı belirle

### P4-T3 · FEAT-006 · Akıllı Görev Hatırlatma
- **Yeni servis:** `TaskReminderWorker` — her saat başı çalışır
- `DayTimeSlot` bazlı — şu anki slotta görevi olan ve tamamlamamış kullanıcılara SignalR bildirimi
- Günde max 1 hatırlatma per görev
- `AppUser.NudgeEnabled` field'ını kontrol et

### P4-T4 · FEAT-003 · Görev Şablonları
- **Yeni entity:** `TaskTemplate { Id, Name, Description, CategoryId, Tasks: List<AppTask>, IsPublic, CreatedBy }`
- **Endpoint'ler:** `GET /api/task-templates` · `POST /api/task-templates/{id}/apply`
- Seed şablonlar: Sabah Rutini, Çalışma Seansı, Fitness Başlangıcı

### P4-T5 · FEAT-004 · Haftalık Özet Email
- `IEmailService.SendWeeklyReportAsync` zaten tasarlandı (P1-T7'de)
- `WeeklyReportWorker`'a email gönderimi ekle
- `AppUser.EmailNotificationsEnabled bool` opt-out field

### P4-T6 · MISS-011 · Kullanıcı tercih güncelleme
- **Yeni endpoint:** `PATCH /api/users/{id}/preferences`
- **Frontend:** `ProfilePage.jsx`'e 'Tercihlerimi Düzenle' bölümü

### P4-T7 · P4-T7 · PWA desteği
- `npm install vite-plugin-pwa`
- `vite.config.js`'e `VitePWA` plugin konfigürasyonu
- manifest: name, theme_color, icons, display: standalone
- workbox: tüm statik asset'leri cache'le

---

## 📱 PHASE 5 — Frontend Kalite & Mobil *(2-3 gün)*

### P5-T1 · IMP-012 · Responsive / Mobil optimizasyon
- `MainLayout.jsx` — hamburger menu, bottom navigation
- `DashBoard.jsx` — tek kolon mobil layout
- `TaskList.jsx` — swipe action (sağ=tamamla, sol=ertele)
- `TaskTimer.jsx` — büyük timer display
- `ChatBot.jsx` — full-screen modal
- Tüm Tailwind breakpoint'lerini (sm/md/lg) review et

### P5-T2 · IMP-011 · Frontend error handling tutarsızlığı
- `axiosClient.js` response interceptor: hata logla + toast göster
- `ErrorBoundary.jsx` tüm page-level component'leri sarsın
- 401 → logout + login sayfasına yönlendir
- Network error özel mesaj: `"Bağlantı sorunu, lütfen tekrar deneyin"`

### P5-T3 · IMP-010 · Frontend Docker API URL hardcoded
- Nginx config'de runtime env injection: `envsubst` ile `${API_URL}`

### P5-T4 · MISS-012 · SeedData genişletme
- Minimum 150 görev: 4 kategori × 3 zorluk × ~12 görev
- Minimum 20 rozet: level(5), streak(5), task count(5), kategori master(4), sosyal(3)
- En az 6 kategori: Eğitim, Sağlık, Kariyer, Sosyal, Kişisel Gelişim, Yaratıcılık
- 10-15 başarı tanımı

### P5-T5 · IMP-009 · TensorFlow dependency kaldır
- `requirements.txt`'den `tensorflow==2.18.0` kaldır (2GB+ image küçülür)
- `requirements-full.txt` oluştur

### P5-T6 · IMP-005 · AI intent model mimarisi
- 7 → 17 intent için Naive Bayes yeterli değil
- Keyword rule-based ön filtre + ML fallback mimarisi
- Keyword'ler: `başlat/start` → gorev_baslat, `bitti/tamamladım` → gorev_tamamla vb.

### P5-T7 · IMP-013 · Hata mesajları lokalizasyon
- Kullanıcıya dönen tüm mesajlar Türkçe
- Error code'lar İngilizce: `VALIDATION_ERROR`, `NOT_FOUND`
- Frontend: `error_code → Türkçe mesaj` map helper

---


## 🎯 Phase 4 - İkincil Özellikler (Düşük Öncelikli)

### FEAT-002 · Micro-journaling
- `UserTask.ReflectionText string?` field ekle
- `POST /api/ai/analyze-reflection` endpoint
- Reflection yazana `+5 XP` bonus

### FEAT-001 · Görev Paylaşım Linki
- `POST /api/user-tasks/{id}/share` → `ShareToken` (UUID, 7 gün)
- `GET /share/{token}` → public sayfa (login gerekmez)
- TaskItem'e 'Paylaş' butonu + Open Graph meta tag'ları

### FEAT-005 · Kudos Sistemi
- **Yeni entity:** `Kudos { Id, SenderId, ReceiverId, UserTaskId, CreatedAt }`
- `POST /api/Social/kudos`
- `+2 XP`, günde max 3 kudos alınabilsin

### FEAT-008 · Görev Tag Sistemi
- `AppTask.Tags List<string>?` — jsonb kolonu
- `UserTask.CustomTags List<string>?`
- Frontend: tag chip'leri + tag filtresi

### FEAT-009 · Admin Analitik Dashboard
- Endpoint'ler: task-dropout, category-performance, hourly-completions, user-retention
- `AdminPage.jsx`'e 'Analitik' tab + Recharts görselleştirme

---

## 📊 Özet Tablo

| Phase | Başlık | Süre | Öncelik |
|-------|--------|------|---------|
| Phase 0 | Kritik Bug Fix | 1-2 gün | 🔴 ZORUNLU |
| Phase 1 | Güvenlik & Auth | 2-3 gün | 🔴 ZORUNLU |
| Phase 2 | Core Features | 5-7 gün | 🟠 YÜKSEK |
| Phase 3 | Performans & Altyapı | 2-3 gün | 🟠 YÜKSEK |
| Phase 4 | Yeni Özellikler | 5-7 gün | 🟡 ORTA |
| Phase 5 | Frontend & Mobil | 2-3 gün | 🟡 ORTA |
| Phase 6 | Test Kapsamı | 3-5 gün | 🟢 DÜŞÜK |
| **TOPLAM** | | **~20-30 gün** | |

## ⚠️ Kesinlikle Atlanmaması Gerekenler

- `P0-T1` — Proje derlemiyor (Goals çakışması)
- `P0-T3` — Herkes admin görevi oluşturabilir
- `P0-T4` — Onboarding hiç çalışmıyor (her zaman true döndürüyor)
- `P1-T1` — XSS güvenlik açığı (localStorage JWT)
- `P0-T5` — AI servisi dışa açık (CORS)
- `P0-T7` — CORS tüm origin'lere açık
