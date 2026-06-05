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
        "Bazen sadece durup dinlenmek gerekir. Bugün kendine yüklenme, yarın yepyeni bir gün. 😊💙",
        "Hissettiklerini tamamen anlıyorum. Senin gibi duyarlı biri için bu duygular çok normal. Her zaman yanındayım. 🌟",
        "Kendini yalnız hissetme. Her zorluğun arkasında bir kolaylık ve büyüme vardır. Küçük bir mola verelim mi? 💫",
    ],
    "motivator": [
        "Zor zamanlar güçlü insanlar yaratır! Bu geçici, sen kalıcısın! 💪",
        "Her fırtına diner. Sen bu fırtınayı geçecek güçtesin! 🌟",
        "Düşmek önemli değil, ayağa kalkmak önemli. Hazır ol! 🔥",
        "Bu geçici bulutlar senin içindeki güneşi gölgeleyemez! Gücünü hatırla ve devam et! 🚀",
        "En dik yokuşlar en güzel manzaraya çıkar! Vazgeçme, azminle bu engeli de aşacaksın! ⚡",
        "Şu anki yorgunluğun gelecekteki başarın için bir hazırlık. İnancını kaybetme, odaklan ve harekete geç! 💥",
    ],
    "coach": [
        "Bu durumu analiz edelim: Şu an yüksek stres altındasın. Önce bir nefes al, sonra adım adım ilerleyelim. 📊",
        "Kriz anlarında yapılacaklar listesi net olmalı. Şu an tek görevin kendine iyi bakmak. 🎯",
        "Verimliliğin düşük olduğunda zorlamak ters tepki yapar. Bugün hafif bir şey yapalım. 📈",
        "Duygusal iniş çıkışlar performans eğrisinin doğal bir parçasıdır. Bunu bir kayıp değil, dinlenme sinyali olarak görelim. 💡",
        "Bugün hedefleri biraz küçültmek ve toparlanmaya odaklanmak stratejik olarak en doğru karardır. Planımızı güncelleyelim. 📋",
        "Kapasitenin sınırlarını zorlamak yerine sürdürülebilir bir tempo belirleyelim. Küçük ama tutarlı adımlar bizi hedefe ulaştırır. 📈",
    ]
}


class PersonalityResponseGenerator:
    """
    Base yanıtı kullanıcının kişiliğine göre uyarlar.
    Sentiment-aware: negatif duygu durumunda coşkulu prefix kullanmaz.
    """

    def generate(self, base_response: str, personality: str,
                 user_name: str = "", is_empathy: bool = False,
                 sentiment: str = "neutral", intent: str = "",
                 urgency_level: int = 0, context = None) -> str:
        """
        Args:
            base_response: Ham yanıt metni
            personality: "friendly" | "motivator" | "coach"
            user_name: Kullanıcı adı (kişiselleştirme için)
            is_empathy: True ise empati yanıtı döndür
            sentiment: "positive" | "neutral" | "negative"
            intent: Tespit edilen intent (selamlama, alakasiz vb.)
            urgency_level: Aciliyet/kriz seviyesi (0-3)
            context: Kullanıcı context payload'ı
        """
        personality = personality if personality in PERSONALITY_PREFIXES else "friendly"

        if is_empathy:
            response = random.choice(EMPATHY_RESPONSES[personality])
            if user_name:
                response = f"{user_name}, {response}"
            if urgency_level >= 3:
                response += "\n\n⚠️ Eğer çok zor durumdaysan veya kendine zarar vermeyi düşünüyorsan, lütfen yalnız kalma. Aile ve Sosyal Hizmetler Bakanlığı ALO 183 hattını ücretsiz arayabilir veya bir uzmandan destek alabilirsin. Sana değer veriyoruz! 💙"
        else:
            # Selamlama, alakasız ve yardım intent'lerinde prefix ekleme — doğal kalsın
            skip_prefix_intents = {"selamlama", "alakasiz", "yardim", "mood_negatif"}
            if intent in skip_prefix_intents:
                if intent == "selamlama":
                    response = self.add_name_greeting(base_response, user_name, personality)
                else:
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
                if sentiment != "negative" and intent != "selamlama":
                    response = f"{user_name}, {response}"

        # ─── Davranışsal analiz ile yanıtı dinamik olarak özelleştirme ───
        if context and (intent in ("motivasyon", "mood_negatif", "selamlama") or is_empathy):
            custom_insights = []
            
            # 1. Streak kontrolü
            streak = getattr(context, "current_streak", 0)
            if streak > 2:
                if personality == "friendly":
                    custom_insights.append(f"Bu arada, {streak} günlük harika serini bozmayalım, bugün de devam edelim! 🌟")
                elif personality == "motivator":
                    custom_insights.append(f"Gözüm üzerinde! Tam {streak} günlük serin var. Bu alevi söndürme, bugün de yakalım! 🔥")
                elif personality == "coach":
                    custom_insights.append(f"Mevcut disiplin analizi: {streak} günlük ardışık aktiflik serisi tespit edildi. Sürekliliği korumak önemlidir. 📈")
            
            # 2. Level kontrolü
            level = getattr(context, "general_level", 1.0)
            if level > 1.0:
                if personality == "friendly":
                    custom_insights.append(f"Seviyen şu an {int(level)}. Harika ilerliyorsun, gelişmeye devam! 💫")
                elif personality == "motivator":
                    custom_insights.append(f"Level {level} gücüyle bugün önündeki tüm hedefleri devireceksin! 🚀")
                elif personality == "coach":
                    custom_insights.append(f"Profil seviyesi: {level}. Gelişim eğrimiz yukarı yönlü devam ediyor. 📊")

            # 3. En çok atlanan kategori kontrolü
            most_skipped = getattr(context, "most_skipped_category", None)
            if most_skipped:
                if personality == "friendly":
                    custom_insights.append(f"Minik bir tavsiye: Son zamanlarda {most_skipped} alanını biraz aksattık sanki. Bugün oradan çok küçük, 5-10 dakikalık bir görevle başlamaya ne dersin? 😊")
                elif personality == "motivator":
                    custom_insights.append(f"Analizlere göre {most_skipped} alanında biraz yavaşlamışız. Bugün o korktuğumuz kategoriye hücum ediyoruz! Bahane yok! 💪")
                elif personality == "coach":
                    custom_insights.append(f"Stratejik zayıflık tespiti: {most_skipped} kategorisinde tamamlanma açığı mevcut. Bugün dengeli gelişim adına bu kategoriye odaklanmak faydalı olacaktır. 💡")

            # 4. Güçlü yönler kontrolü
            strengths = getattr(context, "strengths", [])
            if strengths and len(strengths) > 0:
                strength = random.choice(strengths)
                if personality == "friendly":
                    custom_insights.append(f"Senin {strength} konusundaki yeteneğin ve gücün sayesinde bugün harika şeyler yapacağına inanıyorum! ✨")
                elif personality == "motivator":
                    custom_insights.append(f"Unutma, senin en büyük silahlarından biri {strength}! Bu gücü serbest bırak ve hedefini tamamla! ⚡")
                elif personality == "coach":
                    custom_insights.append(f"Stratejik avantaj: {strength} alanındaki yetkinliğini bugünkü görevlerde bir kaldıraç olarak kullanabilirsin. 📋")

            # 5. Zayıf yönler kontrolü
            weaknesses = getattr(context, "weaknesses", [])
            if weaknesses and len(weaknesses) > 0:
                weakness = random.choice(weaknesses)
                if personality == "friendly":
                    custom_insights.append(f"Biliyorum, bazen {weakness} konusunda zorluk çekebiliyorsun, ama adımları küçük tuttukça bunu aşacağız. 💙")
                elif personality == "motivator":
                    custom_insights.append(f"Kendini {weakness} konusunda zayıf hissetme! Her tamamladığın görev bu zırhı daha da kalınlaştırıyor! 💥")
                elif personality == "coach":
                    custom_insights.append(f"Gelişim alanı analizi: {weakness} konusundaki direnci kırmak için görevleri küçük alt adımlara bölerek ilerlemeyi tavsiye ederim. 🔍")

            if custom_insights:
                # Rastgele bir insight seçip response'a ekleyelim
                insight = random.choice(custom_insights)
                response = f"{response}\n\n{insight}"

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
