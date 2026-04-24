"""
UrgencyDetector — Kullanıcı mesajının aciliyet/kriz seviyesini tespit eder.
SP-02: Yüksek negatif sentiment + kriz kelimeleri = empati modu tetikle.
"""

URGENCY_KEYWORDS = {
    3: [  # HIGH — Sadece empati, görev önerme
        "bırakmak istiyorum", "dayanamıyorum", "her şey anlamsız",
        "hiçbir şey yapamıyorum", "çöktüm", "bunalım yaşıyorum",
        "artık dayanamıyorum", "pes ettim", "bıraktım her şeyi",
        "anlamsız geliyor hayat", "yoruldum artık her şeyden"
    ],
    2: [  # MODERATE — Empati + hafif mod öner
        "çok bunaldım", "stresim dorukta", "her şey üstüme yıkılıyor",
        "baş edemiyorum", "aşırı stresli", "panik halindeyim",
        "kafam patlayacak", "delireceğim az kaldı", "her şeyden sıkıldım",
        "çok kötü hissediyorum", "berbat bir gün", "mahvoldum",
        "kafam çok karışık", "ne yapacağımı bilmiyorum"
    ],
    1: [  # MILD — Hafif olumsuz, teşvik edici ton
        "biraz yorgunum", "sıkıldım biraz", "halsizim",
        "canım istemedi bugün", "biraz mutsuzum", "motivasyonum düştü",
        "bugün çok hevesliyim değil", "biraz bunaldım",
        "enerjim az bugün", "biraz kötüyüm"
    ]
}


class UrgencyDetector:
    """
    Kullanıcı mesajının aciliyet/kriz seviyesini tespit eder.
    0 = Normal, 1 = Mild, 2 = Moderate, 3 = High
    """

    def detect(self, message: str, sentiment: str = "neutral", sentiment_confidence: float = 0.0) -> int:
        """
        Args:
            message: Kullanıcı mesajı
            sentiment: "positive" | "neutral" | "negative"
            sentiment_confidence: 0.0 - 1.0
        Returns:
            urgency_level: 0-3
        """
        msg_lower = message.lower()

        # Kriz kelimelerini kontrol et (yüksekten düşüğe)
        for level in [3, 2, 1]:
            for keyword in URGENCY_KEYWORDS[level]:
                if keyword in msg_lower:
                    return level

        # Sentiment bazlı ek kontrol
        if sentiment == "negative" and sentiment_confidence >= 0.80:
            return 1

        return 0

    def get_urgency_label(self, level: int) -> str:
        labels = {0: "Normal", 1: "Mild", 2: "Moderate", 3: "High"}
        return labels.get(level, "Normal")

    def should_suppress_tasks(self, level: int) -> bool:
        """Level 3'te görev önerme — sadece empati."""
        return level >= 3

    def should_suggest_light_mode(self, level: int) -> bool:
        """Level 2'de hafif mod öner."""
        return level >= 2
