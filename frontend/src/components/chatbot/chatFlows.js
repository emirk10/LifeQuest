
// ─── LifeQuest Chatbot Flow Engine ───────────────────────────────────────────
// Her "node" bir ekran durumunu temsil eder.
// type: "message" | "options" | "api_call" | "dynamic"
// options içindeki her buton { label, icon, nextNode, action? } içerir.

export const FLOWS = {

  // ══════════════════════════════════════════════════════
  //  ANA MENÜ
  // ══════════════════════════════════════════════════════
  main_menu: {
    id: "main_menu",
    type: "options",
    message: "Merhaba! 👋 Ben LifeQuest asistanın. Sana nasıl yardımcı olabilirim?",
    options: [
      { label: "📋 Görevlerim", icon: "📋", nextNode: "tasks_menu" },
      { label: "🎯 Hedeflerim", icon: "🎯", nextNode: "goals_menu" },
      { label: "📊 İstatistiklerim", icon: "📊", nextNode: "stats_menu" },
      { label: "💪 Motivasyon İstiyorum", icon: "💪", nextNode: "motivation_check_mood" },
      { label: "✨ Görev Öner", icon: "✨", nextNode: "suggest_task_category" },
      { label: "🏆 Rozetlerim", icon: "🏆", nextNode: "badges_show" },
      { label: "😊 Ruh Halimi Güncelle", icon: "😊", nextNode: "mood_update_intro" },
    ],
  },

  // ══════════════════════════════════════════════════════
  //  GÖREV MENÜSÜ
  // ══════════════════════════════════════════════════════
  tasks_menu: {
    id: "tasks_menu",
    type: "options",
    message: "Görevlerinle ilgili ne yapmak istersin?",
    options: [
      { label: "⏳ Aktif Görevlerim", icon: "⏳", nextNode: "tasks_active" },
      { label: "✅ Tamamladıklarım (Bugün)", icon: "✅", nextNode: "tasks_completed_today" },
      { label: "📅 Ertelenen Görevler", icon: "📅", nextNode: "tasks_postponed" },
      { label: "🎲 Görev Havuzu", icon: "🎲", nextNode: "tasks_daily_pool" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  tasks_active: {
    id: "tasks_active",
    type: "api_call",
    apiAction: "get_active_tasks",
    loadingMessage: "Aktif görevlerin yükleniyor...",
    nextNode: "tasks_active_result",
  },

  tasks_active_result: {
    id: "tasks_active_result",
    type: "task_list",
    emptyMessage: "🎉 Şu an aktif bir görevin yok! Yeni görev öneri almak ister misin?",
    emptyOptions: [
      { label: "✨ Görev Öner", icon: "✨", nextNode: "suggest_task_category" },
      { label: "⬅️ Görev Menüsü", icon: "⬅️", nextNode: "tasks_menu" },
    ],
    options: [
      { label: "⬅️ Görev Menüsü", icon: "⬅️", nextNode: "tasks_menu" },
    ],
  },

  tasks_completed_today: {
    id: "tasks_completed_today",
    type: "api_call",
    apiAction: "get_completed_today",
    loadingMessage: "Bugün tamamlananlar getiriliyor...",
    nextNode: "tasks_completed_today_result",
  },

  tasks_completed_today_result: {
    id: "tasks_completed_today_result",
    type: "completed_task_list",
    emptyMessage: "Bugün henüz hiç görev tamamlamadın. Hadi başlayalım! 💪",
    emptyOptions: [
      { label: "✨ Görev Öner", icon: "✨", nextNode: "suggest_task_category" },
      { label: "⬅️ Görev Menüsü", icon: "⬅️", nextNode: "tasks_menu" },
    ],
    options: [
      { label: "💪 Motivasyon İstiyorum", icon: "💪", nextNode: "motivation_check_mood" },
      { label: "⬅️ Görev Menüsü", icon: "⬅️", nextNode: "tasks_menu" },
    ],
  },

  tasks_postponed: {
    id: "tasks_postponed",
    type: "api_call",
    apiAction: "get_postponed_tasks",
    loadingMessage: "Ertelenen görevler yükleniyor...",
    nextNode: "tasks_postponed_result",
  },

  tasks_postponed_result: {
    id: "tasks_postponed_result",
    type: "postponed_task_list",
    emptyMessage: "✅ Harika! Ertelenmiş görevin yok.",
    emptyOptions: [
      { label: "⬅️ Görev Menüsü", icon: "⬅️", nextNode: "tasks_menu" },
    ],
    options: [
      { label: "⬅️ Görev Menüsü", icon: "⬅️", nextNode: "tasks_menu" },
    ],
  },

  tasks_daily_pool: {
    id: "tasks_daily_pool",
    type: "api_call",
    apiAction: "get_daily_pool",
    loadingMessage: "Bugünkü görev havuzu hazırlanıyor...",
    nextNode: "tasks_daily_pool_result",
  },

  tasks_daily_pool_result: {
    id: "tasks_daily_pool_result",
    type: "pool_task_list",
    emptyMessage: "Şu an havuzda görev bulunmuyor. Yeni görev önerisi ister misin?",
    emptyOptions: [
      { label: "✨ Görev Öner", icon: "✨", nextNode: "suggest_task_category" },
      { label: "⬅️ Görev Menüsü", icon: "⬅️", nextNode: "tasks_menu" },
    ],
    options: [
      { label: "⬅️ Görev Menüsü", icon: "⬅️", nextNode: "tasks_menu" },
    ],
  },

  // ══════════════════════════════════════════════════════
  //  HEDEF MENÜSÜ
  // ══════════════════════════════════════════════════════
  goals_menu: {
    id: "goals_menu",
    type: "options",
    message: "Hedeflerinle ilgili ne yapmak istersin?",
    options: [
      { label: "📈 Aktif Hedeflerim", icon: "📈", nextNode: "goals_active" },
      { label: "🎯 Hedef Dashboard", icon: "🎯", nextNode: "goals_dashboard" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  goals_active: {
    id: "goals_active",
    type: "api_call",
    apiAction: "get_goals",
    loadingMessage: "Hedeflerin yükleniyor...",
    nextNode: "goals_active_result",
  },

  goals_active_result: {
    id: "goals_active_result",
    type: "goal_list",
    emptyMessage: "Henüz bir hedef belirlemedin. Hedefler sayfasından ekleyebilirsin!",
    emptyOptions: [
      { label: "🎯 Hedefler Sayfasına Git", icon: "🎯", action: "navigate", target: "/goals" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
    options: [
      { label: "⬅️ Hedef Menüsü", icon: "⬅️", nextNode: "goals_menu" },
    ],
  },

  goals_dashboard: {
    id: "goals_dashboard",
    type: "api_call",
    apiAction: "get_goals_dashboard",
    loadingMessage: "Hedef özeti hazırlanıyor...",
    nextNode: "goals_dashboard_result",
  },

  goals_dashboard_result: {
    id: "goals_dashboard_result",
    type: "goals_summary",
    options: [
      { label: "📈 Detaylar İçin Git", icon: "📈", action: "navigate", target: "/goals" },
      { label: "⬅️ Hedef Menüsü", icon: "⬅️", nextNode: "goals_menu" },
    ],
  },

  // ══════════════════════════════════════════════════════
  //  İSTATİSTİK MENÜSÜ
  // ══════════════════════════════════════════════════════
  stats_menu: {
    id: "stats_menu",
    type: "options",
    message: "Hangi istatistiklerine bakmak istersin?",
    options: [
      { label: "👤 Profil & Seviyem", icon: "👤", nextNode: "stats_profile" },
      { label: "📅 Bu Haftaki Performansım", icon: "📅", nextNode: "stats_weekly" },
      { label: "🔥 Günlük XP Kazanımı", icon: "🔥", nextNode: "stats_daily_xp" },
      { label: "📊 Genel Özet", icon: "📊", nextNode: "stats_overview" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  stats_profile: {
    id: "stats_profile",
    type: "api_call",
    apiAction: "get_profile",
    loadingMessage: "Profil bilgilerin getiriliyor...",
    nextNode: "stats_profile_result",
  },

  stats_profile_result: {
    id: "stats_profile_result",
    type: "profile_card",
    options: [
      { label: "📊 Genel Özet", icon: "📊", nextNode: "stats_overview" },
      { label: "⬅️ İstatistik Menüsü", icon: "⬅️", nextNode: "stats_menu" },
    ],
  },

  stats_weekly: {
    id: "stats_weekly",
    type: "api_call",
    apiAction: "get_weekly_report",
    loadingMessage: "Haftalık rapor hazırlanıyor...",
    nextNode: "stats_weekly_result",
  },

  stats_weekly_result: {
    id: "stats_weekly_result",
    type: "weekly_stats",
    options: [
      { label: "⬅️ İstatistik Menüsü", icon: "⬅️", nextNode: "stats_menu" },
    ],
  },

  stats_daily_xp: {
    id: "stats_daily_xp",
    type: "api_call",
    apiAction: "get_daily_breakdown",
    loadingMessage: "XP verileri yükleniyor...",
    nextNode: "stats_daily_xp_result",
  },

  stats_daily_xp_result: {
    id: "stats_daily_xp_result",
    type: "xp_chart",
    options: [
      { label: "⬅️ İstatistik Menüsü", icon: "⬅️", nextNode: "stats_menu" },
    ],
  },

  stats_overview: {
    id: "stats_overview",
    type: "api_call",
    apiAction: "get_user_stats",
    loadingMessage: "Genel istatistikler hesaplanıyor...",
    nextNode: "stats_overview_result",
  },

  stats_overview_result: {
    id: "stats_overview_result",
    type: "stats_card",
    options: [
      { label: "⬅️ İstatistik Menüsü", icon: "⬅️", nextNode: "stats_menu" },
    ],
  },

  // ══════════════════════════════════════════════════════
  //  MOTİVASYON AKIŞI (AI Destekli)
  // ══════════════════════════════════════════════════════
  motivation_check_mood: {
    id: "motivation_check_mood",
    type: "api_call",
    apiAction: "get_today_mood",
    loadingMessage: "Ruh halin kontrol ediliyor...",
    nextNode: "motivation_mood_aware",
  },

  motivation_mood_aware: {
    id: "motivation_mood_aware",
    type: "dynamic",
    // Bu node, mood datasına göre farklı mesaj üretir
    // Mood yoksa → mood_missing, varsa → AI motivasyon çağrısı
    dynamicType: "motivation_with_mood",
    options: [
      { label: "🔄 Farklı Mesaj", icon: "🔄", nextNode: "motivation_mood_aware", action: "refresh_motivation" },
      { label: "😊 Ruh Halimi Güncelle", icon: "😊", nextNode: "mood_update_intro" },
      { label: "✨ Görev Öner", icon: "✨", nextNode: "suggest_task_category" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  motivation_mood_missing: {
    id: "motivation_mood_missing",
    type: "options",
    message: "Bugün nasıl hissediyorsun? Ruh haline göre sana özel bir mesaj vereyim! 😊",
    options: [
      { label: "😊 Harika Hissediyorum", icon: "😊", nextNode: "motivation_ai_call", moodHint: "pozitif" },
      { label: "😐 Normal", icon: "😐", nextNode: "motivation_ai_call", moodHint: "normal" },
      { label: "😔 Biraz Yorgunum", icon: "😔", nextNode: "motivation_ai_call", moodHint: "yorgun" },
      { label: "😟 Zor Bir Gün", icon: "😟", nextNode: "motivation_ai_call", moodHint: "zor" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  motivation_ai_call: {
    id: "motivation_ai_call",
    type: "api_call",
    apiAction: "get_ai_motivation",
    loadingMessage: "Sana özel motivasyon mesajı hazırlanıyor... ✨",
    nextNode: "motivation_result",
  },

  motivation_result: {
    id: "motivation_result",
    type: "motivation_card",
    options: [
      { label: "🔄 Farklı Mesaj", icon: "🔄", nextNode: "motivation_ai_call", action: "refresh_motivation" },
      { label: "✨ Görev Öner", icon: "✨", nextNode: "suggest_task_category" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  // ══════════════════════════════════════════════════════
  //  GÖREV ÖNERİ AKIŞI (Kişiselleştirilmiş)
  // ══════════════════════════════════════════════════════
  suggest_task_category: {
    id: "suggest_task_category",
    type: "options",
    message: "Harika! Hangi alanda bir görev yapmak istersin? 🎯\nRuh halin, seviyene ve değerlerine göre sana özel öneriler getireceğim.",
    options: [
      { label: "💪 Sağlık & Spor", icon: "💪", nextNode: "suggest_task_difficulty", categoryHint: "saglik" },
      { label: "📚 Eğitim & Gelişim", icon: "📚", nextNode: "suggest_task_difficulty", categoryHint: "egitim" },
      { label: "🧘 Zihinsel Sağlık", icon: "🧘", nextNode: "suggest_task_difficulty", categoryHint: "mental" },
      { label: "🤝 Sosyal & İlişkiler", icon: "🤝", nextNode: "suggest_task_difficulty", categoryHint: "sosyal" },
      { label: "💼 Kariyer & Üretkenlik", icon: "💼", nextNode: "suggest_task_difficulty", categoryHint: "kariyer" },
      { label: "🎨 Yaratıcılık & Hobi", icon: "🎨", nextNode: "suggest_task_difficulty", categoryHint: "hobi" },
      { label: "🌟 Sürpriz Et! (Her Şey)", icon: "🌟", nextNode: "suggest_task_fetch", categoryHint: "all" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  suggest_task_difficulty: {
    id: "suggest_task_difficulty",
    type: "options",
    message: "Ne kadar zorlu bir görev istersin?",
    options: [
      { label: "🌱 Kolay — Hızlı tamamlanır", icon: "🌱", nextNode: "suggest_task_fetch", difficultyHint: "Easy" },
      { label: "⚡ Orta — Dengeli meydan okuma", icon: "⚡", nextNode: "suggest_task_fetch", difficultyHint: "Medium" },
      { label: "🔥 Zor — Gerçek sınırlarımı zorla", icon: "🔥", nextNode: "suggest_task_fetch", difficultyHint: "Hard" },
      { label: "🤖 Karar Ver (Ruh haline göre)", icon: "🤖", nextNode: "suggest_task_fetch", difficultyHint: "auto" },
      { label: "⬅️ Kategori Seç", icon: "⬅️", nextNode: "suggest_task_category" },
    ],
  },

  suggest_task_fetch: {
    id: "suggest_task_fetch",
    type: "api_call",
    apiAction: "get_personalized_tasks",
    loadingMessage: "Sana özel görevler hazırlanıyor... 🎯",
    nextNode: "suggest_task_result",
  },

  suggest_task_result: {
    id: "suggest_task_result",
    type: "suggested_task_cards",
    emptyMessage: "Bu kriterlere uygun görev bulunamadı. Başka bir kategori dene!",
    emptyOptions: [
      { label: "🔄 Başka Kategori", icon: "🔄", nextNode: "suggest_task_category" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
    options: [
      { label: "🔄 Farklı Öneriler", icon: "🔄", nextNode: "suggest_task_fetch", action: "refresh_suggestions" },
      { label: "🗂️ Başka Kategori", icon: "🗂️", nextNode: "suggest_task_category" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  // ══════════════════════════════════════════════════════
  //  ROZET AKIŞI
  // ══════════════════════════════════════════════════════
  badges_show: {
    id: "badges_show",
    type: "api_call",
    apiAction: "get_badges",
    loadingMessage: "Rozetlerin yükleniyor... 🏆",
    nextNode: "badges_result",
  },

  badges_result: {
    id: "badges_result",
    type: "badge_gallery",
    options: [
      { label: "📊 İstatistiklerim", icon: "📊", nextNode: "stats_menu" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  // ══════════════════════════════════════════════════════
  //  RUH HALİ GÜNCELLEME
  // ══════════════════════════════════════════════════════
  mood_update_intro: {
    id: "mood_update_intro",
    type: "options",
    message: "Bugün nasıl hissediyorsun? 🌈",
    options: [
      { label: "🚀 Harika, Enerji doluyum!", icon: "🚀", nextNode: "mood_update_energy", moodMode: 0 }, // Normal
      { label: "😊 İyi, motivasyonum var", icon: "😊", nextNode: "mood_update_energy", moodMode: 0 },
      { label: "😐 Fena değil, idare eder", icon: "😐", nextNode: "mood_update_energy", moodMode: 1 }, // LightMode
      { label: "😔 Yorgunum, düşük enerjiliyim", icon: "😔", nextNode: "mood_update_energy", moodMode: 1 },
      { label: "🌧️ Çok zorlu bir gün", icon: "🌧️", nextNode: "mood_update_energy", moodMode: 2 }, // VacationMode
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },

  mood_update_energy: {
    id: "mood_update_energy",
    type: "options",
    message: "Bugün ne kadar zamanın ve enerjin var?",
    options: [
      { label: "⚡ Çok Enerjik (8-10/10)", icon: "⚡", nextNode: "mood_update_time", energyLevel: 9 },
      { label: "💪 Enerjik (6-7/10)", icon: "💪", nextNode: "mood_update_time", energyLevel: 7 },
      { label: "😐 Orta (4-5/10)", icon: "😐", nextNode: "mood_update_time", energyLevel: 5 },
      { label: "😴 Düşük (1-3/10)", icon: "😴", nextNode: "mood_update_time", energyLevel: 2 },
    ],
  },

  mood_update_time: {
    id: "mood_update_time",
    type: "options",
    message: "Bugün görevler için kaç dakikan var?",
    options: [
      { label: "⏱️ 15-30 dk", icon: "⏱️", nextNode: "mood_update_save", availableMinutes: 20 },
      { label: "⏰ 30-60 dk", icon: "⏰", nextNode: "mood_update_save", availableMinutes: 45 },
      { label: "🕐 1-2 saat", icon: "🕐", nextNode: "mood_update_save", availableMinutes: 90 },
      { label: "🕑 2+ saat", icon: "🕑", nextNode: "mood_update_save", availableMinutes: 150 },
    ],
  },

  mood_update_save: {
    id: "mood_update_save",
    type: "api_call",
    apiAction: "save_mood",
    loadingMessage: "Ruh halin kaydediliyor...",
    nextNode: "mood_update_result",
  },

  mood_update_result: {
    id: "mood_update_result",
    type: "mood_saved",
    options: [
      { label: "✨ Görev Öner (Ruh Halime Göre)", icon: "✨", nextNode: "suggest_task_category" },
      { label: "💪 Motivasyon İstiyorum", icon: "💪", nextNode: "motivation_mood_aware" },
      { label: "⬅️ Ana Menü", icon: "⬅️", nextNode: "main_menu" },
    ],
  },
};

// ── Motivasyon mesajları (AI olmadan offline fallback) ────────────────────────
export const MOTIVATION_TEMPLATES = {
  pozitif: [
    "🔥 Muhteşem bir enerji içindesin! Bu gücü bugün tam anlamıyla kullan. Her görev tamamlandığında bir adım daha ileri gidiyorsun.",
    "⚡ Bugün zirvedesin! Bu momentum'u koru — büyük hedefler küçük adımlarla başlar ve sen zaten koşuyorsun!",
    "🌟 Enerjin harika! Bugün en zorlu görevini listele ve üzerine git. Sen yapabilirsin!",
  ],
  normal: [
    "😊 Her gün mükemmel olmak zorunda değil. Bugün küçük ama anlamlı bir adım at — bu yeter!",
    "🌱 Süreklilik, mükemmeliyetten daha güçlüdür. Bugün sadece bir şey yap — geri kalanı kendiliğinden gelir.",
    "💫 Normal bir gün de harika bir fırsattır. Küçük bir ilerleme, büyük bir fark yaratır.",
  ],
  yorgun: [
    "🤗 Yorgunluk, sert çalıştığının kanıtıdır. Bugün kendinle nazik ol — küçük bir görev bile ilerleme sayılır.",
    "🌿 Bazen en cesur eylem, dinlenmek ve devam etmektir. Bugün hafif bir görevle başla.",
    "💙 Enerjin düşük olabilir ama niyetin güçlü! Tek bir küçük adım seni ileriye taşır.",
  ],
  zor: [
    "🫂 Bugün zor bir gün olabilir, ama sen burada olmaya devam ediyorsun — bu başlı başına bir başarı.",
    "🌧️ Fırtınalar geçer. Bugün sadece varlığını koru, yarın daha güçlü olacaksın.",
    "💜 Kendine karşı nazik ol. Bugün çok küçük bir hedef belirle ve onu tamamla — bu yeterli.",
  ],
  seviye_dusuk: [
    "🌱 Her uzman bir zamanlar yeni başlıyandı. Temeli güçlü at, zirve kendiliğinden gelecek!",
    "⭐ Başlangıç noktası en önemli adımdır. Her görev seni bir sonraki seviyeye taşıyor!",
  ],
  seviye_yuksek: [
    "🏆 Bu seviyeye gelmen rastlantı değil — sürekli çalışmanın ürünü! Hedeflerin büyüsün!",
    "🚀 Deneyimli bir yolcusun. Şimdi başkalarına ilham verme zamanı!",
  ],
};
