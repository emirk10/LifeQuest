"""
LifeQuest AI Mikro Servis — Ana Giriş Noktası

FastAPI tabanlı AI servisi. .NET backend'den gelen istekleri karşılar.

Endpoint'ler:
  POST /api/chat              → Chatbot (Intent + Sentiment + Cevap)
  POST /api/verify-reflection → Yansıma metni doğrulama
  POST /api/verify-image      → Görüntü doğrulama
  POST /api/train             → Modelleri yeniden eğit
  GET  /health                → Sağlık kontrolü

Çalıştırma:
  uvicorn main:app --reload --port 5001
"""

import random
import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

from services.intent_classifier import IntentClassifier
from services.sentiment_analyzer import SentimentAnalyzer
from services.reflection_verifier import ReflectionVerifier
from services.image_verifier import ImageVerifier

# ------------------------------------------------------------------ #
#  LOGGING
# ------------------------------------------------------------------ #

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
)
logger = logging.getLogger("lifequest_ai")

# ------------------------------------------------------------------ #
#  SERVİS INSTANCE'LARI
# ------------------------------------------------------------------ #

intent_clf = IntentClassifier()
sentiment_analyzer = SentimentAnalyzer()
reflection_verifier = ReflectionVerifier()
image_verifier = ImageVerifier()


# ------------------------------------------------------------------ #
#  UYGULAMA BAŞLATMA
# ------------------------------------------------------------------ #

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Uygulama başlangıcında modelleri eğit (eğer henüz yoksa)."""
    logger.info("🚀 LifeQuest AI Servis başlatılıyor...")

    # Modellerinin eğitilmesini sağla
    if intent_clf.model is None:
        logger.info("Intent modeli eğitiliyor...")
        result = intent_clf.train()
        logger.info("Intent model: %s", result)

    if sentiment_analyzer.model is None:
        logger.info("Sentiment modeli eğitiliyor...")
        result = sentiment_analyzer.train()
        logger.info("Sentiment model: %s", result)

    logger.info("✅ Tüm modeller hazır!")
    yield
    logger.info("🛑 LifeQuest AI Servis kapatılıyor...")


app = FastAPI(
    title="LifeQuest AI Service",
    description="Yapay Zeka Destekli Kişisel Gelişim — AI Mikro Servis",
    version="1.0.0",
    lifespan=lifespan,
)

# CORS ayarları (.NET backend'in erişimi için)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ------------------------------------------------------------------ #
#  PYDANTIC ŞEMALARI
# ------------------------------------------------------------------ #

class ChatRequest(BaseModel):
    user_id: str
    message: str = Field(..., min_length=1, max_length=1000)


class ChatResponse(BaseModel):
    intent: str
    confidence: float
    response: str
    sentiment: str
    sentiment_confidence: float
    suggestions: list[str] = []


class ReflectionVerifyRequest(BaseModel):
    text: str = Field(..., min_length=1)
    min_word_count: int = Field(default=10, ge=1)
    task_category: str | None = None


class ReflectionVerifyResponse(BaseModel):
    is_valid: bool
    word_count: int
    quality_score: float
    relevance_score: float
    spam_detected: bool
    issues: list[str]
    message: str


class ImageVerifyRequest(BaseModel):
    user_id: str
    task_id: str
    task_category: str
    image_base64: str = Field(..., min_length=10)


class ImageVerifyResponse(BaseModel):
    is_valid: bool
    confidence: float
    detected_labels: list[dict] = []
    expected_category: str
    message: str


class TrainResponse(BaseModel):
    intent_result: dict
    sentiment_result: dict


class HealthResponse(BaseModel):
    status: str
    intent_model_loaded: bool
    sentiment_model_loaded: bool
    version: str


# ------------------------------------------------------------------ #
#  CHATBOT CEVAP MOTORU
# ------------------------------------------------------------------ #

RESPONSE_TEMPLATES: dict[str, dict[str, list[str]]] = {
    "motivasyon": {
        "negative": [
            "Herkesin zor günleri olur! Senin şimdiye kadar başardıklarına bak — bu kadar yol geldiysen devam edebilirsin! 💪",
            "Küçük adımlar büyük değişimler yaratır. Bugün sadece bir görev tamamlamayı hedefle, gerisini yarına bırak. 🌟",
            "Unutma, başarısızlık bir son değil, öğrenme fırsatıdır. Sana inanıyorum! 🚀",
            "Kendine çok yüklenme. Bazen en iyi motivasyon küçük bir mola vermekten gelir. Sonra daha güçlü döneceksin! ☕",
            "Bugün zor olabilir, ama yarın daha iyi olacak. Her gece ardından bir sabah gelir. 🌅",
            "Senin potansiyelini görüyorum! Bu geçici bir dönem — birlikte aşacağız. 🤝",
            "Zor zamanlar güçlü insanlar yaratır. Sen bunun canlı kanıtısın! 💎",
            "Bir adım geri attıysan, iki adım ileri atacaksın. Bu sadece bir ara, son değil! 🎯",
        ],
        "neutral": [
            "Harika gidiyorsun! Devam et, her gün bir adım daha ileri! 💪",
            "Düzenli çalışma alışkanlığın seni zirveye taşıyacak! 🎯",
            "İstikrar başarının anahtarı — sen doğru yoldasın! ✨",
            "Her tamamladığın görev seni bir üst seviyeye taşıyor, bunu unutma! 📈",
        ],
        "positive": [
            "Bu enerjini seviyorum! Hadi bugün ekstra bir görev daha halledelim! 🔥",
            "Pozitif enerjin bulaşıcı! Bu gidişle yeni rozetler kazanacaksın! 🏆",
            "Sen durdurulamazsın! Bu motivasyonu devam ettir, harikalar yaratıyorsun! ⭐",
            "Aynı bu enerjiyle devam! İnanılmaz bir tempo yakalamışsın! 🚀",
        ],
    },
    "gorev_sorgula": {
        "default": [
            "📋 Görevlerini kontrol ediyorum! Aktif görevlerini profil sayfandan görebilirsin.",
            "📋 Günlük görevlerini görmek için görev listesine göz at! Her tamamladığın görev seni ileriye taşır.",
            "📋 Bugünkü görev listeni kontrol edelim. Hadi bakalım neler var!",
            "📋 Görevlerin seni bekliyor! Hangisiyle başlamak istersin?",
        ],
    },
    "istatistik": {
        "default": [
            "📊 İstatistiklerini profil sayfandan detaylıca görebilirsin! Gelişimin harika gidiyor.",
            "📊 Performansını takip etmek harika bir alışkanlık! Profil sayfanda tüm detaylar var.",
            "📊 İstatistiklerin hazır! Gelişim grafiğin gerçekten etkileyici.",
            "📊 Verilerine göre güzel ilerliyorsun. Detaylar için profil sayfanı kontrol et!",
        ],
    },
    "oneri": {
        "default": [
            "🎯 Sana uygun görevleri bulmam için görev listesine göz at! Seviyene uygun görevler seni bekliyor.",
            "🎯 Yeni bir şey denemek istiyorsan, farklı kategorilerdeki görevlere bak! Çeşitlilik gelişimi hızlandırır.",
            "🎯 Bugün kendinle ilgili küçük ama anlamlı bir adım at. Meditasyon, yürüyüş veya 10 dakika okuma gibi!",
            "🎯 Başlamak için harika bir zaman! Kolay bir görevle başla, momentum oluştur.",
            "🎯 Spor, eğitim, sağlık... Hangi alanda gelişmek istersen, görev listende mutlaka bir şey bulacaksın!",
        ],
    },
    "selamlama": {
        "default": [
            "Merhaba! 👋 Bugün kendini geliştirmek için hazır mısın? Görevlerini kontrol edebilir, istatistiklerini görebilir veya sohbet edebiliriz!",
            "Selam! 👋 LifeQuest'e hoş geldin! Bugün ne yapmak istersin? Görevler, istatistikler, yoksa biraz motivasyon mu?",
            "Hey! 👋 Seni görmek güzel! Bugün birlikte harika işler başaracağız!",
            "Hoş geldin! 👋 Hazırsan bugünün görevlerine birlikte bakalım!",
            "Günaydın! ☀️ Yeni bir gün, yeni fırsatlar! Bugün ne yapmak istiyorsun?",
        ],
    },
    "saglik": {
        "negative": [
            "💊 Sağlık her şeyden önemli. Kendine iyi bak! Bugün bol su iç, erken yat ve stresten uzak dur.",
            "💊 Stresle başa çıkmanın en iyi yolu küçük adımlar: 5 dakika nefes egzersizi dene, fark yaratacak!",
            "💊 Uyku düzenin bozuksa, yatmadan 1 saat önce ekranlardan uzak dur. Vücudun sana teşekkür edecek!",
        ],
        "neutral": [
            "💊 Sağlıklı yaşam önerileri: Günde 8 bardak su, 30 dakika yürüyüş ve 7-8 saat uyku harika bir başlangıç!",
            "💊 Meditasyon ve nefes egzersizleri stresi azaltmak için çok etkili. Günde 10 dakika yeterli!",
            "💊 Sağlıklı beslenme ile başla: Daha fazla meyve, sebze ve protein. Hazır gıdaları azalt!",
        ],
        "positive": [
            "💊 Harika bir enerji! Bu enerjiyi sağlıklı alışkanlıklarla destekle: Bol su, düzenli spor, yeterli uyku! 💪",
            "💊 Sağlığına önem verdiğin için tebrikler! Bugün 30 dakika yürüyüş yapmaya ne dersin? ☀️",
        ],
    },
    "rozet": {
        "default": [
            "🏆 Rozetlerini profil sayfandan görebilirsin! Yeni rozetler kazanmak için görevleri tamamlamaya devam et.",
            "🏆 Her rozet bir başarı hikayesi! Hangi rozeti kazanmak istiyorsun? Hedefe kilitlen!",
            "🏆 Rozet koleksiyonun büyüyor! Sıradaki rozet için hangi görevleri yapman gerektiğini kontrol et.",
            "🏆 Başarıların seni tanımlıyor! Yeni rozetler almak için farklı kategorilerde görevler tamamla.",
        ],
    },
}

SUGGESTION_MAP: dict[str, list[str]] = {
    "motivasyon": ["Görevlerimi göster", "Bana kolay görev öner", "İstatistiklerimi göster"],
    "gorev_sorgula": ["Yeni görev iste", "Profilimi göster", "Rozetlerimi göster"],
    "istatistik": ["Haftalık rapor", "En iyi olduğum kategori", "Hedeflerimi göster"],
    "oneri": ["Spor görevi", "Eğitim görevi", "Sağlık görevi"],
    "selamlama": ["Bugünkü görevlerim", "İstatistiklerim", "Beni motive et"],
    "saglik": ["Egzersiz öner", "Su içmeyi hatırlat", "Uyku ipuçları"],
    "rozet": ["Tüm rozetler", "Sıradaki hedef", "Görevleri göster"],
}


def generate_response(intent: str, confidence: float, sentiment_label: str) -> str:
    """Intent ve duygu durumuna göre Türkçe cevap üret."""

    if confidence < 0.3:
        return (
            "Tam olarak anlayamadım. Görevlerin, istatistiklerin veya "
            "motivasyon hakkında konuşabiliriz. 😊 Ne hakkında yardım istersin?"
        )

    intent_responses = RESPONSE_TEMPLATES.get(intent, {})

    # Duygu durumuna göre cevap seç
    if sentiment_label in intent_responses:
        return random.choice(intent_responses[sentiment_label])
    elif "default" in intent_responses:
        return random.choice(intent_responses["default"])
    else:
        return "İsteğini anladım! Hemen yardımcı oluyorum. 😊"


def get_suggestions(intent: str) -> list[str]:
    """Quick reply önerileri."""
    return SUGGESTION_MAP.get(intent, ["Yardım", "Görevlerim", "Profilim"])


# ------------------------------------------------------------------ #
#  ENDPOINT'LER
# ------------------------------------------------------------------ #

@app.get("/health", response_model=HealthResponse, tags=["System"])
async def health_check():
    """Servis sağlık kontrolü."""
    return HealthResponse(
        status="ok",
        intent_model_loaded=intent_clf.model is not None,
        sentiment_model_loaded=sentiment_analyzer.model is not None,
        version="1.0.0",
    )


@app.post("/api/chat", response_model=ChatResponse, tags=["Chatbot"])
async def chat(request: ChatRequest):
    """Kullanıcı mesajını analiz et ve yanıt üret.

    1. Intent sınıflandırma
    2. Duygu analizi
    3. Cevap üretimi
    """
    try:
        # Intent tahmin et
        intent_result = intent_clf.predict(request.message)

        # Duygu analizi
        sentiment_result = sentiment_analyzer.analyze(request.message)

        # Cevap üret
        response_text = generate_response(
            intent_result["intent"],
            intent_result["confidence"],
            sentiment_result["label"],
        )

        return ChatResponse(
            intent=intent_result["intent"],
            confidence=intent_result["confidence"],
            response=response_text,
            sentiment=sentiment_result["label"],
            sentiment_confidence=sentiment_result["confidence"],
            suggestions=get_suggestions(intent_result["intent"]),
        )

    except Exception as e:
        logger.error("Chat hatası: %s", e)
        raise HTTPException(status_code=500, detail=str(e))


@app.post(
    "/api/verify-reflection",
    response_model=ReflectionVerifyResponse,
    tags=["Verification"],
)
async def verify_reflection(request: ReflectionVerifyRequest):
    """Görev tamamlama yansıma metnini doğrula."""
    try:
        result = reflection_verifier.verify(
            text=request.text,
            min_word_count=request.min_word_count,
            task_category=request.task_category,
        )
        return ReflectionVerifyResponse(**result)

    except Exception as e:
        logger.error("Reflection doğrulama hatası: %s", e)
        raise HTTPException(status_code=500, detail=str(e))


@app.post(
    "/api/verify-image",
    response_model=ImageVerifyResponse,
    tags=["Verification"],
)
async def verify_image(request: ImageVerifyRequest):
    """Görev tamamlama fotoğrafını doğrula."""
    try:
        result = image_verifier.verify(
            image_base64=request.image_base64,
            expected_category=request.task_category,
        )
        return ImageVerifyResponse(**result)

    except Exception as e:
        logger.error("Image doğrulama hatası: %s", e)
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/train", response_model=TrainResponse, tags=["Admin"])
async def train_models():
    """Tüm modelleri yeniden eğit (Admin tarafından tetiklenir)."""
    try:
        intent_result = intent_clf.train()
        sentiment_result = sentiment_analyzer.train()

        logger.info("✅ Modeller yeniden eğitildi!")

        return TrainResponse(
            intent_result=intent_result,
            sentiment_result=sentiment_result,
        )

    except Exception as e:
        logger.error("Model eğitim hatası: %s", e)
        raise HTTPException(status_code=500, detail=str(e))


# ------------------------------------------------------------------ #
#  DOĞRUDAN ÇALIŞTIRMA
# ------------------------------------------------------------------ #

if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=5001,
        reload=True,
        log_level="info",
    )
