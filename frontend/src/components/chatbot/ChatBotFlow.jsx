import React, { useState, useRef, useEffect, useCallback } from "react";
import { useAuth } from "../../context/authContext";
import { FLOWS } from "./chatFlows";
import { executeApiAction, assignTask, selectTask } from "./chatApiHandler";
import {
  TaskCard, CompletedTaskCard, BadgeCard, ProfileCard,
  MoodSavedCard, MotivationCard, WeeklyStatsCard,
  XPBreakdownChart, StatsCard, GoalCard, GoalsDashboardCard,
} from "./ChatBotCards";

// ─── Mesaj fabrikası ──────────────────────────────────────────────────────────
const mkMsg = (sender, content, extras = {}) => ({
  id: Date.now() + Math.random(),
  sender,
  time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
  content,
  ...extras,
});

// ─── Intent → Flow Mapping (AI yanıtından sonra doğru flow'a yönlendirme) ─────
const INTENT_FLOW_MAP = {
  gorev_sorgula: "tasks_menu",
  istatistik: "stats_menu",
  istatistik_sorgula: "stats_menu",
  motivasyon: null,
  oneri: "suggest_task_category",
  oneri_iste: "suggest_task_category",
  rozet: "badges_show",
  saglik: "suggest_task_category",
  selamlama: "main_menu",
  mood_negatif: null,
  mood_pozitif: null,
  hafif_mod: "suggest_task_category",
  alakasiz: "main_menu",
  yardim: "main_menu",
  gorev_baslat: null,
  gorev_tamamla: null,
  gorev_ertele: null,
  gorev_vazgec: null,
  gorev_sec: null,
};

const INTENT_LABELS = {
  motivasyon: "💪 Motivasyon", gorev_sorgula: "📋 Görev", istatistik: "📊 İstatistik",
  oneri: "🎯 Öneri", oneri_iste: "🎯 Öneri", selamlama: "👋 Selamlama",
  saglik: "💊 Sağlık", rozet: "🏆 Rozet", gorev_ertele: "📅 Erteleme",
  gorev_baslat: "▶️ Başlat", gorev_tamamla: "✅ Tamamla", gorev_vazgec: "🚫 Vazgeç",
  gorev_sec: "🎯 Seç", mood_negatif: "💙 Empati", mood_pozitif: "🔥 Enerji",
  hafif_mod: "🌿 Hafif Mod", istatistik_sorgula: "📊 İstatistik",
  alakasiz: "🤔 Genel", yardim: "❓ Yardım",
};

// ─── Ana Bileşen ──────────────────────────────────────────────────────────────
const ChatBotFlow = () => {
  const { user } = useAuth();
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState([]);
  const [loading, setLoading] = useState(false);
  const [unread, setUnread] = useState(0);
  const [flowCtx, setFlowCtx] = useState({});
  const [inputText, setInputText] = useState("");
  const messagesEndRef = useRef(null);
  const inputRef = useRef(null);
  // React Strict Mode çift çalışmayı önlemek için guard
  const initDone = useRef(false);
  // pushNode içinde stale closure'dan kaçınmak için ref
  const flowCtxRef = useRef({});

  // Kullanıcı değişince (logout / farklı hesapla login) her şeyi sıfırla
  useEffect(() => {
    if (!user) {
      // Çıkış yapıldı: chat'i kapat ve temizle
      setIsOpen(false);
      setMessages([]);
      setFlowCtx({});
      flowCtxRef.current = {};
      initDone.current = false;
      return;
    }
    // Yeni kullanıcı girişi veya ilk yükleme: ana menüyü göster
    if (!initDone.current) {
      initDone.current = true;
      pushNode("main_menu");
    }
  }, [user]); // eslint-disable-line react-hooks/exhaustive-deps

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  useEffect(() => {
    if (isOpen) {
      setUnread(0);
      setTimeout(() => inputRef.current?.focus(), 100);
    }
  }, [isOpen]);

  const addMsg = useCallback((msg) => {
    setMessages(prev => [...prev, msg]);
    if (!isOpen) setUnread(c => c + 1);
  }, [isOpen]);

  // Dışarıdan tetikleme (dashboard toast)
  useEffect(() => {
    const handler = (e) => {
      setIsOpen(true);
      if (e?.detail?.message) {
        addMsg(mkMsg("bot", { type: "text", text: e.detail.message }));
      }
    };
    window.addEventListener("openChatBot", handler);
    return () => window.removeEventListener("openChatBot", handler);
  }, [addMsg]); // addMsg değişince handler'ı güncelle

  // ─── Node'u işle ve mesaj olarak ekle ────────────────────────────────────
  const pushNode = useCallback(async (nodeId, ctx = {}) => {
    const node = FLOWS[nodeId];
    if (!node) return;

    const mergedCtx = { ...flowCtxRef.current, ...ctx };

    if (node.type === "options" || node.type === "message") {
      addMsg(mkMsg("bot", {
        type: "options",
        text: node.message,
        options: node.options,
        nodeId,
      }));
      return;
    }

    if (node.type === "api_call") {
      addMsg(mkMsg("bot", { type: "loading", text: node.loadingMessage }));
      setLoading(true);
      try {
        const data = await executeApiAction(node.apiAction, mergedCtx);
        setMessages(prev => prev.filter(m => m.content?.type !== "loading"));
        setLoading(false);
        const nextNode = FLOWS[node.nextNode];
        if (!nextNode) return;
        // Dynamic node'lar renderDataNode'a gönderilmez, pushNode ile işlenir
        if (nextNode.type === "dynamic") {
          // API'den gelen veriyi context'e aktar
          const dataCtx = { ...mergedCtx };
          if (data.mood) dataCtx.moodFromApi = data.mood;
          await pushNode(node.nextNode, dataCtx);
        } else {
          renderDataNode(nextNode, data, mergedCtx);
        }
      } catch (err) {
        setMessages(prev => prev.filter(m => m.content?.type !== "loading"));
        setLoading(false);
        addMsg(mkMsg("bot", { type: "text", text: "⚠️ Bir hata oluştu. Lütfen tekrar deneyin." }));
        addMsg(mkMsg("bot", { type: "options", text: "Ne yapmak istersin?", options: FLOWS.main_menu.options, nodeId: "main_menu" }));
      }
      return;
    }

    if (node.type === "dynamic" && node.dynamicType === "motivation_with_mood") {
      // Mood verisi ya API'den geldi (moodFromApi) ya da burada fetch edelim
      const existingMood = mergedCtx.moodFromApi || null;
      if (existingMood !== undefined && existingMood !== null) {
        // Mood var — direkt AI motivasyon çağrısına geç
        const newCtx = { ...mergedCtx, moodHint: getMoodHint(existingMood.energyLevel) };
        flowCtxRef.current = newCtx;
        setFlowCtx(newCtx);
        await pushNode("motivation_ai_call", newCtx);
      } else {
        // Mood yok — önce mood'unu sor
        addMsg(mkMsg("bot", { type: "loading", text: "Ruh halin kontrol ediliyor..." }));
        setLoading(true);
        try {
          const moodRes = await executeApiAction("get_today_mood", mergedCtx);
          const mood = moodRes.mood;
          setMessages(prev => prev.filter(m => m.content?.type !== "loading"));
          setLoading(false);
          if (!mood) {
            await pushNode("motivation_mood_missing", mergedCtx);
          } else {
            const newCtx = { ...mergedCtx, moodHint: getMoodHint(mood.energyLevel), moodFromApi: mood };
            flowCtxRef.current = newCtx;
            setFlowCtx(newCtx);
            await pushNode("motivation_ai_call", newCtx);
          }
        } catch {
          setMessages(prev => prev.filter(m => m.content?.type !== "loading"));
          setLoading(false);
          await pushNode("motivation_mood_missing", mergedCtx);
        }
      }
      return;
    }
  }, [addMsg]); // flowCtxRef.current kullanıldığı için flowCtx bağımlılığı gerekmez

  // ─── Data node'u render et ─────────────────────────────────────────────────
  const renderDataNode = (node, data, ctx) => {
    if (!node) return;

    switch (node.type) {
      // Aktif / Ertelenen görev listeleri
      case "task_list":
      case "postponed_task_list": {
        const tasks = data.tasks || [];
        if (tasks.length === 0) {
          addMsg(mkMsg("bot", { type: "options", text: node.emptyMessage, options: node.emptyOptions, nodeId: node.id }));
          return;
        }
        addMsg(mkMsg("bot", { type: "task_cards", tasks, isPool: false, options: node.options, nodeId: node.id }));
        break;
      }

      // Tamamlanan görevler
      case "completed_task_list": {
        const tasks = data.tasks || [];
        if (tasks.length === 0) {
          addMsg(mkMsg("bot", { type: "options", text: node.emptyMessage, options: node.emptyOptions, nodeId: node.id }));
          return;
        }
        addMsg(mkMsg("bot", { type: "completed_tasks", tasks, options: node.options, nodeId: node.id }));
        break;
      }

      // Havuz (seç butonu olan liste)
      case "pool_task_list": {
        const tasks = data.tasks || [];
        if (tasks.length === 0) {
          addMsg(mkMsg("bot", { type: "options", text: node.emptyMessage, options: node.emptyOptions, nodeId: node.id }));
          return;
        }
        addMsg(mkMsg("bot", { type: "pool_tasks", tasks, options: node.options, nodeId: node.id }));
        break;
      }

      // Kişiselleştirilmiş öneriler
      case "suggested_task_cards": {
        const tasks = data.tasks || [];
        if (tasks.length === 0) {
          addMsg(mkMsg("bot", { type: "options", text: node.emptyMessage, options: node.emptyOptions, nodeId: node.id }));
          return;
        }
        addMsg(mkMsg("bot", { type: "text", text: buildSuggestionIntro(data) }));
        addMsg(mkMsg("bot", { type: "suggested_tasks", tasks, options: node.options, nodeId: node.id }));
        break;
      }

      case "profile_card":
        addMsg(mkMsg("bot", { type: "profile_card", profile: data.profile, options: node.options, nodeId: node.id }));
        break;

      case "weekly_stats":
        addMsg(mkMsg("bot", { type: "weekly_stats", report: data.report, options: node.options, nodeId: node.id }));
        break;

      case "xp_chart":
        addMsg(mkMsg("bot", { type: "xp_chart", breakdown: data.breakdown, options: node.options, nodeId: node.id }));
        break;

      case "stats_card":
        addMsg(mkMsg("bot", { type: "stats_card", stats: data.stats, options: node.options, nodeId: node.id }));
        break;

      case "goal_list": {
        const goals = data.goals || [];
        if (goals.length === 0) {
          addMsg(mkMsg("bot", { type: "options", text: node.emptyMessage, options: node.emptyOptions, nodeId: node.id }));
          return;
        }
        addMsg(mkMsg("bot", { type: "goal_list", goals, options: node.options, nodeId: node.id }));
        break;
      }

      case "goals_summary":
        addMsg(mkMsg("bot", { type: "goals_dashboard", dashboard: data.dashboard, options: node.options, nodeId: node.id }));
        break;

      case "badge_gallery": {
        const badges = data.badges || [];
        const unlocked = badges.filter(b => b.isUnlocked).length;
        addMsg(mkMsg("bot", { type: "text", text: `🏆 Toplam ${badges.length} rozetten ${unlocked} tanesini kazandın!` }));
        addMsg(mkMsg("bot", { type: "badge_gallery", badges: badges.slice(0, 8), options: node.options, nodeId: node.id }));
        break;
      }

      case "mood_saved":
        addMsg(mkMsg("bot", {
          type: "mood_saved",
          mood: { moodMode: ctx.moodMode, energyLevel: ctx.energyLevel, availableMinutes: ctx.availableMinutes },
          options: node.options,
          nodeId: node.id,
        }));
        break;

      // Motivasyon — AI veya offline
      case "motivation_card":
        if (!data.message || typeof data.message !== "string") {
          // Güvenlik: boş veya geçersiz cevap gelirse offline fallback
          addMsg(mkMsg("bot", { type: "text", text: "💪 Bugün küçük bir adım bile büyük bir zafer. Devam et!" }));
        } else {
          addMsg(mkMsg("bot", {
            type: "motivation_card",
            message: data.message,
            isAI: data.isAI,
            options: node.options,
            nodeId: node.id,
          }));
        }
        break;

      default:
        // JSON dump yerine kullanıcı dostu mesaj
        addMsg(mkMsg("bot", {
          type: "options",
          text: "İşlem tamamlandı! Başka bir şey yapayım mı?",
          options: FLOWS.main_menu.options,
          nodeId: "main_menu",
        }));
    }
  };

  // ─── Buton tıklaması ──────────────────────────────────────────────────────
  const handleOption = useCallback(async (opt, msgId) => {
    if (loading) return;

    // Kullanıcı mesajı ekle
    addMsg(mkMsg("user", { type: "text", text: opt.label }));

    // Eski mesajdaki seçenekleri devre dışı bırak
    setMessages(prev => prev.map(m => m.id === msgId ? { ...m, content: { ...m.content, optionsUsed: true } } : m));

    // Context güncelle
    const ctxUpdate = {};
    if (opt.categoryHint !== undefined) ctxUpdate.categoryHint = opt.categoryHint;
    if (opt.difficultyHint !== undefined) ctxUpdate.difficultyHint = opt.difficultyHint;
    if (opt.moodHint !== undefined) ctxUpdate.moodHint = opt.moodHint;
    if (opt.moodMode !== undefined) ctxUpdate.moodMode = opt.moodMode;
    if (opt.energyLevel !== undefined) ctxUpdate.energyLevel = opt.energyLevel;
    if (opt.availableMinutes !== undefined) ctxUpdate.availableMinutes = opt.availableMinutes;
    const newCtx = { ...flowCtxRef.current, ...ctxUpdate };
    flowCtxRef.current = newCtx;
    setFlowCtx(newCtx);

    // Özel aksiyon
    if (opt.action === "navigate" && opt.target) {
      window.location.href = opt.target;
      return;
    }

    if (opt.nextNode) {
      await pushNode(opt.nextNode, newCtx);
    }
  }, [loading, addMsg, pushNode]);

  // ─── Görev atama ─────────────────────────────────────────────────────────
  const handleAssign = async (task) => {
    setLoading(true);
    try {
      const res = await assignTask(task.id || task.taskId);
      
      // Backend'den 200 dönse de success false (örn: limit aşıldı) olabilir
      if (res && (res.success === false || res.Success === false)) {
        throw new Error(res.message || res.Message || "Görev eklenemedi.");
      }

      addMsg(mkMsg("bot", { type: "text", text: `✅ "${task.title}" görevin eklendi! Görevler sayfasından takip edebilirsin.` }));
      addMsg(mkMsg("bot", { type: "options", text: "Başka bir şey yapayım mı?", options: FLOWS.main_menu.options, nodeId: "main_menu" }));
    } catch (err) {
      const errMsg = err.response?.data?.message || err.response?.data || err.message || "Bilinmeyen bir hata oluştu.";
      addMsg(mkMsg("bot", { type: "text", text: `⚠️ Görev eklenemedi: ${errMsg}` }));
      addMsg(mkMsg("bot", { type: "options", text: "Başka bir görev seçmek ister misin?", options: FLOWS.main_menu.options, nodeId: "main_menu" }));
    } finally {
      setLoading(false);
    }
  };

  const handleSelectPoolTask = async (task) => {
    setLoading(true);
    try {
      await selectTask(task.id || task.taskId);
      addMsg(mkMsg("bot", { type: "text", text: `✅ "${task.title}" seçildi! Görevler sayfasında aktif görünecek.` }));
      addMsg(mkMsg("bot", { type: "options", text: "Başka bir şey yapayım mı?", options: FLOWS.main_menu.options, nodeId: "main_menu" }));
    } catch (err) {
      const errMsg = err.response?.data?.message || err.response?.data || err.message || "Görev seçilirken hata oluştu.";
      addMsg(mkMsg("bot", { type: "text", text: `⚠️ Hata: ${errMsg}` }));
    } finally {
      setLoading(false);
    }
  };

  // ─── Sohbeti sıfırla ──────────────────────────────────────────────────────
  const handleReset = () => {
    setMessages([]);
    setFlowCtx({});
    flowCtxRef.current = {};
    setInputText("");
    // initDone'u sıfırlamaya gerek yok — manuel reset
    setTimeout(() => pushNode("main_menu"), 50);
  };

  // ─── Serbest metin gönderimi (AI Chat) ──────────────────────────────────────
  const handleFreeText = useCallback(async (textOverride) => {
    const trimmed = (textOverride || inputText).trim();
    if (!trimmed || loading) return;

    // Kullanıcı mesajını ekle
    addMsg(mkMsg("user", { type: "text", text: trimmed }));
    setInputText("");

    // Yükleniyor animasyonu
    addMsg(mkMsg("bot", { type: "loading", text: "Düşünüyorum..." }));
    setLoading(true);

    try {
      const result = await executeApiAction("send_free_text", {
        ...flowCtxRef.current,
        freeTextMessage: trimmed,
      });

      // Loading mesajını kaldır
      setMessages(prev => prev.filter(m => m.content?.type !== "loading"));
      setLoading(false);

      // AI yanıtını göster (metadata ile)
      addMsg(mkMsg("bot", {
        type: "ai_response",
        text: result.aiResponse,
        ...result
      }));

      // Flow yönlendirmesi varsa, ilgili node'un butonlarını göster
      const flowTarget = result.flowRedirect || INTENT_FLOW_MAP[result.intent];
      if (flowTarget && FLOWS[flowTarget]) {
        const targetNode = FLOWS[flowTarget];
        // Kısa gecikme ile butonları göster (doğal his vermesi için)
        setTimeout(() => {
          if (targetNode.type === "api_call") {
            // API call node ise direkt çalıştır
            pushNode(flowTarget, flowCtxRef.current);
          } else {
            // Options node ise butonları göster
            addMsg(mkMsg("bot", {
              type: "options",
              text: null, // AI yanıtı zaten gösterildi
              options: targetNode.options,
              nodeId: flowTarget,
            }));
          }
        }, 300);
      } else {
        // Flow yönlendirmesi yoksa (görev aksiyon intent'leri) ana menü butonlarını göster
        setTimeout(() => {
          addMsg(mkMsg("bot", {
            type: "options",
            text: "Başka bir şey yapmak ister misin?",
            options: FLOWS.main_menu.options,
            nodeId: "main_menu",
          }));
        }, 300);
      }
    } catch {
      setMessages(prev => prev.filter(m => m.content?.type !== "loading"));
      setLoading(false);
      addMsg(mkMsg("bot", { type: "text", text: "⚠️ AI servisine bağlanamadı. Lütfen tekrar deneyin." }));
      addMsg(mkMsg("bot", { type: "options", text: "Ne yapmak istersin?", options: FLOWS.main_menu.options, nodeId: "main_menu" }));
    }
  }, [inputText, loading, addMsg, pushNode]);

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleFreeText();
    }
  };

  // ─── Mesaj render ────────────────────────────────────────────────────────
  const renderMessage = (msg) => {
    const { content } = msg;
    const isUser = msg.sender === "user";

    if (isUser) {
      return (
        <div key={msg.id} className="flex justify-end">
          <div className="max-w-[80%] bg-indigo-600 text-white rounded-2xl rounded-br-md px-4 py-2.5">
            <p className="text-sm">{content.text}</p>
            <p className="text-[9px] text-indigo-200 mt-1">{msg.time}</p>
          </div>
        </div>
      );
    }

    if (content.type === "loading") {
      return (
        <div key={msg.id} className="flex justify-start">
          <div className="bg-slate-800 rounded-2xl rounded-bl-md px-4 py-3 border border-slate-700/50">
            <p className="text-slate-400 text-xs mb-2">{content.text}</p>
            <div className="flex gap-1.5">
              {[0, 150, 300].map(d => (
                <span key={d} className="w-2 h-2 bg-indigo-400 rounded-full animate-bounce" style={{ animationDelay: `${d}ms` }} />
              ))}
            </div>
          </div>
        </div>
      );
    }

    return (
      <div key={msg.id} className="flex justify-start">
        <div className="max-w-[92%] space-y-2 w-full">
          {/* Bot avatar */}
          <div className="flex items-center gap-1.5 mb-1">
            <div className="w-5 h-5 bg-indigo-600/50 rounded-full flex items-center justify-center text-[10px]">🤖</div>
            <span className="text-[9px] text-slate-500">{msg.time}</span>
          </div>

          {/* Metin (ai_response hariç — kendi render'ı var) */}
          {content.text && content.type !== "ai_response" && (
            <div className="bg-slate-800 rounded-2xl rounded-bl-md px-4 py-3 border border-slate-700/50">
              <p className="text-sm text-slate-100 leading-relaxed whitespace-pre-wrap">{content.text}</p>
            </div>
          )}

          {/* AI Yanıtı (metadata ile) */}
          {content.type === "ai_response" && (
            <div className="bg-slate-800 rounded-2xl rounded-bl-md px-4 py-3 border border-slate-700/50">
              <p className="text-sm text-slate-100 leading-relaxed whitespace-pre-wrap">{content.text}</p>
              <div className="mt-2 pt-2 border-t border-slate-700/50 flex flex-wrap gap-1.5 text-[10px]">
                <span className="bg-indigo-500/20 text-indigo-300 px-2 py-0.5 rounded-full font-bold">
                  {INTENT_LABELS[content.intent] || content.intent}
                </span>
                <span className="bg-slate-700/50 text-slate-400 px-2 py-0.5 rounded-full">
                  %{((content.confidence ?? 0) * 100).toFixed(0)} güven
                </span>
                {content.urgencyLevel > 0 && (
                  <span className={`px-2 py-0.5 rounded-full font-bold ${
                    content.urgencyLevel >= 3 ? "bg-red-500/20 text-red-300"
                      : content.urgencyLevel === 2 ? "bg-orange-500/20 text-orange-300"
                        : "bg-yellow-500/20 text-yellow-300"
                  }`}>
                    ⚠️ Aciliyet {content.urgencyLevel}
                  </span>
                )}
              </div>

              {/* Tekil görev önerisi (eski uyumluluk) */}
              {content.suggestedTaskId && (
                <div className="mt-3 bg-indigo-500/10 rounded-xl p-3 border border-indigo-500/30">
                  <div className="flex justify-between items-start mb-2">
                    <h4 className="text-white font-bold text-sm">{content.suggestedTaskTitle}</h4>
                    <span className="text-xs font-bold text-amber-400">+{content.suggestedTaskXp} XP</span>
                  </div>
                  <button
                    onClick={() => {
                      handleAssign({ id: content.suggestedTaskId, title: content.suggestedTaskTitle });
                      // Atandıktan sonra butonu gizle (opsiyonel)
                      setMessages(prev => prev.map(m => m.id === msg.id ? { ...m, content: { ...m.content, suggestedTaskId: null } } : m));
                    }}
                    className="w-full bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-bold py-2 rounded-lg transition-colors"
                  >
                    ✅ Görevi Kabul Et
                  </button>
                </div>
              )}
            </div>
          )}

          {/* Özel kartlar */}
          {content.type === "profile_card" && <ProfileCard profile={content.profile} />}
          {content.type === "weekly_stats" && <WeeklyStatsCard report={content.report} />}
          {content.type === "xp_chart" && <XPBreakdownChart breakdown={content.breakdown} />}
          {content.type === "stats_card" && <StatsCard stats={content.stats} />}
          {content.type === "goals_dashboard" && <GoalsDashboardCard dashboard={content.dashboard} />}
          {content.type === "motivation_card" && <MotivationCard message={content.message} isAI={content.isAI} />}
          {content.type === "mood_saved" && <MoodSavedCard mood={content.mood} />}

          {/* Aktif / ertelenen görev listesi (buton yok) */}
          {content.type === "task_cards" && content.tasks?.map(t => (
            <TaskCard key={t.id || t.taskId} task={t} />
          ))}

          {/* Tamamlananlar */}
          {content.type === "completed_tasks" && content.tasks?.map(t => (
            <CompletedTaskCard key={t.id || t.taskId} task={t} />
          ))}

          {/* Havuz görevleri — seç butonu var */}
          {content.type === "pool_tasks" && content.tasks?.map(t => (
            <TaskCard
              key={t.id || t.taskId}
              task={t}
              onAction={handleSelectPoolTask}
              actionLabel="Seç"
              actionIcon="✅"
            />
          ))}

          {content.type === "suggested_tasks" && content.tasks?.map(t => (
            <TaskCard
              key={t.id || t.taskId}
              task={t}
              onAction={handleAssign}
              actionLabel="Göreve Başla"
              actionIcon="🚀"
              actionColor="emerald"
            />
          ))}

          {content.type === "badge_gallery" && (
            <div className="space-y-2">
              {content.badges?.map((b, i) => <BadgeCard key={i} badge={b} />)}
            </div>
          )}

          {content.type === "goal_list" && (
            <div className="space-y-2">
              {content.goals?.map((g, i) => <GoalCard key={i} goal={g} />)}
            </div>
          )}

          {/* Seçenek Butonları */}
          {content.options?.length > 0 && !content.optionsUsed && (
            <div className="flex flex-wrap gap-2 pt-1">
              {content.options.map((opt, i) => (
                <button
                  key={i}
                  onClick={() => handleOption(opt, msg.id)}
                  disabled={loading}
                  className="text-xs font-semibold px-3 py-2 rounded-xl border transition-all disabled:opacity-40 disabled:cursor-not-allowed
                    bg-slate-700/60 border-slate-600/50 text-slate-200 hover:bg-indigo-600/30 hover:border-indigo-500/50 hover:text-white active:scale-95"
                >
                  {opt.label}
                </button>
              ))}
            </div>
          )}
          {content.optionsUsed && content.options?.length > 0 && (
            <p className="text-[10px] text-slate-600 italic">Seçim yapıldı</p>
          )}
        </div>
      </div>
    );
  };

  // Giriş yapılmamışsa chatbot'u tamamen gizle
  if (!user) return null;

  return (
    <>
      {/* Floating Buton */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className={`fixed bottom-6 right-6 z-50 w-16 h-16 rounded-full flex items-center justify-center shadow-2xl transition-all duration-300 hover:scale-110 active:scale-95 ${
          isOpen ? "bg-slate-700" : "bg-indigo-600 shadow-indigo-500/40 hover:shadow-indigo-500/60"
        }`}
      >
        {isOpen ? (
          <span className="text-2xl text-white">✕</span>
        ) : (
          <div className="relative">
            <span className="text-2xl">🤖</span>
            <span className="absolute -top-1 -right-1 w-3 h-3 bg-emerald-400 rounded-full border-2 border-slate-950 animate-pulse" />
            {unread > 0 && (
              <span className="absolute -top-2 -left-2 min-w-[20px] h-5 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center px-1">
                {unread}
              </span>
            )}
          </div>
        )}
      </button>

      {/* Chat Penceresi */}
      {isOpen && (
        <div className="fixed inset-0 sm:inset-auto sm:bottom-24 sm:right-6 z-50 w-full h-full sm:w-[420px] sm:h-[650px] bg-slate-900 sm:rounded-3xl border-t sm:border border-slate-700/50 shadow-2xl shadow-black/50 flex flex-col overflow-hidden">
          
          {/* Header */}
          <div className="p-4 bg-gradient-to-r from-indigo-600 to-indigo-700 flex items-center justify-between shrink-0">
            <div className="flex items-center gap-3">
              <button onClick={() => setIsOpen(false)} className="sm:hidden text-white/80 hover:text-white mr-1">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div className="w-9 h-9 bg-white/20 rounded-xl flex items-center justify-center text-base backdrop-blur-sm">🤖</div>
              <div>
                <h3 className="text-white font-black text-sm tracking-wide">LifeQuest Asistan</h3>
                <div className="flex items-center gap-1.5">
                  <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
                  <span className="text-indigo-200 text-[10px] font-medium">Çevrimiçi · Yardıma hazır</span>
                </div>
              </div>
            </div>
            <button
              onClick={handleReset}
              className="text-white/60 hover:text-white text-xs font-bold px-2.5 py-1.5 rounded-lg hover:bg-white/10 transition-colors"
              title="Sohbeti yenile"
            >
              🔄
            </button>
          </div>

          {/* Mesajlar */}
          <div className="flex-1 overflow-y-auto p-4 space-y-4 scrollbar-thin scrollbar-thumb-slate-700 scrollbar-track-transparent">
            {messages.map(msg => renderMessage(msg))}
            <div ref={messagesEndRef} />
          </div>

          {/* Mesaj Giriş Alanı */}
          <div className="p-3 border-t border-slate-800 bg-slate-900/80 backdrop-blur-sm shrink-0">
            <div className="flex gap-2">
              <input
                ref={inputRef}
                type="text"
                value={inputText}
                onChange={(e) => setInputText(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder="Mesajını yaz veya seçeneklere tıkla..."
                className="flex-1 bg-slate-800 border border-slate-700 text-white text-sm rounded-xl px-4 py-2.5 outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all placeholder:text-slate-500"
                disabled={loading}
              />
              <button
                onClick={() => handleFreeText()}
                disabled={loading || !inputText.trim()}
                className="bg-indigo-600 hover:bg-indigo-500 disabled:bg-slate-700 disabled:text-slate-500 text-white px-4 py-2.5 rounded-xl font-bold transition-all active:scale-95 shrink-0"
              >
                ➤
              </button>
            </div>
            <p className="text-[9px] text-slate-600 mt-1.5 text-center">AI destekli asistan • Yazı yazabilir veya butonlara tıklayabilirsin</p>
          </div>
        </div>
      )}
    </>
  );
};

export default ChatBotFlow;

// ─── Yardımcı ─────────────────────────────────────────────────────────────────
function getMoodHint(energyLevel) {
  if (!energyLevel) return "normal";
  if (energyLevel >= 8) return "pozitif";
  if (energyLevel >= 6) return "normal";
  if (energyLevel >= 4) return "yorgun";
  return "zor";
}

function buildSuggestionIntro(data) {
  const cat = data.categoryHint;
  const diff = data.difficultyHint;
  const energy = data.mood?.energyLevel;
  const level = data.profile?.generalLevel ? Math.floor(data.profile.generalLevel) : null;

  let intro = "🎯 İşte sana özel görev önerileri!";
  const parts = [];
  if (level) parts.push(`Seviye ${level}`);
  if (energy) parts.push(`Enerji: ${energy}/10`);
  if (diff && diff !== "auto") parts.push(diff === "Easy" ? "Kolay" : diff === "Medium" ? "Orta" : "Zor");
  if (parts.length > 0) intro += `\n📌 ${parts.join(" · ")}`;
  return intro;
}
