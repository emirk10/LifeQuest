"""
LifeQuest AI Mikro Servis — SP-02 Güncellemesi

Endpoint'ler:
  POST /api/chat         → Chatbot (Context-aware Intent + Sentiment + Cevap + Actions)
  POST /api/recommend-tasks → Görev önerisi (chatbot oneri_iste için)
  POST /api/train        → Modelleri yeniden eğit (Admin)
  GET  /health           → Sağlık kontrolü
"""

import random
import logging
from contextlib import asynccontextmanager
from typing import Optional

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

from services.intent_classifier import IntentClassifier
from services.sentiment_analyzer import SentimentAnalyzer
from services.urgency_detector import UrgencyDetector
from services.action_extractor import ActionExtractor
from services.personality_response_generator import PersonalityResponseGenerator

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(name)s: %(message)s")
logger = logging.getLogger("lifequest_ai")

intent_clf = IntentClassifier()
sentiment_analyzer = SentimentAnalyzer()
urgency_detector = UrgencyDetector()
action_extractor = ActionExtractor()
personality_gen = PersonalityResponseGenerator()


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("🚀 LifeQuest AI Servis başlatılıyor...")
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
    description="SP-02: Bağlam-Duyarlı Chatbot + Kişilik Motoru",
    version="3.0.0",
    lifespan=lifespan,
)

import os

app.add_middleware(
    CORSMiddleware,
    allow_origins=os.environ.get("ALLOWED_ORIGINS", "http://lifequest-api:5020,http://localhost:5020,http://localhost:5173").split(","),
    allow_credentials=False,
    allow_methods=["POST", "GET"],
    allow_headers=["Content-Type"],
)


# ─── Pydantic Şemaları ──────────────────────────────────────────────────────

class TaskContext(BaseModel):
    id: str
    title: str
    status: str
    category: str = ""
    estimated_minutes: Optional[int] = None


class ChatContextPayload(BaseModel):
    user_name: str = ""
    personality: str = "friendly"
    today_tasks: list[TaskContext] = []
    completed_today: int = 0
    current_streak: int = 0
    total_xp: int = 0
    today_mood: Optional[str] = None
    available_minutes: int = 30


class ChatRequest(BaseModel):
    user_id: str
    message: str = Field(..., min_length=1, max_length=1000)
    context: Optional[ChatContextPayload] = None


class ChatAction(BaseModel):
    type: str
    label: str
    task_id: Optional[str] = None
    payload: Optional[dict] = None


class MiniTaskDto(BaseModel):
    task_id: str
    title: str
    category_name: str
    estimated_minutes: Optional[int] = None
    base_xp: int = 0
    difficulty: str = "Easy"
    why_this_task: Optional[str] = None


class ChatResponse(BaseModel):
    intent: str
    intent_confidence: float
    response: str
    sentiment: str
    sentiment_confidence: float
    urgency_level: int = 0
    suggestions: list[str] = []
    actions: list[ChatAction] = []
    suggested_tasks: Optional[list[MiniTaskDto]] = None
    should_show_mood_selector: bool = False

    # Geriye uyumluluk
    confidence: float = 0.0


class RecommendRequest(BaseModel):
    user_id: str
    available_minutes: int = 30
    energy_level: int = 3
    mood: str = "Normal"
    preferred_categories: Optional[list[str]] = None


class RecommendResponse(BaseModel):
    tasks: list[MiniTaskDto] = []
    question_to_ask: Optional[str] = None


class TrainResponse(BaseModel):
    intent_result: dict
    sentiment_result: dict


class HealthResponse(BaseModel):
    status: str
    intent_model_loaded: bool
    sentiment_model_loaded: bool
    version: str


# ─── Response Template'leri ─────────────────────────────────────────────────

RESPONSE_TEMPLATES: dict[str, dict[str, list[str]]] = {
    "motivasyon": {
        "negative": [
            "Herkesin zor günleri olur! Şimdiye kadar başardıklarına bak — bu kadar yol geldiysen devam edebilirsin! 💪",
            "Küçük adımlar büyük değişimler yaratır. Bugün sadece bir şey tamamlamayı hedefle. 🌟",
            "Zor zamanlar güçlü insanlar yaratır. Sen bunun canlı kanıtısın! 💎",
        ],
        "neutral": [
            "Harika gidiyorsun! Devam et, her gün bir adım daha ileri! 💪",
            "İstikrar başarının anahtarı — sen doğru yoldasın! ✨",
        ],
        "positive": [
            "Bu enerjini seviyorum! Hadi bugün ekstra bir görev daha halledelim! 🔥",
            "Sen durdurulamazsın! Bu motivasyonu devam ettir! ⭐",
        ],
    },
    "gorev_sorgula": {"default": [
        "📋 Aktif görevlerini görev sayfasından görebilirsin!",
        "📋 Bugünkü görev listeni kontrol edelim. Hadi bakalım neler var!",
    ]},
    "istatistik": {"default": [
        "📊 İstatistiklerini profil sayfandan detaylıca görebilirsin!",
        "📊 Gelişim grafiğin gerçekten etkileyici.",
    ]},
    "oneri": {"default": [
        "🎯 Sana uygun görevleri hemen buluyorum!",
        "🎯 Bugün kendinle ilgili küçük ama anlamlı bir adım at.",
    ]},
    "oneri_iste": {"default": [
        "🎯 Sana en uygun görevleri seçiyorum...",
        "🎯 Bağlamına göre özel görevler getiriyorum!",
    ]},
    "selamlama": {"default": [
        "Merhaba! 👋 Bugün kendini geliştirmek için hazır mısın?",
        "Selam! 👋 Bugün ne yapmak istersin?",
    ]},
    "saglik": {
        "negative": ["💊 Sağlık her şeyden önemli. Kendine iyi bak!"],
        "neutral": ["💊 Günde 8 bardak su, 30 dakika yürüyüş harika bir başlangıç!"],
        "positive": ["💊 Bu enerjiyle sağlıklı alışkanlıkları destekle! 💪"],
    },
    "rozet": {"default": [
        "🏆 Rozetlerini rozet sayfasından görebilirsin!",
        "🏆 Yeni rozetler kazanmak için görevleri tamamlamaya devam et.",
    ]},
    "gorev_ertele": {"default": [
        "Anladım, bu görevi erteliyoruz. Yarın hatırlatacağım! 📅",
        "Tamam, görevi yarına alıyoruz. Küçük not: %70 XP kazanacaksın.",
    ]},
    "gorev_baslat": {"default": [
        "Harika! Görevi başlatıyoruz. Bol şans! 🚀",
        "Timer başlatılıyor. Odaklan ve başar! ⏱️",
    ]},
    "gorev_tamamla": {"default": [
        "Tebrikler! Görevi tamamladın! 🎉 XP hesaplanıyor...",
        "Harika iş! Görev tamamlandı. 🏆",
    ]},
    "gorev_vazgec": {"default": [
        "Anladım. Görevden vazgeçiyorsun. Bu sefer olmadı ama bir sonrakinde başaracaksın! 💙",
        "Tamam, bu görevi iptal ediyorum. Seninle daha uygun bir görev bulacağız.",
    ]},
    "gorev_sec": {"default": [
        "Harika seçim! Görevi listene ekliyorum. 📋",
        "Tamam, bu görevi seçtik. Başarılar! 🎯",
    ]},
    "mood_negatif": {"default": [
        "Seni duyuyorum, zor bir dönemdesin. Bugün kendine nazik ol. 💙",
        "Bu hisleri yaşamak çok normal. Bir nefes al, küçük adımlarla devam edelim. 🌸",
    ]},
    "mood_pozitif": {"default": [
        "Harika! Bu enerji bulaşıcı! Hadi hep birlikte harika işler yapalım! 🔥",
        "Süper! Bugün çok şey başaracaksın, hissediyorum! ⭐",
    ]},
    "hafif_mod": {"default": [
        "Anlıyorum, bugün hafif tutalım. Sana kolay ve kısa bir görev öneriyorum! 🌿",
        "Enerji tasarrufu modunda! Sana mini bir görev buluyorum. ✨",
    ]},
    "istatistik_sorgula": {"default": [
        "📊 İstatistiklerini getiriyorum...",
        "📊 Profilini kontrol et, tüm veriler orada!",
    ]},
}

SUGGESTION_MAP: dict[str, list[str]] = {
    "motivasyon": ["Görevlerimi göster", "Kolay görev öner", "İstatistiklerimi göster"],
    "gorev_sorgula": ["Yeni görev iste", "Profilimi göster", "Rozetlerimi göster"],
    "istatistik": ["Haftalık rapor", "Rozetlerimi göster", "Hedeflerimi göster"],
    "oneri": ["Spor görevi", "Eğitim görevi", "Sağlık görevi"],
    "oneri_iste": ["Kolay görev öner", "Orta zorluk", "Bugünkü öneriler"],
    "selamlama": ["Bugünkü görevlerim", "İstatistiklerim", "Beni motive et"],
    "saglik": ["Egzersiz öner", "Su içmeyi hatırlat", "Uyku ipuçları"],
    "rozet": ["Tüm rozetler", "Sıradaki hedef", "Görevleri göster"],
    "gorev_ertele": ["Yarın hatırlat", "Başka görev öner", "Görevlerimi göster"],
    "gorev_baslat": ["Timer başlat", "Motivasyon ver", "Görevleri göster"],
    "gorev_tamamla": ["Sıradaki görev", "İstatistiklerimi göster", "XP durumum"],
    "gorev_vazgec": ["Başka görev öner", "Motivasyon ver", "Görevleri göster"],
    "gorev_sec": ["Timer başlat", "Görevlerimi göster", "Motivasyon ver"],
    "mood_negatif": ["Hafif görev öner", "Sadece dinlen", "Motivasyon ver"],
    "mood_pozitif": ["Zor görev öner", "En fazla XP", "Bugünkü hedefler"],
    "hafif_mod": ["Mini görev öner", "5 dakikalık görev", "En kolay görev"],
    "istatistik_sorgula": ["Streak durumum", "XP geçmişim", "Haftalık rapor"],
}


def build_actions(intent: str, context: ChatContextPayload | None, task_id: str | None = None) -> list[ChatAction]:
    """Intent ve bağlama göre aksiyon butonları üret."""
    actions = []
    today_tasks = context.today_tasks if context else []

    if intent == "gorev_ertele" and today_tasks:
        tid = task_id or today_tasks[0].id
        ttitle = next((t.title for t in today_tasks if t.id == tid), today_tasks[0].title)
        actions.append(ChatAction(type="postpone_task", label=f"'{ttitle}' yarına ertele", task_id=tid))

    elif intent == "gorev_baslat" and today_tasks:
        tid = task_id or today_tasks[0].id
        ttitle = next((t.title for t in today_tasks if t.id == tid), today_tasks[0].title)
        actions.append(ChatAction(type="start_task", label=f"'{ttitle}' başlat", task_id=tid))

    elif intent == "gorev_tamamla" and today_tasks:
        tid = task_id or today_tasks[0].id
        ttitle = next((t.title for t in today_tasks if t.id == tid), today_tasks[0].title)
        actions.append(ChatAction(type="complete_task", label=f"'{ttitle}' tamamla", task_id=tid))

    elif intent == "gorev_vazgec" and today_tasks:
        tid = task_id or today_tasks[0].id
        ttitle = next((t.title for t in today_tasks if t.id == tid), today_tasks[0].title)
        actions.append(ChatAction(type="abandon_task", label=f"'{ttitle}' vazgeç", task_id=tid))

    elif intent == "hafif_mod" or intent == "mood_negatif":
        actions.append(ChatAction(type="activate_light_mode", label="Hafif moda geç 🌿"))

    elif intent in ("istatistik", "istatistik_sorgula"):
        actions.append(ChatAction(type="show_stats", label="İstatistiklerimi göster 📊"))

    return actions


def generate_response(
    intent: str,
    confidence: float,
    sentiment_label: str,
    context: ChatContextPayload | None,
    urgency_level: int,
    task_id: str | None = None
) -> str:
    personality = context.personality if context else "friendly"
    user_name = context.user_name if context else ""

    # Urgency 3: Sadece empati
    if urgency_level >= 3:
        return personality_gen.generate("", personality, user_name, is_empathy=True)

    if confidence < 0.3:
        base = "Tam olarak anlayamadım. Görevlerin, istatistiklerin veya motivasyon hakkında konuşabiliriz. 😊"
        return personality_gen.generate(base, personality, user_name)

    intent_responses = RESPONSE_TEMPLATES.get(intent, {})
    if sentiment_label in intent_responses:
        base = random.choice(intent_responses[sentiment_label])
    elif "default" in intent_responses:
        base = random.choice(intent_responses["default"])
    else:
        base = "İsteğini anladım! Hemen yardımcı oluyorum. 😊"

    # Urgency 2: Hafif empati dokunuşu
    if urgency_level == 2:
        base = personality_gen.generate("", personality, user_name, is_empathy=True)
    else:
        base = personality_gen.generate(base, personality, user_name)

    # Bağlam ekleme (görev varsa)
    if context and intent == "gorev_sorgula" and context.today_tasks:
        task_list = "\n".join(f"  • {t.title}" for t in context.today_tasks[:3])
        base += f"\n\nAktif görevlerin:\n{task_list}"
        if context.completed_today > 0:
            base += f"\n\n✅ Bugün {context.completed_today} görev tamamladın! Harika!"

    return base


# ─── Endpoint'ler ──────────────────────────────────────────────────────────

@app.get("/health", response_model=HealthResponse, tags=["System"])
async def health_check():
    return HealthResponse(
        status="ok",
        intent_model_loaded=intent_clf.model is not None,
        sentiment_model_loaded=sentiment_analyzer.model is not None,
        version="3.0.0",
    )


@app.post("/api/chat", response_model=ChatResponse, tags=["Chatbot"])
async def chat(request: ChatRequest):
    """SP-02: Bağlam-duyarlı chat endpoint."""
    try:
        intent_result = intent_clf.predict(request.message)
        sentiment_result = sentiment_analyzer.analyze(request.message)

        urgency = urgency_detector.detect(
            request.message,
            sentiment_result["label"],
            sentiment_result["confidence"]
        )

        # Görev çıkarımı (context varsa)
        extracted_task_id = None
        if request.context and request.context.today_tasks:
            task_dicts = [{"id": t.id, "title": t.title, "status": t.status}
                          for t in request.context.today_tasks]
            extraction = action_extractor.extract(request.message, task_dicts)
            if extraction.confidence >= 0.3:
                extracted_task_id = extraction.task_id

        response_text = generate_response(
            intent_result["intent"],
            intent_result["confidence"],
            sentiment_result["label"],
            request.context,
            urgency,
            extracted_task_id
        )

        actions = build_actions(intent_result["intent"], request.context, extracted_task_id)
        suggestions = SUGGESTION_MAP.get(intent_result["intent"], ["Yardım", "Görevlerim", "Profilim"])

        should_show_mood = urgency >= 2 or intent_result["intent"] in ("mood_negatif", "hafif_mod")

        return ChatResponse(
            intent=intent_result["intent"],
            intent_confidence=intent_result["confidence"],
            confidence=intent_result["confidence"],  # uyumluluk
            response=response_text,
            sentiment=sentiment_result["label"],
            sentiment_confidence=sentiment_result["confidence"],
            urgency_level=urgency,
            suggestions=suggestions,
            actions=actions,
            should_show_mood_selector=should_show_mood,
        )

    except Exception as e:
        logger.error("Chat hatası: %s", e)
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/recommend-tasks", response_model=RecommendResponse, tags=["Recommendations"])
async def recommend_tasks(request: RecommendRequest):
    """SP-02: Chatbot 'görev öner' intent'i için bağlam bazlı öneriler."""
    # Not: Gerçek görev verisi .NET backend'den gelecek.
    # Bu endpoint şu an placeholder olarak çalışır; SP-03'te ML entegrasyonu yapılacak.
    question = None
    if request.energy_level <= 2:
        question = "Enerji seviyeniz düşük, hafif görevler mi istersiniz?"
    return RecommendResponse(tasks=[], question_to_ask=question)


@app.post("/api/train", response_model=TrainResponse, tags=["Admin"])
async def train_models():
    try:
        intent_result = intent_clf.train()
        sentiment_result = sentiment_analyzer.train()
        logger.info("✅ Modeller yeniden eğitildi!")
        return TrainResponse(intent_result=intent_result, sentiment_result=sentiment_result)
    except Exception as e:
        logger.error("Model eğitim hatası: %s", e)
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=5001, reload=True, log_level="info")
