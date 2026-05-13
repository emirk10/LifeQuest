"""
PersonalityResponseGenerator — Yanıtları kullanıcının seçtiği kişiliğe göre uyarlar.
SP-02: friendly | motivator | coach

GÜNCELLEME: Bağlamsız ön-ek problemi çözüldü. Artık prefix'ler sentiment ve
intent'e göre filtreleniyor, "Harika!" gibi uygunsuz ön-ekler negatif duygu
durumunda veya selamlama/empati bağlamında kullanılmıyor.
"""

import random


# ─── Sentiment'e göre ayrıştırılmış prefix'ler ─────────────────────────────────
# Her prefix setinde "" (boş) ağırlıklı olmalı — her cümlenin önüne ek koymak doğal değil

PERSONALITY_PREFIXES = {
    "friendly": {
        "positive": ["", "", "", "Süper! ", "😊 "],
        "neutral": ["", "", "", ""],
        "negative": ["", "", ""],  # Negatif duygu durumunda asla "Harika!" deme
    },
    "motivator": {
        "positive": ["", "Hadi! ", "🔥 ", "Güç sende! "],
        "neutral": ["", "", "Yapabilirsin! "],
        "negative": ["", ""],  # Empati öncelikli, zorlamadan
    },
    "coach": {
        "positive": ["", "Stratejik olarak... "],
        "neutral": ["", "Değerlendirmeme göre... "],
        "negative": ["", ""],
    }
}

PERSONALITY_EMOJIS = {
    "friendly": ["😊", "🌟", "💫", "✨", "🎉", "👋"],
    "motivator": ["🔥", "💪", "🚀", "⚡", "🏆", "💥"],
    "coach": ["📊", "🎯", "📈", "🔍", "💡", "📋"]
}

EMPATHY_RESPONSES = {
    "friendly": [
        "Seni duyuyorum ve bu gerçekten zor bir süreç. Yanındayım! 💙",
        "Böyle hissetmek çok normal. Kendine iyi bak, adım adım ilerleriz. 🤗",
        "Zor günler geçiyor, ama seni destekliyorum. Bir nefes al. 💙",
    ],
    "motivator": [
        "Zor zamanlar güçlü insanlar yaratır! Bu geçici, sen kalıcısın! 💪",
        "Her fırtına diner. Sen bu fırtınayı geçecek güçtesin! 🌟",
        "Düşmek önemli değil, ayağa kalkmak önemli. Hazır ol! 🔥",
    ],
    "coach": [
        "Bu durumu analiz edelim: Şu an yüksek stres altındasın. Önce bir nefes al, sonra adım adım ilerleyelim. 📊",
        "Kriz anlarında yapılacaklar listesi net olmalı. Şu an tek görevin kendine iyi bakmak. 🎯",
        "Verimliliğin düşük olduğunda zorlamak ters tepki yapar. Bugün hafif bir şey yapalım. 📈",
    ]
}


class PersonalityResponseGenerator:
    """
    Base yanıtı kullanıcının kişiliğine göre uyarlar.
    Sentiment-aware: negatif duygu durumunda coşkulu prefix kullanmaz.
    """

    def generate(self, base_response: str, personality: str,
                 user_name: str = "", is_empathy: bool = False,
                 sentiment: str = "neutral", intent: str = "") -> str:
        """
        Args:
            base_response: Ham yanıt metni
            personality: "friendly" | "motivator" | "coach"
            user_name: Kullanıcı adı (kişiselleştirme için)
            is_empathy: True ise empati yanıtı döndür
            sentiment: "positive" | "neutral" | "negative"
            intent: Tespit edilen intent (selamlama, alakasiz vb.)
        """
        personality = personality if personality in PERSONALITY_PREFIXES else "friendly"

        if is_empathy:
            response = random.choice(EMPATHY_RESPONSES[personality])
            if user_name:
                response = f"{user_name}, {response}"
            return response

        # Selamlama, alakasız ve yardım intent'lerinde prefix ekleme — doğal kalsın
        skip_prefix_intents = {"selamlama", "alakasiz", "yardim", "mood_negatif"}
        if intent in skip_prefix_intents:
            response = base_response
        else:
            # Sentiment'e uygun prefix seç
            prefix_set = PERSONALITY_PREFIXES[personality].get(sentiment,
                          PERSONALITY_PREFIXES[personality].get("neutral", [""]))
            prefix = random.choice(prefix_set)
            response = f"{prefix}{base_response}" if prefix else base_response

        # İsim ile kişiselleştir (düşük olasılıkla, doğal olsun)
        if user_name and personality == "friendly" and random.random() > 0.75:
            # Negatif duygu durumunda isim kullanma — mesafe hissi verir
            if sentiment != "negative":
                response = f"{user_name}, {response}"

        return response

    def add_name_greeting(self, response: str, user_name: str, personality: str) -> str:
        """Selamlama mesajlarına isim ekle."""
        if not user_name:
            return response
        if personality == "friendly":
            return f"Hey {user_name}! {response}"
        elif personality == "motivator":
            return f"Hadi {user_name}! {response}"
        else:
            return f"{user_name}, {response}"
