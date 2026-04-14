"""
LifeQuest AI - Reflection Verifier (Yansıma Doğrulayıcı)

Görev tamamlama metinlerinin kalitesini ve ilgililiğini analiz eder:
  - Kelime sayısı kontrolü
  - Spam / tekrar tespiti
  - Görev kategorisi ile ilgililik
  - Metin kalite skoru

Kural tabanlı + basit NLP hibrit yaklaşım.
"""

import re
from collections import Counter


# Kategori → anahtar kelime eşleştirmesi
CATEGORY_KEYWORDS: dict[str, list[str]] = {
    "spor": [
        "koş", "koşu", "yürü", "yürüyüş", "egzersiz", "spor", "fitness",
        "antrenman", "bisiklet", "yüz", "yüzme", "squat", "plank",
        "kardiyo", "kas", "ağırlık", "salon", "ter", "adım", "set",
        "tekrar", "dakika", "kalori", "nabız", "tempo", "mesafe",
    ],
    "eğitim": [
        "oku", "okuma", "kitap", "ders", "çalış", "öğren", "kurs",
        "eğitim", "sayfa", "not", "makale", "araştır", "video",
        "podcast", "bölüm", "sunum", "yazı", "blog", "ödev",
        "sınav", "quiz", "sertifika", "dil", "matematik", "kod",
    ],
    "sağlık": [
        "su", "uyku", "beslen", "yemek", "diyet", "vitamin", "ilaç",
        "meditasyon", "nefes", "yoga", "sağlık", "kalori", "protein",
        "kahvaltı", "öğün", "meyve", "sebze", "uyudum", "kalktım",
        "saat", "litre", "bardak", "gıda", "doktor", "check-up",
    ],
    "kariyer": [
        "iş", "proje", "toplantı", "sunum", "plan", "hedef", "rapor",
        "mail", "müşteri", "görev", "deadline", "teslim", "portföy",
        "network", "linkedin", "cv", "başvuru", "mülakat", "kariyer",
        "terfi", "maaş", "şirket", "ekip", "liderlik", "strateji",
    ],
    "sosyal": [
        "arkadaş", "aile", "tanış", "sohbet", "buluş", "etkinlik",
        "gönüllü", "yardım", "paylaş", "davet", "parti", "kutla",
        "hediye", "tebrik", "destek", "grup", "topluluk", "kulüp",
        "organizasyon", "kahve", "yemek", "çay", "seyahat",
    ],
}


class ReflectionVerifier:
    """Görev tamamlama yansıma metinlerini doğrulayan servis."""

    def verify(
        self,
        text: str,
        min_word_count: int = 10,
        task_category: str | None = None,
    ) -> dict:
        """Yansıma metnini analiz et.

        Args:
            text:           Kullanıcının yazdığı yansıma metni
            min_word_count: Minimum kabul edilebilir kelime sayısı
            task_category:  Görevin kategorisi (ilgililik kontrolü için)

        Returns:
            {
                "is_valid": bool,
                "word_count": int,
                "quality_score": float,       # 0-1
                "relevance_score": float,     # 0-1
                "spam_detected": bool,
                "issues": list[str],          # Tespit edilen sorunlar
                "message": str,               # Kullanıcıya gösterilecek mesaj
            }
        """
        issues: list[str] = []

        # ---- 1. Kelime sayısı ----
        words = text.split()
        word_count = len(words)

        if word_count < min_word_count:
            issues.append(
                f"Metin çok kısa: {word_count} kelime "
                f"(minimum {min_word_count} kelime gerekli)"
            )

        # ---- 2. Spam / tekrar tespiti ----
        spam_detected = self._detect_spam(words)
        if spam_detected:
            issues.append("Tekrarlayan veya spam içerik tespit edildi")

        # ---- 3. Kalite skoru ----
        quality_score = self._calculate_quality(text, words)
        if quality_score < 0.3:
            issues.append("Metin kalitesi düşük (çok kısa cümleler veya anlamsız içerik)")

        # ---- 4. İlgililik skoru ----
        relevance_score = 0.5  # Varsayılan
        if task_category:
            relevance_score = self._calculate_relevance(text, task_category)
            if relevance_score < 0.2:
                issues.append(
                    f"Metin '{task_category}' kategorisi ile ilgisiz görünüyor"
                )

        # ---- Sonuç ----
        is_valid = (
            word_count >= min_word_count
            and not spam_detected
            and quality_score >= 0.3
        )

        if is_valid:
            message = "✅ Yansıma metni başarıyla doğrulandı."
        else:
            message = "❌ Yansıma metni doğrulanamadı: " + "; ".join(issues)

        return {
            "is_valid": is_valid,
            "word_count": word_count,
            "quality_score": round(quality_score, 4),
            "relevance_score": round(relevance_score, 4),
            "spam_detected": spam_detected,
            "issues": issues,
            "message": message,
        }

    # ------------------------------------------------------------------ #
    #  YARDIMCI METODLAR
    # ------------------------------------------------------------------ #

    def _detect_spam(self, words: list[str]) -> bool:
        """Aynı kelimenin %50'den fazla tekrarı → spam."""
        if len(words) < 3:
            return False

        lower_words = [w.lower() for w in words]
        counter = Counter(lower_words)
        most_common_count = counter.most_common(1)[0][1]

        # Bir kelimenin toplam kelimelerin yarısından fazla tekrarı → spam
        if most_common_count / len(lower_words) > 0.5:
            return True

        # Ardışık aynı kelimelerden oluşan diziler (copy-paste tespiti)
        consecutive = 1
        for i in range(1, len(lower_words)):
            if lower_words[i] == lower_words[i - 1]:
                consecutive += 1
                if consecutive >= 4:
                    return True
            else:
                consecutive = 1

        return False

    def _calculate_quality(self, text: str, words: list[str]) -> float:
        """Metin kalitesini 0-1 arası puanla.

        Kriterler:
          - Cümle varlığı (noktalama)
          - Ortalama kelime uzunluğu
          - Benzersiz kelime oranı
          - Büyük harf kullanımı (cümle başı)
        """
        if not words:
            return 0.0

        score = 0.0

        # Cümle varlığı (nokta, ünlem, soru işareti)
        sentences = re.split(r"[.!?]+", text.strip())
        sentences = [s.strip() for s in sentences if s.strip()]
        if len(sentences) >= 1:
            score += 0.25
        if len(sentences) >= 2:
            score += 0.15

        # Ortalama kelime uzunluğu (3-8 arası ideal)
        avg_word_len = sum(len(w) for w in words) / len(words)
        if 3 <= avg_word_len <= 10:
            score += 0.2

        # Benzersiz kelime oranı (çeşitlilik)
        unique_ratio = len(set(w.lower() for w in words)) / len(words)
        score += unique_ratio * 0.25

        # Minimum uzunluk bonusu
        if len(words) >= 20:
            score += 0.15

        return min(score, 1.0)

    def _calculate_relevance(self, text: str, task_category: str) -> float:
        """Metnin görev kategorisi ile ilgililiğini 0-1 arası puanla."""

        category_key = task_category.lower().strip()

        # Kategoriye ait anahtar kelimeleri bul
        keywords: list[str] = []
        for cat, kws in CATEGORY_KEYWORDS.items():
            if cat in category_key or category_key in cat:
                keywords = kws
                break

        if not keywords:
            # Bilinmeyen kategori → nötr skor
            return 0.5

        text_lower = text.lower()
        matches = sum(1 for kw in keywords if kw in text_lower)

        # Normalize et (en fazla 10 eşleşme → 1.0)
        relevance = min(matches / 5.0, 1.0)
        return relevance
