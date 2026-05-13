// ─── ChatBot API Handler ──────────────────────────────────────────────────────
import axiosClient from "../../api/axiosClient";
import chatService from "../../services/chatService";
import taskService from "../../services/taskService";
import userService from "../../services/userService";
import moodService from "../../services/moodService";
import badgeService from "../../services/badgeService";
import goalService from "../../services/goalService";
import { MOTIVATION_TEMPLATES } from "./chatFlows";

// ─── Yardımcı: API cevabından metin çıkar ────────────────────────────────────
function extractText(val) {
  if (!val) return null;
  if (typeof val === "string") return val;
  // AI servisi bazen { response: "..." } ya da { message: "..." } döner
  if (typeof val === "object") {
    return val.response || val.message || val.text || null;
  }
  return String(val);
}

// ─── Yardımcı: Mood açıklaması ────────────────────────────────────────────────
function getMoodDescription(mood) {
  if (!mood) return "bilinmiyor";
  const energy = mood.energyLevel ?? 5;
  if (energy >= 8) return "pozitif ve enerjik";
  if (energy >= 6) return "normal, iyi hissediyor";
  if (energy >= 4) return "yorgun, orta enerji";
  return "düşük enerji, zorlu bir gün geçiriyor";
}

// ─── Yardımcı: Mood hint'ten template key ────────────────────────────────────
function moodHintToTemplateKey(moodHint) {
  const map = {
    pozitif: "pozitif",
    normal: "normal",
    yorgun: "yorgun",
    zor: "zor",
  };
  return map[moodHint] || "normal";
}

// ─── Ana API Yürütücü ─────────────────────────────────────────────────────────
export async function executeApiAction(apiAction, context = {}) {
  switch (apiAction) {

    // ── Görev Sorgulama ────────────────────────────────────────────────────────
    case "get_active_tasks": {
      const tasks = await taskService.getAllTasks();
      const active = tasks.filter(t =>
        t.status === "Selected" || t.status === "InProgress" || t.status === "Suggested"
      );
      return { tasks: active };
    }

    case "get_completed_today": {
      const tasks = await taskService.getAllTasks();
      const today = new Date().toDateString();
      const completed = tasks.filter(t => {
        if (t.status !== "Completed") return false;
        if (!t.completedAt) return false;
        return new Date(t.completedAt).toDateString() === today;
      });
      return { tasks: completed };
    }

    case "get_postponed_tasks": {
      const tasks = await taskService.getAllTasks();
      const postponed = tasks.filter(t => t.status === "Postponed");
      return { tasks: postponed };
    }

    case "get_daily_pool": {
      const data = await taskService.getDailyPool();
      // Backend { tasks: [...] } veya doğrudan array dönebilir
      const tasks = Array.isArray(data) ? data : (data.tasks || []);
      return { tasks };
    }

    // ── Kişiselleştirilmiş Görev Önerisi ──────────────────────────────────────
    case "get_personalized_tasks": {
      const [allTasksRes, profileRes, moodRes] = await Promise.allSettled([
        axiosClient.get("/user-tasks/available-tasks"),
        userService.getProfile(),
        moodService.getTodayMood(),
      ]);

      // available-tasks endpoint'i zaten level filtrelemesini backend'de yapıyor
      const rawTasks = allTasksRes.status === "fulfilled"
        ? (Array.isArray(allTasksRes.value.data) ? allTasksRes.value.data : [])
        : [];
      const user = profileRes.status === "fulfilled" ? profileRes.value : null;
      const mood = moodRes.status === "fulfilled"
        ? (moodRes.value?.data ?? null)
        : null;

      let filtered = [...rawTasks];

      // ── Kategori filtresi ────────────────────────────────────────────────────
      const cat = context.categoryHint;
      if (cat && cat !== "all" && filtered.length > 0) {
        const catKeywords = {
          saglik:  ["sağlık", "spor", "fitness", "egzersiz", "health", "spor"],
          egitim:  ["eğitim", "okuma", "kitap", "öğrenme", "kurs", "education"],
          mental:  ["zihinsel", "meditasyon", "nefes", "mental", "mindfulness", "psikoloji"],
          sosyal:  ["sosyal", "aile", "arkadaş", "iletişim", "social"],
          kariyer: ["kariyer", "iş", "üretkenlik", "proje", "hedef", "productivity"],
          hobi:    ["hobi", "yaratıcı", "sanat", "müzik", "dans", "hobby", "resim"],
        };
        const keywords = catKeywords[cat] || [];
        const catFiltered = filtered.filter(t => {
          const name = (t.categoryName || "").toLowerCase();
          return keywords.some(kw => name.includes(kw));
        });
        // Eşleşme yoksa orijinal listeyi koru (kullanıcı kafası karışmasın)
        if (catFiltered.length > 0) filtered = catFiltered;
      }

      // ── Zorluk filtresi ──────────────────────────────────────────────────────
      const diff = context.difficultyHint;
      if (diff && diff !== "auto") {
        const diffFiltered = filtered.filter(t => t.difficulty === diff);
        if (diffFiltered.length > 0) filtered = diffFiltered;
      } else if (diff === "auto") {
        // Mood enerji seviyesine göre otomatik zorluk seçimi
        const energy = mood?.energyLevel ?? 5;
        let preferredDiffs;
        if (energy >= 8)      preferredDiffs = ["Hard", "Medium"];
        else if (energy >= 5) preferredDiffs = ["Medium", "Easy"];
        else                  preferredDiffs = ["Easy"];

        const autoFiltered = filtered.filter(t => preferredDiffs.includes(t.difficulty));
        if (autoFiltered.length > 0) filtered = autoFiltered;
      }

      // ── Seviyeye göre sırala (backend zaten filtreler ama sıralama ekle) ────
      if (user?.generalLevel) {
        const lvl = Math.floor(user.generalLevel);
        filtered.sort((a, b) => {
          // MinLevel'ı kullanıcı seviyesine en yakın olanlar önce
          const aDiff = Math.abs((a.minLevel || 1) - lvl);
          const bDiff = Math.abs((b.minLevel || 1) - lvl);
          return aDiff - bDiff;
        });
      }

      // ── Mood'a göre XP ağırlıklandırması ────────────────────────────────────
      // Düşük enerjide küçük XP'li (hızlı biten) görevler öne çıksın
      if (mood?.energyLevel && mood.energyLevel < 5) {
        filtered.sort((a, b) => (a.baseXP || 0) - (b.baseXP || 0));
      }

      // En iyi 5 tane al (shuffle ile çeşitlilik ekle)
      const top = filtered.slice(0, 10);
      const shuffled = top.sort(() => 0.5 - Math.random()).slice(0, 5);

      return {
        tasks: shuffled,
        profile: user,
        mood,
        categoryHint: cat,
        difficultyHint: diff,
      };
    }

    // ── Profil & İstatistik ────────────────────────────────────────────────────
    case "get_profile": {
      const profile = await userService.getProfile();
      return { profile };
    }

    case "get_user_stats": {
      const stats = await userService.getUserStats();
      return { stats };
    }

    case "get_weekly_report": {
      const res = await axiosClient.get("/reports");
      const reports = res.data?.reports || [];
      return { report: reports[0] || null };
    }

    case "get_daily_breakdown": {
      const res = await axiosClient.get("/reports/daily-breakdown");
      return { breakdown: res.data?.breakdown || [] };
    }

    // ── Hedefler ──────────────────────────────────────────────────────────────
    case "get_goals": {
      const goals = await goalService.getGoals();
      return { goals: Array.isArray(goals) ? goals : [] };
    }

    case "get_goals_dashboard": {
      const data = await goalService.getGoalDashboard();
      return { dashboard: data };
    }

    // ── Rozetler ──────────────────────────────────────────────────────────────
    case "get_badges": {
      const badges = await badgeService.getBadgeGallery();
      return { badges: Array.isArray(badges) ? badges : [] };
    }

    // ── Mood ──────────────────────────────────────────────────────────────────
    case "get_today_mood": {
      const res = await moodService.getTodayMood();
      // { success: true, data: { ... } } yapısından data çıkar
      const mood = res?.data ?? res ?? null;
      return { mood };
    }

    case "save_mood": {
      const moodData = {
        moodMode: context.moodMode ?? 0,
        energyLevel: context.energyLevel ?? 5,
        availableMinutes: context.availableMinutes ?? 60,
      };
      await moodService.recordDailyMood(moodData);
      return { saved: true, mood: moodData };
    }

    // ── AI Motivasyon ─────────────────────────────────────────────────────────
    case "get_ai_motivation": {
      // Profil + mood verisini paralel çek
      const [profileRes, moodRes] = await Promise.allSettled([
        userService.getProfile(),
        moodService.getTodayMood(),
      ]);

      const user = profileRes.status === "fulfilled" ? profileRes.value : null;
      const mood = moodRes.status === "fulfilled"
        ? (moodRes.value?.data ?? null)
        : null;

      const moodDesc = context.moodHint
        ? context.moodHint
        : getMoodDescription(mood);

      const levelText = user
        ? `Kullanıcı seviyesi: ${Math.floor(user.generalLevel || 1)}, toplam XP: ${(user.totalXp || 0).toLocaleString("tr-TR")}.`
        : "";

      const prompt = `${levelText} Kullanıcının şu anki ruh hali: ${moodDesc}. Kullanıcıya kısa, içten ve güçlü Türkçe motivasyon mesajı yaz. Sadece motivasyon metnini yaz, JSON veya başka format kullanma.`;

      try {
        const aiResponse = await chatService.sendMessage(prompt);
        // AI servisi { response: "...", intent: "...", ... } döner
        const text = extractText(aiResponse?.response ?? aiResponse);
        if (!text) throw new Error("Boş AI cevabı");
        return { message: text, mood, profile: user, isAI: true };
      } catch {
        // AI servis yoksa veya hata varsa → offline şablonlar
        const key = moodHintToTemplateKey(context.moodHint || getMoodHint(mood?.energyLevel));
        const templates = MOTIVATION_TEMPLATES[key] || MOTIVATION_TEMPLATES.normal;
        const msg = templates[Math.floor(Math.random() * templates.length)];
        return { message: msg, mood, isAI: false };
      }
    }

    // ── Serbest Metin (AI Chat) ────────────────────────────────────────────────
    case "send_free_text": {
      const response = await chatService.sendMessage(context.freeTextMessage);
      return {
        aiResponse: response.response,
        intent: response.intent,
        confidence: response.confidence ?? response.intentConfidence,
        sentiment: response.sentiment,
        sentimentConfidence: response.sentimentConfidence,
        urgencyLevel: response.urgencyLevel ?? 0,
        suggestions: response.suggestions || [],
        actions: response.actions || [],
        flowRedirect: response.flowRedirect || response.flow_redirect || null,
        shouldShowMoodSelector: response.shouldShowMoodSelector,
        // Geriye uyumluluk / Tekil görev önerisi
        suggestedTaskId: response.suggestedTaskId || response.SuggestedTaskId,
        suggestedTaskTitle: response.suggestedTaskTitle || response.SuggestedTaskTitle,
        suggestedTaskXp: response.suggestedTaskXp || response.SuggestedTaskXp,
      };
    }

    default:
      throw new Error(`Bilinmeyen API aksiyonu: ${apiAction}`);
  }
}

// ─── Yardımcı fonksiyonlar ────────────────────────────────────────────────────
function getMoodHint(energyLevel) {
  if (!energyLevel) return "normal";
  if (energyLevel >= 8) return "pozitif";
  if (energyLevel >= 6) return "normal";
  if (energyLevel >= 4) return "yorgun";
  return "zor";
}

// ─── Görev Atama (available-tasks → assign) ───────────────────────────────────
export async function assignTask(taskId) {
  return await taskService.assignSpecificTask(taskId);
}

// ─── Görev Havuzdan Seç (daily-pool → select) ────────────────────────────────
export async function selectTask(taskId) {
  return await taskService.selectTask(taskId);
}
