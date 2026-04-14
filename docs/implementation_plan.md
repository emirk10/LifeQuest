# 🚀 LifeQuest - Yapay Zeka Destekli Kişisel Gelişim Uygulaması
## Kapsamlı Geliştirme Yol Haritası & Bitirme Projesi Rehberi

---

## 📋 Mevcut Proje Analizi

### Mimari Yapı
Projeniz **Onion Architecture** (Clean Architecture) ile tasarlanmış, çok katmanlı bir .NET + React uygulamasıdır:

| Katman | Teknoloji | Açıklama |
|--------|-----------|----------|
| **Presentation** | ASP.NET Core Web API | 8 Controller (Auth, Users, Tasks, UserTasks, Badges, UserBadges, Categories, Roles) |
| **Application** | MediatR (CQRS) | Command/Query ayrımı, DTOs, Feature klasörleri |
| **Domain** | Entity sınıfları | AppUser, AppTask, UserTask, Badge, Category, ChatBotMessage vb. |
| **Infrastructure** | Services | GamificationService, DailyQuestService, JwtTokenService |
| **Persistence** | EF Core + PostgreSQL | Repository pattern (Read/Write ayrımı) |
| **Frontend** | React + Vite + TailwindCSS | Dashboard, Tasks, Badges, Profile, Admin sayfaları |

### Mevcut Özellikler ✅
- Kullanıcı kayıt/giriş (JWT Authentication)
- Rol tabanlı yetkilendirme (Admin/User)
- Görev oluşturma (tekli/toplu)
- Günlük görev dağıtım sistemi (BackgroundService)
- Görev tamamlama → XP kazanma
- Kategori bazlı seviye sistemi
- Rozet sistemi (seviyeye göre otomatik kazanım)
- Leaderboard (sıralama)
- Admin paneli
- Gamification altyapısı (XP, Level, Badge)

### Tespit Edilen Eksiklikler ❌
1. **Yapay zeka kısmı tamamen eksik** - `ChatBotMessage` entity var ama kullanılmıyor
2. **Görev doğrulama yok** - "Bitir" butonuna basınca direkt tamamlanıyor
3. **Chatbot/Asistan yok** - Kullanıcıyla etkileşim sağlayan AI yok
4. **Öneri sistemi yok** - Görevler rastgele atanıyor
5. **Duygu/ruh hali analizi yok** - Kullanıcının durumuna göre adaptasyon yok
6. **İstatistik/analitik sayfası basit** - Derinlemesine analiz eksik
7. **Sosyal özellikler yok** - Arkadaşlık, paylaşım yok
8. **Bildirim sistemi yok** - Push/in-app notification eksik
9. **Mobil uyumluluk yetersiz** - Sidebar responsive değil

---

## 🤖 BÖLÜM 1: YAPAY ZEKA ENTEGRasyonu (API Kullanmadan - Kendi Modelimiz)

> [!IMPORTANT]
> API kullanmadan yapay zeka geliştirmek için **iki ana yol** var: **.NET tarafında ML.NET** ve **Python tarafında scikit-learn/TensorFlow** ile mikro servis. Bitirme projesi için **ikisinin kombinasyonu** en etkileyici olacaktır.

### 1.1 Teknoloji Seçimi

| Bileşen | Teknoloji | Neden? |
|---------|-----------|--------|
| **Öneri Sistemi** | ML.NET (C#) | Ana backend ile aynı dil, kolay entegrasyon |
| **Metin Sınıflandırma (NLP)** | Python + scikit-learn | Türkçe NLP için daha zengin ekosistem |
| **Görüntü Doğrulama** | Python + TensorFlow/Keras | Transfer learning ile hızlı model eğitimi |
| **Duygu Analizi** | Python + BERT/Transformers | Türkçe duygu analizi modeli |
| **Chatbot** | Rule-based + ML hibrit | Pratik ve eğitilebilir |

---

### 1.2 AI Modül 1: Akıllı Görev Öneri Sistemi (ML.NET)

**Amaç:** Kullanıcının geçmiş davranışlarına göre en uygun görevleri önerme

#### Veri Toplama (Eğitim Verisi)

```csharp
// Yeni Entity: UserTaskInteraction (kullanıcı-görev etkileşim verisi)
public class UserTaskInteraction : BaseEntity
{
    public Guid AppUserId { get; set; }
    public Guid AppTaskId { get; set; }
    
    // Feature'lar
    public int UserLevel { get; set; }
    public int UserCategoryLevel { get; set; }
    public int TaskDifficulty { get; set; }      // 1,2,3
    public int TaskBaseXP { get; set; }
    public int DayOfWeek { get; set; }            // 0-6
    public int HourOfDay { get; set; }            // 0-23
    public int UserCompletedTaskCount { get; set; }
    public int UserStreakDays { get; set; }
    
    // Label (Tahmin edilecek)
    public bool WasCompleted { get; set; }        // Görevi tamamladı mı?
    public float CompletionTimeHours { get; set; } // Kaç saatte tamamladı?
    public float Rating { get; set; }             // 1-5 arası kullanıcı puanı
}
```

#### ML.NET Model Eğitimi

```csharp
// LifeQuestAPI.Infrastructure/Services/AI/TaskRecommendationService.cs
using Microsoft.ML;
using Microsoft.ML.Data;

public class TaskRecommendationService : ITaskRecommendationService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private readonly string _modelPath = "Models/task_recommendation_model.zip";
    
    public TaskRecommendationService()
    {
        _mlContext = new MLContext(seed: 0);
        LoadModel();
    }
    
    public async Task TrainModelAsync(List<TaskInteractionData> trainingData)
    {
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
        
        // Veri pipeline
        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(TaskInteractionData.UserLevel),
                nameof(TaskInteractionData.UserCategoryLevel),
                nameof(TaskInteractionData.TaskDifficulty),
                nameof(TaskInteractionData.TaskBaseXP),
                nameof(TaskInteractionData.DayOfWeek),
                nameof(TaskInteractionData.HourOfDay),
                nameof(TaskInteractionData.UserCompletedTaskCount),
                nameof(TaskInteractionData.UserStreakDays))
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.BinaryClassification.Trainers
                .FastTree(labelColumnName: "WasCompleted", featureColumnName: "Features"));
        
        _model = pipeline.Fit(dataView);
        
        // Modeli kaydet
        _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
    }
    
    public List<Guid> GetRecommendedTasks(UserProfile user, List<AppTask> availableTasks, int count = 3)
    {
        var predEngine = _mlContext.Model
            .CreatePredictionEngine<TaskInteractionData, TaskPrediction>(_model);
        
        var scoredTasks = availableTasks.Select(task => new
        {
            Task = task,
            Score = predEngine.Predict(new TaskInteractionData
            {
                UserLevel = (float)user.GeneralLevel,
                UserCategoryLevel = user.GetCategoryLevel(task.CategoryId),
                TaskDifficulty = (float)task.Difficulty,
                TaskBaseXP = task.BaseXP,
                DayOfWeek = (float)DateTime.Now.DayOfWeek,
                HourOfDay = DateTime.Now.Hour,
                UserCompletedTaskCount = user.CompletedTaskCount,
                UserStreakDays = user.StreakDays
            }).Probability
        })
        .OrderByDescending(x => x.Score)
        .Take(count)
        .Select(x => x.Task.Id)
        .ToList();
        
        return scoredTasks;
    }
}

// Tahmin sınıfı
public class TaskPrediction
{
    [ColumnName("PredictedLabel")]
    public bool WillComplete { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}
```

#### Başlangıç Verisi Stratejisi (Cold Start)

> [!TIP]
> Model eğitmek için yeterli veri olmadığında "cold start" problemi oluşur. Çözüm:

1. **Sentetik veri üretin**: `collector.py` dosyanız zaten var, bunu genişletin
2. **Kural tabanlı hybrid**: İlk 100 etkileşime kadar kural tabanlı, sonra ML
3. **Önceden eğitilmiş model**: Sentetik veri ile pre-train

```python
# collector.py - Genişletilmiş sentetik veri üretici
import random
import json
import uuid
from datetime import datetime, timedelta

def generate_synthetic_training_data(num_samples=5000):
    data = []
    categories = ["Spor", "Eğitim", "Sağlık", "Kariyer", "Sosyal"]
    
    for _ in range(num_samples):
        user_level = random.randint(1, 20)
        task_difficulty = random.choice([1, 2, 3])
        day_of_week = random.randint(0, 6)
        hour = random.randint(6, 23)
        streak = random.randint(0, 30)
        completed_count = random.randint(0, 200)
        
        # Gerçekçi tamamlama olasılığı hesapla
        completion_probability = 0.5
        
        # Seviye-zorluk uyumu
        if user_level >= task_difficulty * 3:
            completion_probability += 0.2
        elif user_level < task_difficulty:
            completion_probability -= 0.3
        
        # Streak etkisi
        if streak > 7:
            completion_probability += 0.15
        
        # Saat etkisi (sabah ve akşam daha verimli)
        if 7 <= hour <= 10 or 18 <= hour <= 21:
            completion_probability += 0.1
        
        # Hafta sonu etkisi
        if day_of_week >= 5:
            completion_probability += 0.05
        
        was_completed = random.random() < min(max(completion_probability, 0.1), 0.95)
        
        data.append({
            "userLevel": user_level,
            "userCategoryLevel": random.randint(1, user_level),
            "taskDifficulty": task_difficulty,
            "taskBaseXP": random.choice([50, 100, 150, 200, 300]),
            "dayOfWeek": day_of_week,
            "hourOfDay": hour,
            "userCompletedTaskCount": completed_count,
            "userStreakDays": streak,
            "wasCompleted": was_completed,
            "completionTimeHours": random.uniform(0.5, 24) if was_completed else 0,
            "rating": random.randint(1, 5) if was_completed else 0
        })
    
    return data

if __name__ == "__main__":
    data = generate_synthetic_training_data(10000)
    with open("training_data.json", "w") as f:
        json.dump(data, f, indent=2)
    print(f"✅ {len(data)} adet eğitim verisi üretildi.")
```

---

### 1.3 AI Modül 2: Türkçe Chatbot / Kişisel Asistan

**Amaç:** Kullanıcıyla doğal dilde etkileşim, motivasyon, yönlendirme

#### Mimari: Rule-Based + Intent Classification Hibrit

```
Kullanıcı Mesajı → Intent Classification → Route
                                             ├─→ Motivasyon → Template cevaplar
                                             ├─→ Görev Sorgulama → DB sorgusu
                                             ├─→ İstatistik → Analiz servisi
                                             ├─→ Öneri İsteme → ML öneri servisi
                                             └─→ Bilinmeyen → Fallback cevap
```

#### Python Mikro Servis: Intent Classification

```python
# ai_service/intent_classifier.py
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.pipeline import Pipeline
import pickle
import json

class IntentClassifier:
    def __init__(self):
        self.model = None
        self.intents = {}
        
    def load_training_data(self):
        """Türkçe intent eğitim verisi"""
        return {
            "motivasyon": [
                "beni motive et", "motivasyonum düştü", "kendimi kötü hissediyorum",
                "başaramıyorum", "vazgeçmek istiyorum", "moralim bozuk",
                "enerjim yok", "sıkıldım", "motivasyon lazım",
                "bıkmak üzereyim", "zorlanıyorum", "yapamıyorum",
                "kendime güvenemiyorum", "başarısız hissediyorum"
            ],
            "gorev_sorgula": [
                "bugünkü görevlerim ne", "görevlerimi göster", "ne yapmam gerekiyor",
                "hangi görevlerim var", "aktif görevler", "yapılacaklar",
                "bugün ne yapacağım", "görev listesi", "görevlerim neler"
            ],
            "istatistik": [
                "istatistiklerimi göster", "ne kadar ilerleme kaydettim",
                "seviyem kaç", "kaç XP kazandım", "performansım nasıl",
                "durumum nedir", "nasıl gidiyorum", "gelişimim nasıl",
                "ilerlemem ne durumda", "başarı oranım"
            ],
            "oneri": [
                "bana görev öner", "ne yapmalıyım", "hangi görevleri yapayım",
                "benim için uygun görev", "önerebileceğin bir şey var mı",
                "yeni görev istiyorum", "farklı bir şey öner"
            ],
            "selamlama": [
                "merhaba", "selam", "nasılsın", "hey", "günaydın",
                "iyi akşamlar", "naber", "ne haber"
            ],
            "saglik": [
                "sağlık önerisi ver", "sağlıklı yaşam", "egzersiz öner",
                "diyet önerisi", "uyku düzenim bozuk", "stresli hissediyorum"
            ],
            "rozet": [
                "rozetlerim neler", "hangi rozetleri kazanabilirim",
                "sonraki rozet ne", "rozet durumum", "başarılarım"
            ]
        }
    
    def train(self):
        training_data = self.load_training_data()
        
        texts = []
        labels = []
        for intent, examples in training_data.items():
            texts.extend(examples)
            labels.extend([intent] * len(examples))
        
        self.model = Pipeline([
            ('tfidf', TfidfVectorizer(
                analyzer='char_wb',  # Türkçe için karakter n-gram daha iyi
                ngram_range=(2, 5),
                max_features=5000
            )),
            ('clf', MultinomialNB(alpha=0.1))
        ])
        
        self.model.fit(texts, labels)
        
        # Modeli kaydet
        with open('models/intent_model.pkl', 'wb') as f:
            pickle.dump(self.model, f)
        
        print("✅ Intent classifier eğitildi ve kaydedildi.")
    
    def predict(self, text: str) -> dict:
        if self.model is None:
            with open('models/intent_model.pkl', 'rb') as f:
                self.model = pickle.load(f)
        
        intent = self.model.predict([text.lower()])[0]
        probabilities = self.model.predict_proba([text.lower()])[0]
        confidence = max(probabilities)
        
        return {
            "intent": intent,
            "confidence": float(confidence),
            "text": text
        }
```

#### Python Flask/FastAPI Mikro Servis

```python
# ai_service/main.py
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from intent_classifier import IntentClassifier
from sentiment_analyzer import SentimentAnalyzer
from image_verifier import ImageVerifier
import uvicorn

app = FastAPI(title="LifeQuest AI Service")

# Servisleri başlat
intent_clf = IntentClassifier()
sentiment = SentimentAnalyzer()
image_verifier = ImageVerifier()

class ChatRequest(BaseModel):
    user_id: str
    message: str

class ChatResponse(BaseModel):
    intent: str
    confidence: float
    response: str
    sentiment: str
    suggestions: list[str] = []

class ImageVerifyRequest(BaseModel):
    user_id: str
    task_id: str
    task_category: str
    image_base64: str

@app.post("/api/chat", response_model=ChatResponse)
async def chat(request: ChatRequest):
    # Intent tahmin et
    intent_result = intent_clf.predict(request.message)
    
    # Duygu analizi
    sentiment_result = sentiment.analyze(request.message)
    
    # Intent'e göre cevap üret
    response_text = generate_response(
        intent_result["intent"], 
        intent_result["confidence"],
        sentiment_result
    )
    
    return ChatResponse(
        intent=intent_result["intent"],
        confidence=intent_result["confidence"],
        response=response_text,
        sentiment=sentiment_result["label"],
        suggestions=get_suggestions(intent_result["intent"])
    )

@app.post("/api/verify-image")
async def verify_image(request: ImageVerifyRequest):
    result = image_verifier.verify(
        request.image_base64,
        request.task_category
    )
    return result

@app.post("/api/train")
async def train_models():
    """Admin tarafından tetiklenen model eğitimi"""
    intent_clf.train()
    return {"status": "success", "message": "Modeller yeniden eğitildi"}

def generate_response(intent: str, confidence: float, sentiment: dict) -> str:
    """Intent ve duygu durumuna göre cevap üret"""
    
    if confidence < 0.3:
        return "Tam olarak anlayamadım. Görevlerin, istatistiklerin veya motivasyon hakkında konuşabiliriz. 😊"
    
    responses = {
        "motivasyon": {
            "negative": [
                "Herkesin zor günleri olur! Senin şimdiye kadar başardıklarına bak - bu kadar yol geldiysen devam edebilirsin! 💪",
                "Küçük adımlar büyük değişimler yaratır. Bugün sadece bir görev tamamlamayı hedefle, gerisini yarına bırak. 🌟",
                "Unutma, başarısızlık bir son değil, öğrenme fırsatıdır. Sana inanıyorum! 🚀"
            ],
            "neutral": [
                "Harika gidiyorsun! Devam et, her gün bir adım daha ileri! 💪",
                "Düzenli çalışma alışkanlığın seni zirveye taşıyacak! 🎯"
            ],
            "positive": [
                "Bu enerjini seviyorum! Hadi bugün ekstra bir görev daha halledelim! 🔥",
                "Pozitif enerjin bulaşıcı! Bu gidişle yeni rozetler kazanacaksın! 🏆"
            ]
        },
        "selamlama": {
            "default": [
                "Merhaba! 👋 Bugün kendini geliştirmek için hazır mısın?",
                "Selam! LifeQuest'e hoş geldin! Bugün ne yapmak istersin?"
            ]
        },
        # ... diğer intent'ler için cevaplar
    }
    
    import random
    
    sentiment_label = sentiment.get("label", "neutral")
    intent_responses = responses.get(intent, {})
    
    if sentiment_label in intent_responses:
        return random.choice(intent_responses[sentiment_label])
    elif "default" in intent_responses:
        return random.choice(intent_responses["default"])
    else:
        return "İsteğini anladım! Hemen yardımcı oluyorum. 😊"

def get_suggestions(intent: str) -> list:
    """Quick reply önerileri"""
    suggestions_map = {
        "motivasyon": ["Görevlerimi göster", "Bana kolay görev öner", "İstatistiklerimi göster"],
        "gorev_sorgula": ["Yeni görev iste", "Profili göster", "Rozetlerimi göster"],
        "istatistik": ["Haftalık rapor", "En iyi olduğum kategori", "Hedeflerimi göster"],
        "selamlama": ["Bugünkü görevlerim", "İstatistiklerim", "Beni motive et"],
    }
    return suggestions_map.get(intent, ["Yardım", "Görevlerim", "Profilim"])

if __name__ == "__main__":
    # İlk çalıştırmada modeli eğit
    intent_clf.train()
    uvicorn.run(app, host="0.0.0.0", port=5001)
```

#### .NET Backend Entegrasyonu

```csharp
// Application/Abstractions/AI/IAIChatService.cs
public interface IAIChatService
{
    Task<ChatBotResponse> ProcessMessageAsync(Guid userId, string message);
}

// Infrastructure/Services/AI/AIChatService.cs
public class AIChatService : IAIChatService
{
    private readonly HttpClient _httpClient;
    private readonly IUserTaskReadRepository _userTaskRepo;
    private readonly IAppUserReadRepository _userRepo;
    
    public AIChatService(HttpClient httpClient, /* ... */)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5001");
    }
    
    public async Task<ChatBotResponse> ProcessMessageAsync(Guid userId, string message)
    {
        // Python AI servisine gönder
        var aiRequest = new { user_id = userId.ToString(), message = message };
        var response = await _httpClient.PostAsJsonAsync("/api/chat", aiRequest);
        var aiResult = await response.Content.ReadFromJsonAsync<AIChatResult>();
        
        // Intent'e göre ek veri çek
        string enrichedResponse = aiResult.Response;
        
        if (aiResult.Intent == "gorev_sorgula")
        {
            var tasks = await _userTaskRepo.Table
                .Where(ut => ut.AppUserId == userId && ut.Status == AppTaskStatus.Assigned)
                .Include(ut => ut.AppTask)
                .ToListAsync();
            
            enrichedResponse += $"\n\n📋 Aktif görevlerin ({tasks.Count} adet):\n";
            foreach (var task in tasks)
            {
                enrichedResponse += $"• {task.AppTask.Title} (+{task.AppTask.BaseXP} XP)\n";
            }
        }
        else if (aiResult.Intent == "istatistik")
        {
            var user = await _userRepo.GetByIdAsync(userId.ToString());
            enrichedResponse += $"\n\n📊 Senin durumun:\n";
            enrichedResponse += $"• Level: {Math.Floor(user.GeneralLevel)}\n";
            enrichedResponse += $"• Toplam XP: {user.TotalXP}\n";
        }
        
        return new ChatBotResponse
        {
            Message = enrichedResponse,
            Intent = aiResult.Intent,
            Suggestions = aiResult.Suggestions
        };
    }
}
```

---

### 1.4 AI Modül 3: Duygu Analizi (Sentiment Analysis)

**Amaç:** Kullanıcının mesajlarından ruh halini tespit edip cevapları uyarlama

```python
# ai_service/sentiment_analyzer.py
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.svm import SVC
from sklearn.pipeline import Pipeline
import pickle

class SentimentAnalyzer:
    def __init__(self):
        self.model = None
        self.train_model()
    
    def get_training_data(self):
        """Türkçe sentiment eğitim verisi"""
        positive = [
            "harika hissediyorum", "çok mutluyum", "süper gidiyor",
            "bugün çok enerjiğim", "kendimle gurur duyuyorum",
            "başardım", "çok iyi hissediyorum", "mükemmel bir gün",
            "motivasyonum çok yüksek", "her şey yolunda"
        ]
        negative = [
            "çok kötü hissediyorum", "mutsuzum", "başaramıyorum",
            "moralim bozuk", "enerjim yok", "sıkıldım",
            "kendime güvenemiyorum", "her şey kötü", "umutsuzum",
            "yoruldum", "bıktım", "stres altındayım"
        ]
        neutral = [
            "bugünkü görevlerim ne", "nasıl yapacağım", "bilgi ver",
            "göster", "kaç görevim var", "seviyem kaç",
            "ne zaman", "nerede", "nasıl"
        ]
        
        texts = positive + negative + neutral
        labels = (["positive"] * len(positive) + 
                  ["negative"] * len(negative) + 
                  ["neutral"] * len(neutral))
        
        return texts, labels
    
    def train_model(self):
        texts, labels = self.get_training_data()
        
        self.model = Pipeline([
            ('tfidf', TfidfVectorizer(
                analyzer='char_wb',
                ngram_range=(2, 4),
                max_features=3000
            )),
            ('svc', SVC(kernel='linear', probability=True))
        ])
        
        self.model.fit(texts, labels)
        
        with open('models/sentiment_model.pkl', 'wb') as f:
            pickle.dump(self.model, f)
    
    def analyze(self, text: str) -> dict:
        if self.model is None:
            with open('models/sentiment_model.pkl', 'rb') as f:
                self.model = pickle.load(f)
        
        label = self.model.predict([text.lower()])[0]
        probas = self.model.predict_proba([text.lower()])[0]
        confidence = max(probas)
        
        return {
            "label": label,
            "confidence": float(confidence),
            "scores": {
                cls: float(prob) 
                for cls, prob in zip(self.model.classes_, probas)
            }
        }
```

---

## ✅ BÖLÜM 2: GÖREV DOĞRULAMA MEKANİZMASI

> [!WARNING]
> Şu anda `CompleteUserTaskCommandHandler` içinde hiçbir doğrulama yok. Kullanıcı "Bitir" butonuna basınca görev direkt tamamlanıyor. Bu, uygulamanın güvenilirliğini ciddi şekilde etkiliyor.

### 2.1 Çok Katmanlı Doğrulama Sistemi

```
Görev Tamamlama İsteği
    │
    ├─→ Katman 1: Fotoğraf/Görsel Kanıt (AI ile doğrulama)
    ├─→ Katman 2: Metin/Yansıma Raporu (NLP analizi)
    ├─→ Katman 3: Zaman Tabanlı Doğrulama (süre kontrolü)
    ├─→ Katman 4: Konum Tabanlı Doğrulama (opsiyonel)
    └─→ Katman 5: Topluluk Doğrulaması (peer review)
```

### 2.2 Doğrulama Tip Tanımları

```csharp
// Domain/Enums/VerificationType.cs
public enum VerificationType
{
    None = 0,           // Doğrulama gerekmez (basit görevler)
    Photo = 1,          // Fotoğraf yükleme (AI doğrulama)
    Reflection = 2,     // Metin/yansıma yazma (NLP analizi)
    Timer = 3,          // Zamanlayıcı (minimum süre)
    Quiz = 4,           // Mini sınav/quiz
    PhotoAndReflection = 5 // Hem fotoğraf hem yansıma
}

// Domain/Enums/VerificationStatus.cs
public enum VerificationStatus
{
    Pending = 0,       // Doğrulama bekliyor
    Approved = 1,      // Onaylandı
    Rejected = 2,      // Reddedildi
    NeedsReview = 3    // Manuel inceleme gerekiyor
}
```

### 2.3 Yeni Entity'ler

```csharp
// Domain/Entities/TaskVerification.cs
public class TaskVerification : BaseEntity
{
    public Guid UserTaskId { get; set; }
    public UserTask UserTask { get; set; } = default!;
    
    public VerificationType Type { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    
    // Fotoğraf doğrulama
    public string? PhotoPath { get; set; }
    public float? AIConfidenceScore { get; set; }  // AI'ın doğruluk güveni (0-1)
    public string? AIAnalysisResult { get; set; }   // AI sonuç detayı (JSON)
    
    // Metin/yansıma doğrulama
    public string? ReflectionText { get; set; }
    public int? ReflectionWordCount { get; set; }
    public float? SentimentScore { get; set; }      // Duygu analizi skoru
    public bool? IsReflectionRelevant { get; set; } // NLP: İlgili mi?
    
    // Zaman doğrulama
    public DateTime? TimerStartedAt { get; set; }
    public DateTime? TimerEndedAt { get; set; }
    public int? MinimumMinutesRequired { get; set; }
    
    // Quiz doğrulama
    public int? QuizScore { get; set; }
    public int? QuizTotalQuestions { get; set; }
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? ReviewNotes { get; set; }        // Admin notu
}
```

```csharp
// AppTask entity'sine eklenmesi gereken alanlar
public class AppTask : BaseEntity
{
    // ... mevcut alanlar ...
    
    // YENİ: Doğrulama gereksinimleri
    public VerificationType VerificationType { get; set; } = VerificationType.None;
    public int MinReflectionWords { get; set; } = 0;        // Min kelime sayısı
    public int MinTimerMinutes { get; set; } = 0;            // Min zamanlayıcı dakika
    public string? VerificationHint { get; set; }            // "Koşu sonrası fotoğraf çekin"
    public string? ExpectedImageCategory { get; set; }       // "outdoor_exercise", "book", "food"
}
```

### 2.4 Görüntü Doğrulama AI Modeli (Transfer Learning)

```python
# ai_service/image_verifier.py
import tensorflow as tf
from tensorflow.keras.applications import MobileNetV2
from tensorflow.keras.layers import Dense, GlobalAveragePooling2D, Dropout
from tensorflow.keras.models import Model
from tensorflow.keras.preprocessing import image
import numpy as np
import base64
from io import BytesIO
from PIL import Image

class ImageVerifier:
    def __init__(self):
        self.model = None
        self.categories = {
            "outdoor_exercise": ["koşu", "yürüyüş", "bisiklet", "park"],
            "indoor_exercise": ["spor salonu", "yoga", "pilates"],
            "reading": ["kitap", "kütüphane", "okuma"],
            "cooking": ["yemek", "mutfak", "sağlıklı gıda"],
            "studying": ["çalışma", "laptop", "not defteri"],
            "meditation": ["meditasyon", "nefes", "huzur"],
            "social": ["arkadaşlar", "toplantı", "etkinlik"],
        }
        self.load_model()
    
    def build_model(self, num_classes):
        """MobileNetV2 ile transfer learning modeli"""
        base_model = MobileNetV2(
            weights='imagenet',
            include_top=False,
            input_shape=(224, 224, 3)
        )
        
        # Son birkaç katmanı eğitilebilir yap
        for layer in base_model.layers[-20:]:
            layer.trainable = True
        
        x = base_model.output
        x = GlobalAveragePooling2D()(x)
        x = Dense(256, activation='relu')(x)
        x = Dropout(0.5)(x)
        x = Dense(128, activation='relu')(x)
        x = Dropout(0.3)(x)
        predictions = Dense(num_classes, activation='softmax')(x)
        
        model = Model(inputs=base_model.input, outputs=predictions)
        model.compile(
            optimizer=tf.keras.optimizers.Adam(learning_rate=0.0001),
            loss='categorical_crossentropy',
            metrics=['accuracy']
        )
        return model
    
    def load_model(self):
        """Eğitilmiş modeli yükle"""
        try:
            self.model = tf.keras.models.load_model('models/image_verifier.h5')
        except:
            print("⚠️ Eğitilmiş model bulunamadı. Yeni model oluşturulacak.")
            self.model = self.build_model(len(self.categories))
    
    def preprocess_image(self, img_base64: str):
        """Base64 görüntüyü preprocessle"""
        img_bytes = base64.b64decode(img_base64)
        img = Image.open(BytesIO(img_bytes)).convert('RGB')
        img = img.resize((224, 224))
        img_array = np.array(img) / 255.0
        return np.expand_dims(img_array, axis=0)
    
    def verify(self, image_base64: str, expected_category: str) -> dict:
        """Görüntünün beklenen kategoriye uygunluğunu doğrula"""
        processed = self.preprocess_image(image_base64)
        
        predictions = self.model.predict(processed)[0]
        category_names = list(self.categories.keys())
        
        # En yüksek tahmin
        max_idx = np.argmax(predictions)
        predicted_category = category_names[max_idx]
        confidence = float(predictions[max_idx])
        
        # Beklenen kategori kontrolü
        is_match = predicted_category == expected_category
        
        # Güven eşiği
        threshold = 0.6
        is_verified = is_match and confidence >= threshold
        
        # Eğer güven düşükse admin incelemesine gönder
        needs_review = not is_verified and confidence >= 0.3
        
        return {
            "is_verified": is_verified,
            "needs_review": needs_review,
            "predicted_category": predicted_category,
            "expected_category": expected_category,
            "confidence": confidence,
            "all_predictions": {
                cat: float(pred) 
                for cat, pred in zip(category_names, predictions)
            },
            "message": self._get_verification_message(is_verified, needs_review, confidence)
        }
    
    def _get_verification_message(self, verified, needs_review, confidence):
        if verified:
            return "✅ Görev doğrulandı! Harika iş çıkardın!"
        elif needs_review:
            return "🔍 Fotoğrafın incelemeye alındı. Sonuç kısa süre içinde bildirilecek."
        else:
            return "❌ Fotoğraf görevle uyuşmuyor gibi görünüyor. Tekrar dener misin?"
```

### 2.5 Metin/Yansıma Doğrulama

```python
# ai_service/reflection_verifier.py
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
import re

class ReflectionVerifier:
    def __init__(self):
        self.vectorizer = TfidfVectorizer(
            analyzer='char_wb',
            ngram_range=(2, 5),
            max_features=5000
        )
    
    def verify(self, reflection_text: str, task_title: str, 
               task_description: str, min_words: int = 20) -> dict:
        """Yansıma metnini doğrula"""
        
        # 1. Kelime sayısı kontrolü
        word_count = len(reflection_text.split())
        has_enough_words = word_count >= min_words
        
        # 2. Spam/anlamsız metin kontrolü
        is_spam = self._check_spam(reflection_text)
        
        # 3. İlgililik kontrolü (görevle alakalı mı?)
        relevance_score = self._check_relevance(
            reflection_text, task_title, task_description
        )
        
        # 4. İçerik kalitesi
        quality_score = self._assess_quality(reflection_text)
        
        # Genel skor hesapla
        overall_score = 0.0
        if has_enough_words:
            overall_score += 0.25
        if not is_spam:
            overall_score += 0.25
        overall_score += relevance_score * 0.3
        overall_score += quality_score * 0.2
        
        is_verified = overall_score >= 0.5 and not is_spam
        
        return {
            "is_verified": is_verified,
            "overall_score": overall_score,
            "word_count": word_count,
            "has_enough_words": has_enough_words,
            "is_spam": is_spam,
            "relevance_score": relevance_score,
            "quality_score": quality_score,
            "feedback": self._generate_feedback(
                is_verified, word_count, min_words, 
                is_spam, relevance_score
            )
        }
    
    def _check_spam(self, text: str) -> bool:
        """Spam/anlamsız metin kontrolü"""
        # Tekrarlayan karakterler
        if re.search(r'(.)\1{5,}', text):
            return True
        # Çok kısa kelimeler oranı
        words = text.split()
        if words:
            short_ratio = sum(1 for w in words if len(w) <= 2) / len(words)
            if short_ratio > 0.7:
                return True
        # Aynı kelimenin aşırı tekrarı
        if words:
            from collections import Counter
            word_counts = Counter(words)
            most_common_ratio = word_counts.most_common(1)[0][1] / len(words)
            if most_common_ratio > 0.5 and len(words) > 5:
                return True
        return False
    
    def _check_relevance(self, reflection: str, title: str, description: str) -> float:
        """Görevle ilgililik kontrolü (TF-IDF cosine similarity)"""
        task_text = f"{title} {description}"
        
        try:
            tfidf_matrix = self.vectorizer.fit_transform([task_text, reflection])
            similarity = cosine_similarity(tfidf_matrix[0:1], tfidf_matrix[1:2])[0][0]
            return float(similarity)
        except:
            return 0.3  # Varsayılan orta skor
    
    def _assess_quality(self, text: str) -> float:
        """Metin kalitesi değerlendirmesi"""
        score = 0.0
        words = text.split()
        
        # Uzunluk
        if len(words) >= 50: score += 0.3
        elif len(words) >= 30: score += 0.2
        elif len(words) >= 15: score += 0.1
        
        # Cümle çeşitliliği
        sentences = text.split('.')
        if len(sentences) >= 3: score += 0.2
        
        # Kelime çeşitliliği
        unique_ratio = len(set(words)) / max(len(words), 1)
        score += unique_ratio * 0.3
        
        # Noktalama kullanımı
        punctuation_count = sum(1 for c in text if c in '.,!?;:')
        if punctuation_count >= 3: score += 0.2
        
        return min(score, 1.0)
    
    def _generate_feedback(self, verified, word_count, min_words, is_spam, relevance):
        if is_spam:
            return "⚠️ Lütfen anlamlı bir yansıma yazın."
        if word_count < min_words:
            return f"📝 En az {min_words} kelime yazmanız gerekiyor. (Şu an: {word_count})"
        if relevance < 0.2:
            return "🔍 Yazdığınız metin görevle ilgili görünmüyor. Görev hakkında deneyimlerinizi paylaşır mısınız?"
        if verified:
            return "✅ Harika bir yansıma! Deneyimlerin kaydedildi."
        return "📝 Biraz daha detay eklerseniz daha iyi olur."
```

### 2.6 Zamanlayıcı Doğrulama (Frontend)

```jsx
// frontend/src/components/TaskTimer.jsx
import React, { useState, useEffect, useRef } from 'react';

const TaskTimer = ({ minimumMinutes, onTimerComplete, taskTitle }) => {
  const [seconds, setSeconds] = useState(0);
  const [isRunning, setIsRunning] = useState(false);
  const [isCompleted, setIsCompleted] = useState(false);
  const intervalRef = useRef(null);
  const requiredSeconds = minimumMinutes * 60;

  useEffect(() => {
    if (isRunning) {
      intervalRef.current = setInterval(() => {
        setSeconds(prev => {
          const next = prev + 1;
          if (next >= requiredSeconds && !isCompleted) {
            setIsCompleted(true);
            onTimerComplete(next);
          }
          return next;
        });
      }, 1000);
    }
    return () => clearInterval(intervalRef.current);
  }, [isRunning]);

  const formatTime = (s) => {
    const mins = Math.floor(s / 60);
    const secs = s % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const progress = Math.min((seconds / requiredSeconds) * 100, 100);

  return (
    <div className="bg-slate-900/60 p-6 rounded-2xl border border-slate-800">
      <h4 className="text-white font-bold mb-2">{taskTitle}</h4>
      <p className="text-slate-500 text-xs mb-4">
        Minimum {minimumMinutes} dakika gerekli
      </p>
      
      {/* Dairesel zamanlayıcı */}
      <div className="relative w-32 h-32 mx-auto mb-4">
        <svg className="w-full h-full -rotate-90" viewBox="0 0 100 100">
          <circle cx="50" cy="50" r="45" fill="none" 
                  stroke="#1e293b" strokeWidth="8"/>
          <circle cx="50" cy="50" r="45" fill="none"
                  stroke={isCompleted ? "#10b981" : "#6366f1"}
                  strokeWidth="8"
                  strokeDasharray={`${progress * 2.83} 283`}
                  strokeLinecap="round"
                  className="transition-all duration-1000"/>
        </svg>
        <div className="absolute inset-0 flex items-center justify-center">
          <span className="text-2xl font-black text-white font-mono">
            {formatTime(seconds)}
          </span>
        </div>
      </div>
      
      <button
        onClick={() => setIsRunning(!isRunning)}
        className={`w-full py-3 rounded-xl font-bold transition-all ${
          isRunning 
            ? 'bg-amber-500 hover:bg-amber-400 text-black' 
            : 'bg-indigo-600 hover:bg-indigo-500 text-white'
        }`}
      >
        {isRunning ? '⏸ Duraklat' : '▶ Başlat'}
      </button>
      
      {isCompleted && (
        <p className="text-emerald-400 text-center text-sm mt-3 font-bold animate-pulse">
          ✅ Süre tamamlandı! Görevi bitirebilirsin.
        </p>
      )}
    </div>
  );
};

export default TaskTimer;
```

### 2.7 Güncellenmiş Görev Tamamlama Akışı

```csharp
// Güncellenmiş CompleteUserTaskCommandRequest
public class CompleteUserTaskCommandRequest : IRequest<CompleteUserTaskCommandResponse>
{
    public Guid UserTaskId { get; set; }
    public Guid UserId { get; set; }
    
    // Doğrulama verileri
    public string? PhotoBase64 { get; set; }
    public string? ReflectionText { get; set; }
    public int? TimerSeconds { get; set; }
    public int? QuizScore { get; set; }
    public int? QuizTotalQuestions { get; set; }
}

// Güncellenmiş Handler
public async Task<CompleteUserTaskCommandResponse> Handle(
    CompleteUserTaskCommandRequest request, CancellationToken ct)
{
    var userTask = await _userTaskReadRepository.Table
        .Include(ut => ut.AppTask)
        .FirstOrDefaultAsync(ut => ut.Id == request.UserTaskId, ct);

    if (userTask == null)
        return new() { Success = false, Message = "Görev bulunamadı." };

    if (userTask.AppUserId != request.UserId)
        return new() { Success = false, Message = "Bu görev size ait değil." };

    if (userTask.CompletedAt.HasValue)
        return new() { Success = false, Message = "Bu görev zaten tamamlanmış." };

    var task = userTask.AppTask;
    
    // ---- DOĞRULAMA KATMANI ----
    var verification = new TaskVerification
    {
        UserTaskId = userTask.Id,
        Type = task.VerificationType
    };

    bool isVerified = true;
    string verificationMessage = "";

    switch (task.VerificationType)
    {
        case VerificationType.Photo:
            if (string.IsNullOrEmpty(request.PhotoBase64))
                return new() { Success = false, Message = "Bu görev için fotoğraf kanıtı gerekli." };
            
            var photoResult = await _imageVerificationService
                .VerifyAsync(request.PhotoBase64, task.ExpectedImageCategory);
            
            verification.PhotoPath = await SavePhoto(request.PhotoBase64);
            verification.AIConfidenceScore = photoResult.Confidence;
            verification.AIAnalysisResult = JsonSerializer.Serialize(photoResult);
            
            isVerified = photoResult.IsVerified;
            
            if (photoResult.NeedsReview)
            {
                verification.Status = VerificationStatus.NeedsReview;
                verificationMessage = "Fotoğrafınız incelemeye alındı.";
            }
            break;

        case VerificationType.Reflection:
            if (string.IsNullOrEmpty(request.ReflectionText))
                return new() { Success = false, Message = "Bu görev için yansıma yazısı gerekli." };
            
            var reflectionResult = await _reflectionVerificationService
                .VerifyAsync(request.ReflectionText, task.Title, task.Description, task.MinReflectionWords);
            
            verification.ReflectionText = request.ReflectionText;
            verification.ReflectionWordCount = reflectionResult.WordCount;
            verification.SentimentScore = reflectionResult.OverallScore;
            verification.IsReflectionRelevant = reflectionResult.RelevanceScore > 0.3;
            
            isVerified = reflectionResult.IsVerified;
            verificationMessage = reflectionResult.Feedback;
            break;

        case VerificationType.Timer:
            if (!request.TimerSeconds.HasValue || request.TimerSeconds < task.MinTimerMinutes * 60)
            {
                int remaining = (task.MinTimerMinutes * 60) - (request.TimerSeconds ?? 0);
                return new() { 
                    Success = false, 
                    Message = $"Zamanlayıcı süresi yetersiz. {remaining / 60} dakika daha gerekli." 
                };
            }
            isVerified = true;
            break;
            
        case VerificationType.None:
            isVerified = true;
            break;
    }

    // Doğrulama kaydını kaydet
    verification.Status = isVerified ? VerificationStatus.Approved : VerificationStatus.Rejected;
    await _verificationWriteRepo.AddAsync(verification);

    if (!isVerified)
    {
        await _verificationWriteRepo.SaveAsync();
        return new() { 
            Success = false, 
            Message = verificationMessage ?? "Doğrulama başarısız oldu. Lütfen tekrar deneyin." 
        };
    }

    // Görev tamamlama (mevcut kod)
    int xpToEarn = task.BaseXP;
    userTask.CompletedAt = DateTime.UtcNow;
    userTask.Status = AppTaskStatus.Completed;
    userTask.EarnedXp = xpToEarn;

    await _userTaskWriteRepository.SaveAsync();
    await _gamificationService.AddExperienceAsync(request.UserId, xpToEarn, task.CategoryId);

    return new()
    {
        Success = true,
        Message = "Görev başarıyla doğrulandı ve tamamlandı! 🎉",
        EarnedXp = xpToEarn
    };
}
```

---

## 🎯 BÖLÜM 3: EKLENMESİ GEREKEN KAPSAMLI ÖZELLİKLER

### 3.1 Hedef Belirleme Sistemi

```csharp
// Domain/Entities/UserGoal.cs
public class UserGoal : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
    
    public string Title { get; set; } = string.Empty;          // "Bu ay 30 görev tamamla"
    public string Description { get; set; } = string.Empty;
    
    public GoalType Type { get; set; }                          // Daily, Weekly, Monthly, Custom
    public int TargetValue { get; set; }                        // Hedef sayı
    public int CurrentValue { get; set; } = 0;                  // Mevcut ilerleme
    
    public Guid? CategoryId { get; set; }                       // Opsiyonel: Belirli kategori
    public Category? Category { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public int BonusXP { get; set; } = 0;                       // Hedef tamamlama bonusu
}

public enum GoalType
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Custom = 3
}
```

### 3.2 Başarı/Achievement Sistemi (Rozet Sistemini Genişleten)

```csharp
// Domain/Entities/Achievement.cs
public class Achievement : BaseEntity
{
    public string Title { get; set; } = string.Empty;       // "İlk Adım"
    public string Description { get; set; } = string.Empty;  // "İlk görevini tamamla"
    public string IconPath { get; set; } = string.Empty;
    public AchievementType Type { get; set; }
    
    public int RequiredCount { get; set; }    // Kaç kez yapılmalı
    public int BonusXP { get; set; }          // Ek XP ödülü
    public bool IsHidden { get; set; }        // Gizli başarı? (Sürpriz)
}

public enum AchievementType
{
    TaskComplete = 0,       // X görev tamamla
    StreakDays = 1,          // X gün seri
    CategoryMaster = 2,     // X kategoride max level
    EarlyBird = 3,          // Sabah 7'den önce görev tamamla
    NightOwl = 4,           // Gece 23'ten sonra görev
    Perfectionist = 5,      // Hiç red almadan X görev
    SocialButterfly = 6,    // X arkadaş edin
    Collector = 7,          // X rozet topla
    SpeedRunner = 8,        // X dakikada görev tamamla
    Consistent = 9          // 30 gün üst üste
}
```

### 3.3 Bildirim Sistemi

```csharp
// Domain/Entities/Notification.cs
public class Notification : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
    
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }        // Tıklanınca nereye gidecek
    public string? IconEmoji { get; set; }
}

public enum NotificationType
{
    TaskAssigned = 0,       // Yeni görev atandı
    TaskCompleted = 1,      // Görev tamamlandı
    BadgeEarned = 2,        // Rozet kazanıldı
    LevelUp = 3,            // Seviye atlandı
    StreakReminder = 4,      // Seri kırılma uyarısı
    GoalProgress = 5,       // Hedef ilerleme
    GoalCompleted = 6,      // Hedef tamamlandı
    AIRecommendation = 7,   // AI önerisi
    SystemMessage = 8       // Sistem mesajı
}
```

### 3.4 Haftalık/Aylık Rapor Sistemi

```csharp
// Domain/Entities/WeeklyReport.cs
public class WeeklyReport : BaseEntity
{
    public Guid AppUserId { get; set; }
    public AppUser AppUser { get; set; } = default!;
    
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    
    public int TasksCompleted { get; set; }
    public int TasksFailed { get; set; }
    public int TotalXPEarned { get; set; }
    public int StreakDays { get; set; }
    
    public string StrongestCategory { get; set; } = string.Empty;
    public string WeakestCategory { get; set; } = string.Empty;
    
    // AI analiz sonuçları
    public string? AIInsight { get; set; }              // "Bu hafta spor kategorisinde %40 artış var"
    public string? AIRecommendation { get; set; }       // "Eğitim kategorisine daha fazla odaklan"
    public float? OverallPerformanceScore { get; set; } // 0-100 arası genel performans
    
    public string? CategoryBreakdownJson { get; set; }  // Kategori bazlı detay
}
```

### 3.5 Sosyal Özellikler

```csharp
// Domain/Entities/Friendship.cs
public class Friendship : BaseEntity
{
    public Guid RequesterId { get; set; }
    public AppUser Requester { get; set; } = default!;
    
    public Guid AddresseeId { get; set; }
    public AppUser Addressee { get; set; } = default!;
    
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
    public DateTime? AcceptedAt { get; set; }
}

// Domain/Entities/Challenge.cs (Arkadaş Mücadelesi)
public class Challenge : BaseEntity
{
    public Guid ChallengerId { get; set; }
    public AppUser Challenger { get; set; } = default!;
    
    public Guid OpponentId { get; set; }
    public AppUser Opponent { get; set; } = default!;
    
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public string Title { get; set; } = string.Empty;     // "7 gün spor mücadelesi"
    public int TargetTaskCount { get; set; }
    
    public int ChallengerProgress { get; set; } = 0;
    public int OpponentProgress { get; set; } = 0;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? WinnerId { get; set; }
    public int WinnerBonusXP { get; set; } = 200;
}
```

---

## 🏗️ BÖLÜM 4: MİMARİ GELİŞTİRMELER

### 4.1 Python AI Mikro Servis Mimarisi

```
LifeQuest/
├── LifeQuestAPI/           # Mevcut .NET Backend
├── frontend/               # Mevcut React Frontend
├── ai_service/             # YENİ: Python AI Mikro Servis
│   ├── main.py             # FastAPI giriş noktası
│   ├── requirements.txt
│   ├── Dockerfile
│   ├── models/             # Eğitilmiş model dosyaları
│   │   ├── intent_model.pkl
│   │   ├── sentiment_model.pkl
│   │   └── image_verifier.h5
│   ├── services/
│   │   ├── intent_classifier.py
│   │   ├── sentiment_analyzer.py
│   │   ├── image_verifier.py
│   │   ├── reflection_verifier.py
│   │   └── recommendation_engine.py
│   ├── training/           # Model eğitim scriptleri
│   │   ├── train_intent.py
│   │   ├── train_sentiment.py
│   │   ├── train_image.py
│   │   └── generate_data.py
│   └── data/               # Eğitim verileri
│       ├── intents_tr.json
│       ├── sentiments_tr.json
│       └── images/
└── docker-compose.yml      # Tüm servisleri bir arada çalıştır
```

### 4.2 Docker Compose

```yaml
# docker-compose.yml
version: '3.8'
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: LifeQuestDB
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: StrongPass123
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
  
  api:
    build: ./LifeQuestAPI
    ports:
      - "5000:8080"
    depends_on:
      - postgres
      - ai-service
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=LifeQuestDB;Username=postgres;Password=StrongPass123
      - AIService__BaseUrl=http://ai-service:5001
  
  ai-service:
    build: ./ai_service
    ports:
      - "5001:5001"
    volumes:
      - ./ai_service/models:/app/models
  
  frontend:
    build: ./frontend
    ports:
      - "3000:3000"
    depends_on:
      - api

volumes:
  pgdata:
```

### 4.3 SignalR ile Gerçek Zamanlı Bildirimler

```csharp
// Presentation/Hubs/NotificationHub.cs
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }
    
    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }
}

// Kullanım:
// await _hubContext.Clients.Group($"user_{userId}")
//     .SendAsync("ReceiveNotification", notification);
```

---

## 🗺️ BÖLÜM 5: ADIM ADIM YOL HARİTASI

> [!IMPORTANT]
> Bu yol haritası 8 sprint (her biri ~2 hafta) olarak planlanmıştır. Toplam ~4 aylık bir çalışma süresini kapsar.

---

### 🔷 SPRINT 1: Temel Altyapı & Veritabanı Genişletme (Hafta 1-2)

**Hedef:** Yeni entity'leri ekle, migration oluştur, temel yapıyı hazırla

| # | Görev | Detay | Süre |
|---|-------|-------|------|
| 1.1 | Yeni Entity'leri oluştur | `TaskVerification`, `UserGoal`, `Achievement`, `Notification`, `WeeklyReport`, `Friendship`, `Challenge`, `UserTaskInteraction` | 2 gün |
| 1.2 | Enum'ları ekle | `VerificationType`, `VerificationStatus`, `GoalType`, `NotificationType`, `AchievementType`, `FriendshipStatus` | 1 gün |
| 1.3 | `AppTask` entity'sini genişlet | `VerificationType`, `MinReflectionWords`, `MinTimerMinutes`, `ExpectedImageCategory` alanlarını ekle | 0.5 gün |
| 1.4 | `AppTaskStatus` enum'unu genişlet | `InProgress`, `Failed`, `Expired` durumlarını ekle | 0.5 gün |
| 1.5 | Repository interface ve implementasyonları | Her yeni entity için Read/Write repository | 2 gün |
| 1.6 | DbContext güncelle | Yeni entity'leri ekle, ilişkileri tanımla | 1 gün |
| 1.7 | Migration oluştur ve test et | `dotnet ef migrations add BigExpansion` | 0.5 gün |
| 1.8 | Seed data hazırla | Test için örnek veriler (kategoriler, görevler, rozetler) | 1 gün |

**Çıktı:** Veritabanı yapısı genişletilmiş, tüm entity'ler hazır

---

### 🔷 SPRINT 2: Python AI Mikro Servis Kurulumu (Hafta 3-4)

**Hedef:** AI mikro servisini ayağa kaldır, ilk modelleri eğit

| # | Görev | Detay | Süre |
|---|-------|-------|------|
| 2.1 | Python proje yapısını oluştur | FastAPI, proje klasör yapısı, requirements.txt | 0.5 gün |
| 2.2 | Intent Classifier eğitim verisi hazırla | En az 200+ Türkçe örnek cümle, 7+ intent | 2 gün |
| 2.3 | Intent Classifier modeli eğit | TF-IDF + Naive Bayes/SVM pipeline | 1 gün |
| 2.4 | Sentiment Analyzer eğitim verisi | Pozitif/Negatif/Nötr Türkçe cümleler (300+) | 1 gün |
| 2.5 | Sentiment Analyzer modeli eğit | TF-IDF + SVM pipeline | 1 gün |
| 2.6 | Reflection Verifier'ı kodla | Spam kontrolü, ilgililik, kalite analizi | 1 gün |
| 2.7 | Image Verifier temel yapı | MobileNetV2 transfer learning kurulumu | 1 gün |
| 2.8 | FastAPI endpoint'leri oluştur | `/api/chat`, `/api/verify-image`, `/api/verify-reflection`, `/api/train` | 1 gün |
| 2.9 | Docker kurulumu | Dockerfile, docker-compose | 0.5 gün |
| 2.10 | Testler | Her endpoint için Postman/pytest testleri | 1 gün |

**Çıktı:** AI mikro servisi çalışır durumda, modeller eğitilmiş

---

### 🔷 SPRINT 3: Görev Doğrulama Entegrasyonu (Hafta 5-6)

**Hedef:** Görev doğrulama sistemini backend ve frontend'e entegre et

| # | Görev | Detay | Süre |
|---|-------|-------|------|
| 3.1 | Backend: Doğrulama servis interface'leri | `IImageVerificationService`, `IReflectionVerificationService` | 0.5 gün |
| 3.2 | Backend: AI servis HTTP client | Python servisine request atan service | 1 gün |
| 3.3 | Backend: CompleteUserTask handler güncelle | Doğrulama katmanı ekle | 2 gün |
| 3.4 | Backend: Fotoğraf yükleme endpoint | Multipart form data, dosya kaydetme | 1 gün |
| 3.5 | Frontend: TaskItem bileşenini güncelle | Doğrulama tipine göre farklı UI (fotoğraf, metin, timer) | 2 gün |
| 3.6 | Frontend: TaskTimer bileşeni | Zamanlayıcı doğrulama için dairesel timer | 1 gün |
| 3.7 | Frontend: Fotoğraf yükleme bileşeni | Kamera/galeri seçimi, önizleme, crop | 1 gün |
| 3.8 | Frontend: Yansıma yazısı bileşeni | Metin editörü, kelime sayacı, gerçek zamanlı feedback | 1 gün |
| 3.9 | Admin: Doğrulama inceleme sayfası | Bekleyen doğrulamaları listele, onayla/reddet | 1 gün |
| 3.10 | Entegrasyon testi | Tüm doğrulama akışlarını test et | 1 gün |

**Çıktı:** Görev doğrulama sistemi tam çalışır durumda

---

### 🔷 SPRINT 4: Chatbot & Kişisel Asistan (Hafta 7-8)

**Hedef:** AI chatbot'u uygulamaya entegre et

| # | Görev | Detay | Süre |
|---|-------|-------|------|
| 4.1 | Backend: ChatBot controller | `/api/chat/send`, `/api/chat/history` | 1 gün |
| 4.2 | Backend: Chat mesaj kaydetme | ChatBotMessage entity'sini kullan | 0.5 gün |
| 4.3 | Backend: AI chat service | Python servisine message gönder, cevap al, enrichment yap | 2 gün |
| 4.4 | Frontend: Chat bileşeni tasarla | Modern chat UI (WhatsApp/Telegram benzeri) | 2 gün |
| 4.5 | Frontend: Quick reply butonları | AI'ın önerdiği hızlı cevaplar | 0.5 gün |
| 4.6 | Frontend: Chat sayfası ekle | Route ve navigasyon güncelle | 0.5 gün |
| 4.7 | Chatbot cevap şablonlarını zenginleştir | 100+ Türkçe motivasyon cümlesi, veri çekme cevapları | 2 gün |
| 4.8 | Intent training verisini artır | Her intent için 50+ örnek | 1 gün |
| 4.9 | Chatbot entegrasyon testi | Tüm intent senaryolarını test et | 1 gün |

**Çıktı:** Çalışan AI chatbot, kullanıcıyla doğal dilde etkileşim

---

### 🔷 SPRINT 5: Akıllı Öneri Sistemi (Hafta 9-10)

**Hedef:** ML.NET ile görev öneri sistemini entegre et

| # | Görev | Detay | Süre |
|---|-------|-------|------|
| 5.1 | Sentetik eğitim verisi üret | collector.py ile 10K+ veri | 1 gün |
| 5.2 | ML.NET pipeline oluştur | FastTree binary classification | 2 gün |
| 5.3 | Model eğitimi ve değerlendirme | Accuracy, precision, recall metrikleri | 1 gün |
| 5.4 | DailyQuestService'i güncelle | Rastgele yerine ML tabanlı atama | 2 gün |
| 5.5 | Cold start stratejisi | Kural tabanlı → ML geçiş mekanizması | 1 gün |
| 5.6 | Kullanıcı etkileşim verisi toplama | Her görev tamamlamada interaction kaydet | 1 gün |
| 5.7 | Model yeniden eğitim endpoint'i | Admin'in modeli güncel veriyle yeniden eğitmesi | 1 gün |
| 5.8 | A/B test altyapısı | Rastgele vs ML önerileri karşılaştır | 1 gün |
| 5.9 | Test ve optimizasyon | Model performans değerlendirme | 1 gün |

**Çıktı:** Akıllı, kişiselleştirilmiş görev öneri sistemi

---

### 🔷 SPRINT 6: Genişletilmiş Özellikler (Hafta 11-12)

**Hedef:** Hedefler, bildirimler, haftalık rapor

| # | Görev | Detay | Süre |
|---|-------|-------|------|
| 6.1 | Hedef sistemi backend | CRUD + otomatik ilerleme takibi | 2 gün |
| 6.2 | Hedef sistemi frontend | Hedef oluşturma/görüntüleme UI | 2 gün |
| 6.3 | Bildirim sistemi backend | SignalR hub + notification service | 2 gün |
| 6.4 | Bildirim sistemi frontend | Çan ikonu, dropdown, okundu/okunmadı | 1 gün |
| 6.5 | Haftalık rapor oluşturma servisi | BackgroundService ile her hafta sonu | 2 gün |
| 6.6 | Haftalık rapor frontend | Görsel rapor sayfası (grafikler, özet) | 2 gün |
| 6.7 | Achievement sistemi | Otomatik başarı kontrolü ve ödüllendirme | 1 gün |

**Çıktı:** Kapsamlı hedef, bildirim ve rapor sistemi

---

### 🔷 SPRINT 7: Sosyal Özellikler & UI/UX Geliştirme (Hafta 13-14)

**Hedef:** Sosyal etkileşim ve arayüz iyileştirmeleri

| # | Görev | Detay | Süre |
|---|-------|-------|------|
| 7.1 | Arkadaşlık sistemi backend | İstek gönder/kabul et/reddet/listele | 2 gün |
| 7.2 | Mücadele (Challenge) sistemi | Arkadaşa mücadele gönder/takip et | 2 gün |
| 7.3 | Sosyal sayfalar frontend | Arkadaş listesi, mücadele UI | 2 gün |
| 7.4 | Leaderboard sayfasını zenginleştir | Arkadaş sıralaması, kategori bazlı | 1 gün |
| 7.5 | Responsive tasarım | Mobil uyum (sidebar → hamburger menü) | 2 gün |
| 7.6 | Dashboard'a AI widget ekle | Chatbot mini pencere, günlük ipuçları | 1 gün |
| 7.7 | Animasyonlar ve micro-interactions | Framer Motion ile geçiş animasyonları | 1 gün |
| 7.8 | Dark/Light theme toggle | Tema değiştirme özelliği | 1 gün |

**Çıktı:** Sosyal özellikler ve geliştirilmiş UI

---

### 🔷 SPRINT 8: Test, Optimizasyon & Sunum Hazırlığı (Hafta 15-16)

**Hedef:** Son cilalama, testler, sunum

| # | Görev | Detay | Süre |
|---|-------|-------|------|
| 8.1 | Birim testler | Kritik servisler için unit test | 2 gün |
| 8.2 | Entegrasyon testleri | API endpoint'leri, AI servisi | 1 gün |
| 8.3 | Performans optimizasyonu | Sorgu optimizasyonu, cache ekleme | 1 gün |
| 8.4 | Güvenlik kontrolü | Input validasyon, rate limiting, SQL injection koruması | 1 gün |
| 8.5 | Hata yönetimi | Global exception handler, kullanıcı dostu hatalar | 1 gün |
| 8.6 | Dökümantasyon | README, API dökümantasyonu (Swagger), kullanım kılavuzu | 2 gün |
| 8.7 | Sunum materyalleri | PowerPoint, demo senaryoları, poster | 2 gün |
| 8.8 | Demo ortamı hazırlığı | Docker ile tam ortam ayağa kaldırma | 1 gün |
| 8.9 | Son kontrol ve ince ayar | Bug fix, son düzeltmeler | 1 gün |

**Çıktı:** Sunuma hazır, test edilmiş proje

---

## 📊 BÖLÜM 6: BİTİRME PROJESİNİ GÜÇLENDİRECEK AKADEMİK NOKTALAR

### 6.1 Raporda Vurgulanması Gerekenler

> [!TIP]
> Jüri, özellikle **kendi geliştirdiğiniz AI modellerini**, karar mekanizmalarını ve değerlendirme metriklerini görmek isteyecektir.

| Başlık | İçerik |
|--------|--------|
| **Problem Tanımı** | Kişisel gelişim uygulamalarında sahte görev tamamlama ve motivasyon eksikliği |
| **Literatür Taraması** | Gamification teorisi, spaced repetition, NLP in Turkish, transfer learning |
| **Metodoloji** | Clean Architecture, CQRS/MediatR, ML.NET, TF-IDF, Transfer Learning |
| **Model Değerlendirme** | Accuracy, Precision, Recall, F1-Score, Confusion Matrix |
| **Karşılaştırma** | Kural tabanlı vs ML tabanlı öneri karşılaştırması |
| **Etik Değerlendirme** | Kullanıcı verisi gizliliği, AI bias, KVKK uyumluluğu |

### 6.2 Model Değerlendirme Metrikleri

```python
# training/evaluate_models.py
from sklearn.metrics import classification_report, confusion_matrix
import matplotlib.pyplot as plt
import seaborn as sns

def evaluate_intent_classifier(model, X_test, y_test):
    """Intent classifier model değerlendirmesi"""
    predictions = model.predict(X_test)
    
    # Classification Report
    report = classification_report(y_test, predictions, output_dict=True)
    print("\n📊 Intent Classifier Performansı:")
    print(classification_report(y_test, predictions))
    
    # Confusion Matrix
    cm = confusion_matrix(y_test, predictions)
    plt.figure(figsize=(10, 8))
    sns.heatmap(cm, annot=True, fmt='d', cmap='Blues',
                xticklabels=model.classes_, yticklabels=model.classes_)
    plt.title('Intent Classifier - Confusion Matrix')
    plt.ylabel('Gerçek')
    plt.xlabel('Tahmin')
    plt.tight_layout()
    plt.savefig('reports/intent_confusion_matrix.png', dpi=150)
    
    return report

# Bu grafikleri bitirme raporu ve sunumda kullanın!
```

### 6.3 Projeyi Farklılaştıracak Noktalar

1. **Hibrit AI Yaklaşımı**: Hem rule-based hem ML tabanlı
2. **Çok Katmanlı Doğrulama**: Fotoğraf + Metin + Zaman + Quiz
3. **Türkçe NLP**: Türkçe için özelleştirilmiş modeller
4. **Kendi Model Eğitimimiz**: API kullanmadan, sıfırdan eğitim
5. **Mikro Servis Mimarisi**: .NET + Python entegrasyonu
6. **Gerçek Zamanlı İletişim**: SignalR ile anlık bildirimler
7. **Veri Odaklı Kararlar**: Kullanıcı verisiyle model iyileştirme

---

## ⚠️ GENEL TAVSİYELER VE UYARILAR

### Güvenlik
- [ ] JWT token süresini kısalt (şu an muhtemelen uzun)
- [ ] Refresh token mekanizması ekle
- [ ] Rate limiting uygula (özellikle AI endpoint'leri için)
- [ ] Fotoğraf yüklemelerinde dosya boyutu ve tip kontrolü
- [ ] appsettings.json'daki gizli anahtarları environment variable'a taşı
- [ ] CORS politikasını sıkılaştır (`AllowAnyOrigin` yerine specific origin)

### Performans
- [ ] Sık kullanılan sorguları cache'le (Redis veya in-memory)
- [ ] Pagination ekle (leaderboard, görev listesi)
- [ ] Lazy loading (Entity Framework Include optimizasyonu)
- [ ] AI model tahminlerini cache'le (aynı input → aynı sonuç)

### Kod Kalitesi
- [ ] FluentValidation ekle (request validation)
- [ ] Global exception middleware'ı geliştir
- [ ] Logging'i yapılandır (Serilog → dosya/veritabanı)
- [ ] Response wrapper (BaseResponse) standardize et

### Frontend
- [ ] Error boundary ekle
- [ ] Loading state'leri ve skeleton UI
- [ ] React Query veya SWR ile veri yönetimi
- [ ] Form validation (react-hook-form + yup/zod)
- [ ] PWA (Progressive Web App) desteği
- [ ] Lazy loading ile route splitting

---

## 📁 FİNAL PROJE YAPISI

```
LifeQuest/
├── LifeQuestAPI/                          # .NET Backend
│   ├── Core/
│   │   ├── LifeQuestAPI.Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── AppTask.cs             (güncellendi: doğrulama alanları)
│   │   │   │   ├── UserTask.cs
│   │   │   │   ├── TaskVerification.cs    ★ YENİ
│   │   │   │   ├── UserGoal.cs            ★ YENİ
│   │   │   │   ├── Achievement.cs         ★ YENİ
│   │   │   │   ├── Notification.cs        ★ YENİ
│   │   │   │   ├── WeeklyReport.cs        ★ YENİ
│   │   │   │   ├── Friendship.cs          ★ YENİ
│   │   │   │   ├── Challenge.cs           ★ YENİ
│   │   │   │   ├── UserTaskInteraction.cs ★ YENİ (ML eğitim verisi)
│   │   │   │   └── ... (mevcut entity'ler)
│   │   │   └── Enums/
│   │   │       ├── VerificationType.cs    ★ YENİ
│   │   │       ├── VerificationStatus.cs  ★ YENİ
│   │   │       └── ... (mevcut + yeni enum'lar)
│   │   └── LifeQuestAPI.Application/
│   │       ├── Abstractions/
│   │       │   ├── AI/                    ★ YENİ
│   │       │   │   ├── IAIChatService.cs
│   │       │   │   ├── IImageVerificationService.cs
│   │       │   │   ├── IReflectionVerificationService.cs
│   │       │   │   └── ITaskRecommendationService.cs
│   │       │   └── ... (mevcut)
│   │       └── Features/
│   │           ├── Chat/                  ★ YENİ
│   │           ├── Goals/                 ★ YENİ
│   │           ├── Notifications/         ★ YENİ
│   │           ├── Reports/               ★ YENİ
│   │           ├── Social/                ★ YENİ
│   │           └── ... (mevcut feature'lar)
│   ├── Infrastructure/
│   │   └── LifeQuestAPI.Infrastructure/
│   │       └── Services/
│   │           ├── AI/                    ★ YENİ
│   │           │   ├── AIChatService.cs
│   │           │   ├── ImageVerificationService.cs
│   │           │   ├── ReflectionVerificationService.cs
│   │           │   └── TaskRecommendationService.cs (ML.NET)
│   │           └── ... (mevcut servisler)
│   └── Presentation/
│       └── LifeQuestAPI.API/
│           ├── Controllers/
│           │   ├── ChatController.cs      ★ YENİ
│           │   ├── GoalsController.cs     ★ YENİ
│           │   ├── NotificationController.cs ★ YENİ
│           │   ├── SocialController.cs    ★ YENİ
│           │   └── ... (mevcut controller'lar)
│           └── Hubs/
│               └── NotificationHub.cs     ★ YENİ (SignalR)
│
├── ai_service/                            ★ YENİ: Python AI Mikro Servis
│   ├── main.py
│   ├── requirements.txt
│   ├── Dockerfile
│   ├── models/
│   ├── services/
│   │   ├── intent_classifier.py
│   │   ├── sentiment_analyzer.py
│   │   ├── image_verifier.py
│   │   └── reflection_verifier.py
│   ├── training/
│   └── data/
│
├── frontend/
│   └── src/
│       ├── pages/
│       │   ├── ChatPage.jsx               ★ YENİ
│       │   ├── GoalsPage.jsx              ★ YENİ
│       │   ├── ReportsPage.jsx            ★ YENİ
│       │   ├── SocialPage.jsx             ★ YENİ
│       │   └── ... (mevcut sayfalar)
│       ├── components/
│       │   ├── TaskTimer.jsx              ★ YENİ
│       │   ├── PhotoUpload.jsx            ★ YENİ
│       │   ├── ReflectionEditor.jsx       ★ YENİ
│       │   ├── ChatWidget.jsx             ★ YENİ
│       │   ├── NotificationBell.jsx       ★ YENİ
│       │   └── ... (mevcut + yeni bileşenler)
│       └── services/
│           ├── chatService.js             ★ YENİ
│           ├── goalService.js             ★ YENİ
│           ├── notificationService.js     ★ YENİ
│           └── ... (mevcut servisler)
│
├── docker-compose.yml                     ★ YENİ
└── README.md                              (güncellendi)
```

---

## 🎯 ÖNCELİK SIRASI (En Önemliden En Az Önemliye)

1. **🔴 Kritik: Görev Doğrulama Sistemi** — Projenin en zayıf noktası
2. **🔴 Kritik: AI Chatbot** — "Yapay zeka destekli" adını hak etmesi için
3. **🟠 Yüksek: Python AI Mikro Servis** — Tüm AI özelliklerin temeli
4. **🟠 Yüksek: Akıllı Öneri Sistemi** — ML.NET ile görev önerisi
5. **🟡 Orta: Bildirim Sistemi** — UX geliştirme
6. **🟡 Orta: Hedef/Goal Sistemi** — Kullanıcı bağlılığı
7. **🟢 Düşük: Sosyal Özellikler** — Ek değer
8. **🟢 Düşük: Haftalık Rapor** — Güzel ama opsiyonel

---

> [!CAUTION]
> **appsettings.json dosyanızda production token'ı açık şekilde duruyor!** Bunu acilen environment variable'a taşıyın. Ayrıca CORS'da `AllowAnyOrigin` güvenlik riski oluşturur, production'da bunu değiştirin.
