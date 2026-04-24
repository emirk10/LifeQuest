"""
PersonalityResponseGenerator — Yanıtları kullanıcının seçtiği kişiliğe göre uyarlar.
SP-02: friendly | motivator | coach
"""

import random


PERSONALITY_PREFIXES = {
    "friendly": [
        "", "Harika! ", "Süper! ", "😊 ", "Tabii ki! ",
        "Seninle konuşmak çok güzel! ", "Hadi bakalım! "
    ],
    "motivator": [
        "Yapabilirsin! ", "Hadi gel! ", "Bugün rekor kırıyoruz! ",
        "🔥 ", "Seni durduramaz kimse! ", "Güç sende! ",
        "Maksimum performans! "
    ],
    "coach": [
        "Analiz ettiğimde... ", "Verimlilik açısından... ",
        "Veriler şunu gösteriyor: ", "Stratejik olarak... ",
        "Değerlendirmeme göre... ", "Hedef odaklı yaklaşırsak... "
    ]
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
        "Zor günler geçiyor, ama seni destekliyorum. Bir nefes al. 💙"
    ],
    "motivator": [
        "Zor zamanlar güçlü insanlar yaratır! Bu geçici, sen kalıcısın! 💪",
        "Her fırtına diner. Sen bu fırtınayı geçecek güçtesin! 🌟",
        "Düşmek önemli değil, ayağa kalkmak önemli. Hazır ol! 🔥"
    ],
    "coach": [
        "Bu durumu analiz edelim: Şu an yüksek stres altındasın. Önce bir nefes al, sonra adım adım ilerleyelim. 📊",
        "Kriz anlarında yapılacaklar listesi net olmalı. Şu an tek görevin kendine iyi bakmak. 🎯",
        "Verimliliğin düşük olduğunda zorlamak ters tepiş yapar. Bugün hafif bir şey yapalım. 📈"
    ]
}


class PersonalityResponseGenerator:
    """
    Base yanıtı kullanıcının kişiliğine göre uyarlar.
    """

    def generate(self, base_response: str, personality: str,
                 user_name: str = "", is_empathy: bool = False) -> str:
        """
        Args:
            base_response: Ham yanıt metni
            personality: "friendly" | "motivator" | "coach"
            user_name: Kullanıcı adı (kişiselleştirme için)
            is_empathy: True ise empati yanıtı döndür
        """
        personality = personality if personality in PERSONALITY_PREFIXES else "friendly"

        if is_empathy:
            response = random.choice(EMPATHY_RESPONSES[personality])
            if user_name:
                response = f"{user_name}, {response}"
            return response

        # Prefix ekle (bazen boş)
        prefix = random.choice(PERSONALITY_PREFIXES[personality])
        response = f"{prefix}{base_response}"

        # İsim ile kişiselleştir (bazı kişiliklerde)
        if user_name and personality == "friendly" and random.random() > 0.6:
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
