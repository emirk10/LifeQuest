"""
LifeQuest AI - Intent Classifier (Niyet Sınıflandırıcı)

Kullanıcı mesajlarını 7 farklı niyete sınıflandırır:
  motivasyon, gorev_sorgula, istatistik, oneri, selamlama, saglik, rozet

Algoritma: TF-IDF (char n-gram) + Multinomial Naive Bayes
"""

import os
import json
import pickle
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.naive_bayes import MultinomialNB
from sklearn.pipeline import Pipeline

# Proje kök dizini (ai_service/)
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DATA_PATH = os.path.join(BASE_DIR, "data", "intents_tr.json")
MODEL_PATH = os.path.join(BASE_DIR, "models", "intent_model.pkl")


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
                analyzer="char_wb",      # Türkçe ekleri için karakter n-gram
                ngram_range=(2, 5),
                max_features=5000,
                sublinear_tf=True,
            )),
            ("clf", MultinomialNB(alpha=0.1)),
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

    # ------------------------------------------------------------------ #
    #  TAHMİN
    # ------------------------------------------------------------------ #

    def predict(self, text: str) -> dict:
        """Verilen metni sınıflandır.

        Returns:
            {
                "intent": str,          # Tahmin edilen niyet
                "confidence": float,    # Güven skoru (0-1)
                "all_scores": dict      # Tüm sınıfların skorları
            }
        """
        if self.model is None:
            # Model henüz yüklenmemişse eğit
            self.train()

        cleaned = text.lower().strip()

        intent = self.model.predict([cleaned])[0]
        probas = self.model.predict_proba([cleaned])[0]
        confidence = float(max(probas))

        all_scores = {
            cls: round(float(prob), 4)
            for cls, prob in zip(self.model.classes_, probas)
        }

        return {
            "intent": intent,
            "confidence": round(confidence, 4),
            "all_scores": all_scores,
        }

    # ------------------------------------------------------------------ #
    #  YARDIMCI METODLAR
    # ------------------------------------------------------------------ #

    def _load_training_data(self) -> tuple[list[str], list[str]]:
        """intents_tr.json dosyasından eğitim verisini yükle."""

        with open(DATA_PATH, "r", encoding="utf-8") as f:
            data = json.load(f)

        texts: list[str] = []
        labels: list[str] = []

        for intent_name, examples in data["intents"].items():
            for example in examples:
                texts.append(example.lower().strip())
                labels.append(intent_name)

        return texts, labels

    def _load_model(self):
        """Daha önce eğitilmiş modeli diskten yükle."""
        if os.path.exists(MODEL_PATH):
            try:
                with open(MODEL_PATH, "rb") as f:
                    self.model = pickle.load(f)
            except Exception:
                self.model = None
