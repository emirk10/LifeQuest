
import axiosClient from "../../api/axiosClient";
import chatService from "../../services/chatService";
import taskService from "../../services/taskService";
import userService from "../../services/userService";
import { moodService } from "../../api/moodApi";
import badgeService from "../../services/badgeService";
import goalService from "../../services/goalService";
import { getMoodHint } from "../../utils/moodUtils";
import { MOTIVATION_TEMPLATES } from "./chatFlows";


function extractText(val) {
  if (!val) return null;
  if (typeof val === "string") return val;
  
  if (typeof val === "object") {
    return val.response || val.message || val.text || null;
  }
  return String(val);
}


function getMoodDescription(mood) {
  if (!mood) return "bilinmiyor";
  const energy = mood.energyLevel ?? 5;
  if (energy >= 8) return "pozitif ve enerjik";
  if (energy >= 6) return "normal, iyi hissediyor";
  if (energy >= 4) return "yorgun, orta enerji";
  return "düşük enerji, zorlu bir gün geçiriyor";
}


function moodHintToTemplateKey(moodHint) {
  const map = {
    pozitif: "pozitif",
    normal: "normal",
    yorgun: "yorgun",
    zor: "zor",
  };
  return map[moodHint] || "normal";
}


export async function executeApiAction(apiAction, context = {}) {
  switch (apiAction) {

    
    case "get_active_tasks": {
      const res = await taskService.getAllTasks(null, 1, 100);
      const tasks = Array.isArray(res) ? res : (res.tasks || []);
      const active = tasks.filter(t =>
        t.status === "Selected" || t.status === "InProgress" || t.status === "Suggested"
      );
      return { tasks: active };
    }

    case "get_completed_today": {
      const res = await taskService.getAllTasks(null, 1, 100);
      const tasks = Array.isArray(res) ? res : (res.tasks || []);
      const today = new Date().toDateString();
      const completed = tasks.filter(t => {
        if (t.status !== "Completed") return false;
        if (!t.completedAt) return false;
        return new Date(t.completedAt).toDateString() === today;
      });
      return { tasks: completed };
    }

    case "get_postponed_tasks": {
      const res = await taskService.getAllTasks(null, 1, 100);
      const tasks = Array.isArray(res) ? res : (res.tasks || []);
      const postponed = tasks.filter(t => t.status === "Postponed");
      return { tasks: postponed };
    }

    case "get_daily_pool": {
      const data = await taskService.getDailyPool();
      
      const tasks = Array.isArray(data) ? data : (data.tasks || []);
      return { tasks };
    }

    
    case "get_personalized_tasks": {
      const [allTasksRes, profileRes, moodRes] = await Promise.allSettled([
        axiosClient.get("/user-tasks/available-tasks"),
        userService.getProfile(),
        moodService.getTodayMood(),
      ]);

      
      const rawTasks = allTasksRes.status === "fulfilled"
        ? (Array.isArray(allTasksRes.value.data) ? allTasksRes.value.data : [])
        : [];
      const user = profileRes.status === "fulfilled" ? profileRes.value : null;
      const mood = moodRes.status === "fulfilled"
        ? (moodRes.value?.data ?? null)
        : null;

      let filtered = [...rawTasks];

      
      let moodMode = "Normal";
      if (mood && mood.moodMode !== undefined) {
        moodMode = String(mood.moodMode);
      } else if (context.moodMode !== undefined) {
        const modesMap = ["Energetic", "Normal", "Tired", "Overwhelmed", "LimitedTime"];
        moodMode = modesMap[context.moodMode] || "Normal";
      }

      const energyLevel = mood ? (mood.energyLevel ?? 5) : (context.energyLevel ?? 5);
      const availableMinutes = mood ? (mood.availableMinutes ?? 60) : (context.availableMinutes ?? 60);

      filtered = filtered.filter(t => {
        const estMin = t.estimatedMinutes ?? t.EstimatedMinutes ?? (t.baseXP > 0 ? Math.ceil(t.baseXP / 5.0) : 15);
        const minEnergy = t.minEnergyLevel ?? t.MinEnergyLevel ?? 1;

        
        if (estMin > availableMinutes) return false;
        
        if (energyLevel < minEnergy) return false;

        
        if (moodMode === "Overwhelmed" || moodMode === "3") {
          
          if (t.difficulty !== "Easy" || minEnergy > 2 || estMin > 15) {
            return false;
          }
        }
        
        else if (moodMode === "Tired" || moodMode === "2") {
          
          if (minEnergy >= 4 || t.difficulty === "Hard") {
            return false;
          }
        }
        return true;
      });

      
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
        
        if (catFiltered.length > 0) filtered = catFiltered;
      }

      
      const diff = context.difficultyHint;
      if (diff && diff !== "auto") {
        const diffFiltered = filtered.filter(t => t.difficulty === diff);
        if (diffFiltered.length > 0) filtered = diffFiltered;
      } else if (diff === "auto") {
        
        const energy = mood?.energyLevel ?? 5;
        let preferredDiffs;
        if (energy >= 8)      preferredDiffs = ["Hard", "Medium"];
        else if (energy >= 5) preferredDiffs = ["Medium", "Easy"];
        else                  preferredDiffs = ["Easy"];

        const autoFiltered = filtered.filter(t => preferredDiffs.includes(t.difficulty));
        if (autoFiltered.length > 0) filtered = autoFiltered;
      }

      
      if (user?.generalLevel) {
        const lvl = Math.floor(user.generalLevel);
        filtered.sort((a, b) => {
          
          const aDiff = Math.abs((a.minLevel || 1) - lvl);
          const bDiff = Math.abs((b.minLevel || 1) - lvl);
          return aDiff - bDiff;
        });
      }

      
      
      if (mood?.energyLevel && mood.energyLevel < 5) {
        filtered.sort((a, b) => (a.baseXP || 0) - (b.baseXP || 0));
      }

      
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

    
    case "get_goals": {
      const goals = await goalService.getGoals();
      return { goals: Array.isArray(goals) ? goals : [] };
    }

    case "get_goals_dashboard": {
      const data = await goalService.getGoalDashboard();
      return { dashboard: data };
    }

    
    case "get_badges": {
      const badges = await badgeService.getBadgeGallery();
      return { badges: Array.isArray(badges) ? badges : [] };
    }

    
    case "get_today_mood": {
      const res = await moodService.getTodayMood();
      
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

    
    case "get_ai_motivation": {
      
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
        const aiResponse = await chatService.sendMessage(prompt, false);
        
        const text = extractText(aiResponse?.response ?? aiResponse);
        if (!text) throw new Error("Boş AI cevabı");
        return { message: text, mood, profile: user, isAI: true };
      } catch {
        
        const key = moodHintToTemplateKey(context.moodHint || getMoodHint(mood?.energyLevel));
        const templates = MOTIVATION_TEMPLATES[key] || MOTIVATION_TEMPLATES.normal;
        const msg = templates[Math.floor(Math.random() * templates.length)];
        return { message: msg, mood, isAI: false };
      }
    }

    
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
        
        suggestedTaskId: response.suggestedTaskId || response.SuggestedTaskId,
        suggestedTaskTitle: response.suggestedTaskTitle || response.SuggestedTaskTitle,
        suggestedTaskXp: response.suggestedTaskXp || response.SuggestedTaskXp,
      };
    }

    default:
      throw new Error(`Bilinmeyen API aksiyonu: ${apiAction}`);
  }
}


export async function assignTask(taskId) {
  return await taskService.assignSpecificTask(taskId);
}


export async function selectTask(taskId) {
  return await taskService.selectTask(taskId);
}


export async function completeTask(userTaskId, reflectionText = "") {
  return await taskService.completeTask(userTaskId, reflectionText);
}


export async function postponeTask(userTaskId) {
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  const dateStr = tomorrow.toISOString().split('T')[0]; 
  return await taskService.postponeTask(userTaskId, dateStr, "Chatbot üzerinden ertelendi");
}
