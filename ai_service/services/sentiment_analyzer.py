"""
LifeQuest AI - Sentiment Analyzer (Duygu Analizi)

Kullanıcı mesajlarının duygusal tonunu analiz eder:
  positive, negative, neutral

Algoritma: TF-IDF (char n-gram) + SVM (Linear Kernel)
"""

import os
import json
import pickle
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.svm import SVC
from sklearn.pipeline import Pipeline

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DATA_PATH = os.path.join(BASE_DIR, "data", "sentiments_tr.json")
MODEL_PATH = os.path.join(BASE_DIR, "models", "sentiment_model.pkl")


class SentimentAnalyzer:
    """Türkçe metin tabanlı duygu analizi servisi."""

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
                analyzer="char_wb",
                ngram_range=(2, 4),
                max_features=3000,
                sublinear_tf=True,
            )),
            ("svc", SVC(
                kernel="linear",
                probability=True,
                C=1.0,
                class_weight="balanced",
            )),
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

    # ------------------------------------------------------------------ #
    #  ANALİZ
    # ------------------------------------------------------------------ #

    def analyze(self, text: str) -> dict:
        """Verilen metnin duygusunu analiz et.

        Returns:
            {
                "label": str,        # positive | negative | neutral
                "confidence": float, # Güven skoru (0-1)
                "scores": dict       # Tüm sınıf olasılıkları
            }
        """
        if self.model is None:
            self.train()

        cleaned = text.lower().strip()

        label = self.model.predict([cleaned])[0]
        probas = self.model.predict_proba([cleaned])[0]
        confidence = float(max(probas))

        scores = {
            cls: round(float(prob), 4)
            for cls, prob in zip(self.model.classes_, probas)
        }

        return {
            "label": label,
            "confidence": round(confidence, 4),
            "scores": scores,
        }

    # ------------------------------------------------------------------ #
    #  YARDIMCI METODLAR
    # ------------------------------------------------------------------ #

    def _load_training_data(self) -> tuple[list[str], list[str]]:
        """sentiments_tr.json dosyasından eğitim verisini yükle."""

        with open(DATA_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)

        texts: list[str] = []
        labels: list[str] = []

        for sentiment_label, examples in data["sentiments"].items():
            for example in examples:
                texts.append(example.lower().strip())
                labels.append(sentiment_label)

        return texts, labels

    def _load_model(self):
        """Daha önce eğitilmiş modeli diskten yükle."""
        if os.path.exists(MODEL_PATH):
            try:
                with open(MODEL_PATH, "rb") as f:
                    self.model = pickle.load(f)
            except Exception:
                self.model = None
