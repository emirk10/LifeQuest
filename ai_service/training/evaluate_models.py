"""
LifeQuest AI - Model Değerlendirme Aracı

Eğitilmiş modellerin performansını değerlendirir:
  - Cross-validation ile accuracy hesabı
  - Confusion matrix
  - Classification report (precision, recall, f1-score)

Kullanım:
  cd ai_service
  python -m training.evaluate_models
"""

import os
import sys
import json
import io
import numpy as np
from sklearn.model_selection import cross_val_score, StratifiedKFold
from sklearn.metrics import classification_report, confusion_matrix
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.pipeline import Pipeline

# Türkçe metin ön işleme fonksiyonu
from services.intent_classifier import turkish_preprocess

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def load_intent_data() -> tuple[list[str], list[str]]:
    """Intent eğitim verisini yükle."""
    data_path = os.path.join(BASE_DIR, "data", "intents_tr.json")
    with open(data_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    texts, labels = [], []
    for intent_name, examples in data["intents"].items():
        for ex in examples:
            texts.append(ex.lower().strip())
            labels.append(intent_name)
    return texts, labels


def load_sentiment_data() -> tuple[list[str], list[str]]:
    """Sentiment eğitim verisini yükle."""
    data_path = os.path.join(BASE_DIR, "data", "sentiments_tr.json")
    with open(data_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    texts, labels = [], []
    for label, examples in data["sentiments"].items():
        for ex in examples:
            texts.append(ex.lower().strip())
            labels.append(label)
    return texts, labels


def evaluate_model(
    name: str,
    texts: list[str],
    labels: list[str],
    pipeline: Pipeline,
    n_folds: int = 5,
):
    """Bir modeli cross-validation ile değerlendir ve rapor yazdır."""

    print(f"\n{'=' * 60}")
    print(f"  [EVAL] {name} -- Model Degerlendirmesi")
    print(f"{'=' * 60}")
    print(f"  Toplam örnek: {len(texts)}")
    print(f"  Sınıf sayısı: {len(set(labels))}")
    print(f"  Sınıflar: {sorted(set(labels))}")

    # ---- Cross-Validation ----
    cv = StratifiedKFold(n_splits=n_folds, shuffle=True, random_state=42)
    scores = cross_val_score(pipeline, texts, labels, cv=cv, scoring="accuracy")

    print(f"\n  [CV] {n_folds}-Fold Cross-Validation:")
    print(f"     Ortalama Accuracy: {scores.mean():.4f} (+/-{scores.std():.4f})")
    print(f"     Fold skorlari: {[f'{s:.4f}' for s in scores]}")

    # ---- Tam model eğitimi & detaylı rapor ----
    pipeline.fit(texts, labels)
    predictions = pipeline.predict(texts)

    print(f"\n  [REPORT] Classification Report (egitim seti uzerinde):")
    print("-" * 60)
    report = classification_report(labels, predictions, zero_division=0)
    # Satırları girintile
    for line in report.split("\n"):
        print(f"  {line}")

    # ---- Confusion Matrix ----
    classes = sorted(set(labels))
    cm = confusion_matrix(labels, predictions, labels=classes)

    print(f"\n  [MATRIX] Confusion Matrix:")
    print("-" * 60)

    # Header
    header = "  " + " " * 16
    for cls in classes:
        header += f"{cls[:8]:>10}"
    print(header)

    # Satırlar
    for i, cls in enumerate(classes):
        row = f"  {cls[:14]:<16}"
        for j in range(len(classes)):
            row += f"{cm[i][j]:>10}"
        print(row)

    return scores.mean()


def main():
    """Tum modelleri degerlendir."""

    # Windows console uyumlulugu icin UTF-8 ayarla
    if sys.platform == "win32":
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

    print("\n" + "=" * 60)
    print("  LifeQuest AI -- Model Degerlendirme Raporu")
    print("=" * 60)

    results = {}

    # ---- Intent Classifier ----
    try:
        texts, labels = load_intent_data()
        intent_pipeline = Pipeline([
            ("tfidf", TfidfVectorizer(
                preprocessor=turkish_preprocess,
                analyzer="char_wb",
                ngram_range=(2, 6),
                max_features=15000,
                sublinear_tf=True,
            )),
            ("clf", LogisticRegression(C=2.0, class_weight="balanced", max_iter=1000)),
        ])
        score = evaluate_model("Intent Classifier", texts, labels, intent_pipeline)
        results["intent"] = score
    except Exception as e:
        print(f"\n  [FAIL] Intent modeli degerlendirilemedi: {e}")

    # ---- Sentiment Analyzer ----
    try:
        texts, labels = load_sentiment_data()
        sentiment_pipeline = Pipeline([
            ("tfidf", TfidfVectorizer(
                preprocessor=turkish_preprocess,
                analyzer="char_wb",
                ngram_range=(2, 6),
                max_features=10000,
                sublinear_tf=True,
            )),
            ("clf", LogisticRegression(C=2.0, class_weight="balanced", max_iter=1000)),
        ])
        score = evaluate_model("Sentiment Analyzer", texts, labels, sentiment_pipeline)
        results["sentiment"] = score
    except Exception as e:
        print(f"\n  [FAIL] Sentiment modeli degerlendirilemedi: {e}")

    # ---- Ozet ----
    print(f"\n{'=' * 60}")
    print("  GENEL OZET")
    print(f"{'=' * 60}")
    for model_name, accuracy in results.items():
        status = "[OK]" if accuracy >= 0.8 else "[WARN]" if accuracy >= 0.6 else "[FAIL]"
        print(f"  {status} {model_name.capitalize()}: {accuracy:.2%}")

    print(f"\n{'=' * 60}")
    print("  Degerlendirme tamamlandi!")
    print(f"{'=' * 60}\n")


if __name__ == "__main__":
    main()
