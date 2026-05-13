"""
LifeQuest AI - Intent Classifier (Niyet Sınıflandırıcı)

Kullanıcı mesajlarını 7 farklı niyete sınıflandırır:
  motivasyon, gorev_sorgula, istatistik, oneri, selamlama, saglik, rozet

Algoritma: Custom Preprocessing + TF-IDF (char_wb) + LogisticRegression
"""

import os
import json
import pickle
import re
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import Pipeline

# Proje kök dizini (ai_service/)
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DATA_PATH = os.path.join(BASE_DIR, "data", "intents_tr.json")
MODEL_PATH = os.path.join(BASE_DIR, "models", "intent_model.pkl")


def turkish_preprocess(text: str) -> str:
    """Türkçe özel karakterleri, noktalama işaretlerini ve basit ekleri temizler."""
    if not isinstance(text, str):
        return ""
    # Küçük harfe çevirme (Türkçe karakter duyarlı)
    text = text.replace("I", "ı").replace("İ", "i").lower()
    # Noktalama işaretlerini sil
    text = re.sub(r"[^\w\s]", " ", text)
    
    words = text.split()
    cleaned = []
    stopwords = {"bir", "ve", "ile", "için", "da", "de", "mı", "mi", "mu", "mü", "bu", "şu", "o"}
    
    for w in words:
        if w in stopwords:
            continue
        # Günlük dildeki yaygın ekleri (basitçe) silerek köke yaklaş
        w = re.sub(r"(yorum|yorsun|yoruz|yorsunuz|yorlar|ıcam|icem|acağım|eceğim|cam|cem|mıcam|micam|mayacağım|meyeceğim|mıyorum|miyorum)$", "", w)
        cleaned.append(w)
        
    return " ".join(cleaned)


class IntentClassifier:
    """Türkçe metin tabanlı niyet sınıflandırıcı."""

    def __init__(self):
        self.model: Pipeline | None = None
        self._load_model()

    # ------------------------------------------------------------------ #
    #  EĞİTİM
    # ------------------------------------------------------------------ #

    def train(self) -> dict:
        """Eğitim verisini yükle, modeli eğit ve diske kaydet."""

        texts, labels = self._load_training_data()

        self.model = Pipeline([
            ("tfidf", TfidfVectorizer(
                preprocessor=turkish_preprocess,
                analyzer="char_wb",      # Türkçe ekleri için karakter n-gram çok etkilidir
                ngram_range=(2, 6),
                max_features=15000,
                sublinear_tf=True,
            )),
            # LogisticRegression metin sınıflandırmada (özellikle çok sınıflı problemlerde) daha kararlı çalışır
            ("clf", LogisticRegression(C=2.0, class_weight="balanced", max_iter=1000)),
        ])

        self.model.fit(texts, labels)

        # models/ klasörünü oluştur
        os.makedirs(os.path.dirname(MODEL_PATH), exist_ok=True)
        with open(MODEL_PATH, "wb") as f:
            pickle.dump(self.model, f)

        return {
            "status": "success",
            "samples": len(texts),
            "classes": list(set(labels)),
        }

    def _load_training_data(self) -> tuple[list[str], list[str]]:
        """JSON dosyasından eğitim verilerini oku."""
        if not os.path.exists(DATA_PATH):
            raise FileNotFoundError(f"Eğitim verisi bulunamadı: {DATA_PATH}")

        with open(DATA_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)

        texts = []
        labels = []
        for intent, examples in data.get("intents", {}).items():
            for text in examples:
                texts.append(text)
                labels.append(intent)

        return texts, labels

    # ------------------------------------------------------------------ #
    #  TAHMİN (PREDICT)
    # ------------------------------------------------------------------ #

    def predict(self, text: str) -> dict:
        """Metnin hangi niyete (intent) ait olduğunu tahmin et."""
        # 1. Önce kural tabanlı (keyword) kontrol
        kw_match = self._check_keywords(text.lower())
        if kw_match:
            return kw_match

        # 2. Model yoksa eğit
        if not self.model:
            self.train()

        # 3. ML ile tahmin (Olasılıklarla)
        proba = self.model.predict_proba([text])[0]
        classes = self.model.classes_

        # En yüksek olasılıklı sınıf
        best_idx = proba.argmax()
        intent = classes[best_idx]
        confidence = proba[best_idx]

        # Güven skoru çok düşükse "alakasiz" döndür (Fallback)
        if confidence < 0.35:
            return {
                "intent": "alakasiz",
                "confidence": round(confidence, 4),
                "method": "low_confidence_fallback"
            }

        return {
            "intent": intent,
            "confidence": round(confidence, 4),
            "method": "ml"
        }

    def _check_keywords(self, text: str) -> dict | None:
        """Kritik anahtar kelimelere göre anında niyet tespiti."""
        rules = {
            "gorev_baslat": ["başlat", "başla", "start", "timer aç", "odağa geç"],
            "gorev_tamamla": ["bitirdim", "bitti", "tamamladım", "check", "onayla", "done"],
            "gorev_ertele": ["ertele", "yarına al", "sonra yapacağım", "postpone"],
            "gorev_vazgec": ["vazgeç", "iptal et", "yapmayacağım", "sil bunu", "çalışmayacağım", "çalışmıcam", "yapmıcam"],
            "gorev_sec": ["seç", "havuzdan al", "listeme ekle"],
            "hafif_mod": ["yoruldum", "hafif olsun", "enerjim düşük", "zorlama"],
            "istatistik": ["rapor", "grafik", "analiz", "nasılım", "gelişim"],
            "rozet": ["rozetler", "başarılar", "ödüller", "medal"],
            "selamlama": ["selam", "merhaba", "hey", "naber", "günaydın", "iyi akşamlar"],
            "yardim": ["yardım", "ne yapabilirsin", "nasıl kullanılır", "help", "menüyü göster",
                        "ana menü", "seçenekler", "komutlar", "neler yapabilirsin", "başa dön"],
            "oneri_iste": ["öner", "ne yapayım", "görev ver", "tavsiye"],
            "alakasiz": ["oyun oynayacağım", "cs oynayacağım", "para lazım", "zengin", "hava durumu", "futbol", "film izleyeceğim", "dizi izleyeceğim", "çalışmayacağım", "oyun oynayacağım"],
        }

        # text'i kelimelere ayırarak tam eşleşme kontrolü (daha sağlam bir yöntem)
        cleaned_text = turkish_preprocess(text)
        
        for intent, keywords in rules.items():
            for kw in keywords:
                # Tam kelime veya basit parça eşleşmesi
                if kw in text or kw in cleaned_text:
                    return {
                        "intent": intent,
                        "confidence": 1.0,
                        "method": "keyword"
                    }
        return None

    # ------------------------------------------------------------------ #
    #  YARDIMCI METOTLAR
    # ------------------------------------------------------------------ #

    def _load_model(self):
        """Eğitilmiş modeli diskten yükle."""
        if os.path.exists(MODEL_PATH):
            try:
                with open(MODEL_PATH, "rb") as f:
                    self.model = pickle.load(f)
            except Exception:
                self.model = None

