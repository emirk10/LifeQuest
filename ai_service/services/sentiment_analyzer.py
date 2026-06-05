"""
LifeQuest AI - Sentiment Analyzer (Duygu Analizi)

Kullanıcı mesajlarının duygusal tonunu analiz eder:
  positive, negative, neutral

Algoritma: Custom Preprocessing + TF-IDF (char_wb) + LogisticRegression
"""

import os
import json
import pickle
import re
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import Pipeline

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DATA_PATH = os.path.join(BASE_DIR, "data", "sentiments_tr.json")
MODEL_PATH = os.path.join(BASE_DIR, "models", "sentiment_model.pkl")


def turkish_preprocess(text: str) -> str:
    """Türkçe özel karakterleri ve noktalama işaretlerini temizler."""
    if not isinstance(text, str):
        return ""
    text = text.replace("I", "ı").replace("İ", "i").lower()
    text = re.sub(r"[^\w\s]", " ", text)
    
    words = text.split()
    cleaned = []
    # Duygu analizinde 'değil', 'yok' gibi kelimeler çok kritik olduğu için stopword listesini çok kısa tutuyoruz
    stopwords = {"bir", "ve", "ile", "için", "mı", "mi", "mu", "mü"}
    
    for w in words:
        if w in stopwords:
            continue
        cleaned.append(w)
        
    return " ".join(cleaned)


class SentimentAnalyzer:
    """Türkçe metin tabanlı duygu analizi servisi."""

    def __init__(self):
        self.model: Pipeline | None = None
        self._load_model()

    # ------------------------------------------------------------------ #
    #  EĞİTİM
    # ------------------------------------------------------------------ #

    def train(self) -> dict:
        texts, labels = self._load_training_data()

        self.model = Pipeline([
            ("tfidf", TfidfVectorizer(
                preprocessor=turkish_preprocess,
                analyzer="char_wb",
                ngram_range=(2, 6),
                max_features=10000,
                sublinear_tf=True,
            )),
            ("clf", LogisticRegression(C=2.0, class_weight="balanced", max_iter=1000)),
        ])

        self.model.fit(texts, labels)

        os.makedirs(os.path.dirname(MODEL_PATH), exist_ok=True)
        with open(MODEL_PATH, "wb") as f:
            pickle.dump(self.model, f)

        return {
            "status": "success",
            "samples": len(texts),
            "classes": list(set(labels)),
        }

    def _load_training_data(self) -> tuple[list[str], list[str]]:
        if not os.path.exists(DATA_PATH):
            raise FileNotFoundError(f"Eğitim verisi bulunamadı: {DATA_PATH}")

        with open(DATA_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)

        texts = []
        labels = []
        for sentiment, examples in data.get("sentiments", {}).items():
            for text in examples:
                texts.append(text)
                labels.append(sentiment)

        return texts, labels

    # ------------------------------------------------------------------ #
    #  TAHMİN
    # ------------------------------------------------------------------ #

    def analyze(self, text: str) -> dict:
        if not self.model:
            self.train()

        # Kısa mesajlarda da modelin tahmin yapmasına izin verelim

        proba = self.model.predict_proba([text])[0]
        classes = self.model.classes_

        best_idx = proba.argmax()
        sentiment = classes[best_idx]
        confidence = proba[best_idx]

        # Güven skoru düşükse "neutral"
        if confidence < 0.45:
            sentiment = "neutral"

        return {
            "label": sentiment,
            "confidence": round(confidence, 4)
        }

    # ------------------------------------------------------------------ #
    #  YARDIMCI METOTLAR
    # ------------------------------------------------------------------ #

    def _load_model(self):
        if os.path.exists(MODEL_PATH):
            try:
                with open(MODEL_PATH, "rb") as f:
                    self.model = pickle.load(f)
            except Exception:
                self.model = None

