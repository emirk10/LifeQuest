import React, { useState, useRef, useEffect, useCallback } from "react";
import aiService from "../services/aiService";
import taskService from "../services/taskService";
import chatService from "../services/chatService";
import userService from "../services/userService";
import MiniTaskCard from "./MiniTaskCard";
import GoalRevisionModal from "./GoalRevisionModal";
import axiosClient from "../api/axiosClient";

// ─── Sabitler ────────────────────────────────────────────────────────────────

const URGENCY_STYLES = {
  0: "", // Normal
  1: "border-l-2 border-yellow-500/40",
  2: "border-l-2 border-orange-500/50 bg-orange-500/5",
  3: "border-l-2 border-red-500/50 bg-red-500/5",
};

const INTENT_LABELS = {
  motivasyon: "💪 Motivasyon", gorev_sorgula: "📋 Görev", istatistik: "📊 İstatistik",
  oneri: "🎯 Öneri", oneri_iste: "🎯 Öneri", selamlama: "👋 Selamlama",
  saglik: "💊 Sağlık", rozet: "🏆 Rozet", gorev_ertele: "📅 Erteleme",
  gorev_baslat: "▶️ Başlat", gorev_tamamla: "✅ Tamamla", gorev_vazgec: "🚫 Vazgeç",
  gorev_sec: "🎯 Seç", mood_negatif: "💙 Empati", mood_pozitif: "🔥 Enerji",
  hafif_mod: "🌿 Hafif Mod", istatistik_sorgula: "📊 İstatistik",
};

// ─── ChatBot ─────────────────────────────────────────────────────────────────

const ChatBot = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState([
    {
      id: 1, sender: "bot", type: "assistant",
      text: "Merhaba! 👋 Ben LifeQuest AI asistanın. Görevlerin, istatistiklerin veya motivasyon hakkında konuşabiliriz!",
      time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
    },
  ]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [aiStatus, setAiStatus] = useState("online");
  const [unreadCount, setUnreadCount] = useState(0);
  const [actionLoading, setActionLoading] = useState(null); // action type
  const [revisionModalOpen, setRevisionModalOpen] = useState(false);
  const [revisionCategory, setRevisionCategory] = useState("");
  const [currentGoals, setCurrentGoals] = useState([]);
  const messagesEndRef = useRef(null);
  const inputRef = useRef(null);

  // AI Servis sağlık kontrolü
  useEffect(() => {
    const checkHealth = async () => {
      try {
        const health = await aiService.healthCheck();
        setAiStatus("online");
      } catch { 
        setAiStatus("online"); 
      }
    };
    checkHealth();
    const interval = setInterval(checkHealth, 30000);
    return () => clearInterval(interval);
  }, []);

  // SP-03: Dashboard toast'tan gelen openChatBot event
  useEffect(() => {
    const handleOpen = (e) => {
      setIsOpen(true);
      const nudgeMsg = e?.detail?.message;
      if (nudgeMsg) {
        setMessages(prev => [...prev, {
          id: Date.now(), sender: "bot", type: "proactive",
          text: nudgeMsg,
          time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
        }]);
        setUnreadCount(0); // panel açık olduğu için sıfırla
      }
    };
    window.addEventListener("openChatBot", handleOpen);
    return () => window.removeEventListener("openChatBot", handleOpen);
  }, []);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  useEffect(() => {
    if (isOpen) {
      inputRef.current?.focus();
      setUnreadCount(0);
    }
  }, [isOpen]);

  const addBotMessage = useCallback((text, extras = {}) => {
    const msg = {
      id: Date.now() + Math.random(),
      sender: "bot", type: "assistant",
      text,
      time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
      ...extras,
    };
    setMessages(prev => [...prev, msg]);
    if (!isOpen) setUnreadCount(c => c + 1);
    return msg.id;
  }, [isOpen]);

  const handleSend = async (textOverride) => {
    const trimmed = (textOverride || input).trim();
    if (!trimmed || loading) return;

    const userMsg = {
      id: Date.now(), sender: "user", type: "user",
      text: trimmed,
      time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
    };
    setMessages(prev => [...prev, userMsg]);
    setInput("");
    setLoading(true);

    try {
      const response = await chatService.sendMessage(trimmed);

      const botExtras = {
        intent: response.intent,
        confidence: response.confidence ?? response.intentConfidence,
        sentiment: response.sentiment,
        sentimentConfidence: response.sentimentConfidence,
        urgencyLevel: response.urgencyLevel ?? 0,
        suggestions: response.suggestions || [],
        actions: response.actions || [],
        suggestedTasks: response.suggestedTasks || response.SuggestedTasks || null,
        suggestedTaskId: response.suggestedTaskId || response.SuggestedTaskId,
        suggestedTaskTitle: response.suggestedTaskTitle || response.SuggestedTaskTitle,
        suggestedTaskXp: response.suggestedTaskXp || response.SuggestedTaskXp || response.suggestedTaskXP,
        shouldShowMoodSelector: response.shouldShowMoodSelector,
        actionsUsed: false, // henüz kullanılmadı
        messageId: response.messageId || response.MessageId,
      };

      console.log("[ChatBot] AI Response received:", response);
      addBotMessage(response.response || response.Response, botExtras);
    } catch {
      addBotMessage("⚠️ AI servisine bağlanılamadı. Servisin çalıştığından emin olun (port 5001).", { isError: true });
    } finally {
      setLoading(false);
    }
  };

  const handleAction = async (action, msgId) => {
    // SP-04: Hedef revizyonu aksiyonlarını araya gir
    if (action.type === "update_goal") {
      try {
        const profile = await userService.getProfile();
        setCurrentGoals(profile.userGoals || []);
        setRevisionCategory(action.payload);
        setRevisionModalOpen(true);
        setMessages(prev => prev.map(m => m.id === msgId ? { ...m, actionsUsed: true } : m));
      } catch (err) {
        addBotMessage("⚠️ Hedefler alınırken hata oluştu.", { isError: true });
      }
      return;
    }
    if (action.type === "keep_goal") {
      setMessages(prev => prev.map(m => m.id === msgId ? { ...m, actionsUsed: true } : m));
      addBotMessage("Harika! Bu hedefle devam ediyoruz. 💪");
      return;
    }

    setActionLoading(action.type);
    try {
      const res = await axiosClient.post("/chat/action", {
        messageId: msgId,
        actionType: action.type,
        taskId: action.task_id,
        payload: action.payload,
      });
      const data = res.data;

      // Aksiyonu "kullanıldı" olarak işaretle
      setMessages(prev => prev.map(m => m.id === msgId ? { ...m, actionsUsed: true } : m));

      // Sonuç mesajı ekle
      addBotMessage(data.message || "✅ İşlem tamamlandı!", { type: "action_result" });
    } catch {
      addBotMessage("⚠️ Aksiyon işlenirken bir hata oluştu.", { isError: true });
    } finally {
      setActionLoading(null);
    }
  };

  const handleAssignTask = async (taskId, msgId) => {
    try {
      await taskService.assignSpecificTask(taskId);
      setMessages(prev => prev.map(m => m.id === msgId ? { ...m, suggestedTaskId: null } : m));
      addBotMessage("✅ Görev başarıyla eklendi! Görevler sayfasından detaylara bakabilirsin.");
    } catch (err) {
      addBotMessage("⚠️ Görev eklenirken bir hata oluştu: " + (err.response?.data?.message || err.message), { isError: true });
    }
  };

  const handleSelectMiniTask = async (taskId) => {
    try {
      await axiosClient.post(`/user-tasks/${taskId}/select`);
      addBotMessage("✅ Görev seçildi! Görevler sayfasında aktif görünecek.");
    } catch {
      addBotMessage("⚠️ Görev seçilirken hata oluştu.");
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) { e.preventDefault(); handleSend(); }
  };

  return (
    <>
      {/* Floating Button */}
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
            {aiStatus === "online" && (
              <span className="absolute -top-1 -right-1 w-3 h-3 bg-emerald-400 rounded-full border-2 border-slate-950 animate-pulse" />
            )}
            {unreadCount > 0 && (
              <span className="absolute -top-2 -left-2 min-w-[20px] h-5 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center px-1">
                {unreadCount}
              </span>
            )}
          </div>
        )}
      </button>

      {/* Chat Window */}
      {isOpen && (
        <div className="fixed inset-0 sm:inset-auto sm:bottom-24 sm:right-6 z-50 w-full h-full sm:w-[440px] sm:h-[620px] bg-slate-900 sm:rounded-3xl border-t sm:border border-slate-700/50 shadow-2xl shadow-black/50 flex flex-col overflow-hidden">
          {/* Header */}
          <div className="p-5 bg-gradient-to-r from-indigo-600 to-indigo-700 flex items-center justify-between shrink-0">
            <div className="flex items-center gap-3">
              <button 
                onClick={() => setIsOpen(false)}
                className="sm:hidden text-white/80 hover:text-white mr-1"
              >
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                </svg>
              </button>
              <div className="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center text-lg backdrop-blur-sm">
                🤖
              </div>
              <div>
                <h3 className="text-white font-black text-sm tracking-wide">LifeQuest AI</h3>
                <div className="flex items-center gap-1.5">
                  <span className={`w-2 h-2 rounded-full ${aiStatus === "online" ? "bg-emerald-400 animate-pulse" : "bg-rose-400"}`} />
                  <span className="text-indigo-200 text-[10px] font-medium">
                    {aiStatus === "online" ? "Çevrimiçi" : "Çevrimdışı"}
                  </span>
                </div>
              </div>
            </div>
            <button
              onClick={() => setMessages(messages.slice(0, 1))}
              className="text-white/60 hover:text-white text-xs font-bold px-3 py-1.5 rounded-lg hover:bg-white/10 transition-colors"
              title="Sohbeti temizle"
            >
              🗑️
            </button>
          </div>

          {/* Messages */}
          <div className="flex-1 overflow-y-auto p-4 space-y-4 scrollbar-thin">
            {messages.map((msg) => (
              <div key={msg.id} className={`flex ${msg.sender === "user" ? "justify-end" : "justify-start"}`}>
                <div
                  className={`max-w-[88%] rounded-2xl px-4 py-3 ${
                    msg.sender === "user"
                      ? "bg-indigo-600 text-white rounded-br-md"
                      : msg.isError
                        ? "bg-rose-500/10 border border-rose-500/30 text-rose-300 rounded-bl-md"
                        : msg.type === "action_result"
                          ? "bg-emerald-500/10 border border-emerald-500/30 text-emerald-200 rounded-bl-md"
                          : `bg-slate-800 text-slate-100 rounded-bl-md border border-slate-700/50 ${URGENCY_STYLES[msg.urgencyLevel ?? 0]}`
                  }`}
                >
                  <p className="text-sm leading-relaxed whitespace-pre-wrap">{msg.text}</p>

                  {/* AI Metadata */}
                  {msg.sender === "bot" && msg.intent && (
                    <div className="mt-2 pt-2 border-t border-slate-700/50 flex flex-wrap gap-1.5 text-[10px]">
                      <span className="bg-indigo-500/20 text-indigo-300 px-2 py-0.5 rounded-full font-bold">
                        {INTENT_LABELS[msg.intent] || msg.intent}
                      </span>
                      <span className="bg-slate-700/50 text-slate-400 px-2 py-0.5 rounded-full">
                        %{((msg.confidence ?? 0) * 100).toFixed(0)} güven
                      </span>
                      {msg.urgencyLevel > 0 && (
                        <span className={`px-2 py-0.5 rounded-full font-bold ${
                          msg.urgencyLevel >= 3 ? "bg-red-500/20 text-red-300"
                            : msg.urgencyLevel === 2 ? "bg-orange-500/20 text-orange-300"
                              : "bg-yellow-500/20 text-yellow-300"
                        }`}>
                          ⚠️ Aciliyet {msg.urgencyLevel}
                        </span>
                      )}
                    </div>
                  )}

                  {/* Aksiyon Butonları */}
                  {msg.sender === "bot" && msg.actions?.length > 0 && !msg.actionsUsed && (
                    <div className="mt-3 flex flex-wrap gap-2">
                      {msg.actions.map((action, i) => (
                        <button
                          key={i}
                          onClick={() => handleAction(action, msg.id)}
                          disabled={actionLoading !== null}
                          className={`text-xs border px-3 py-1.5 rounded-lg transition-all font-semibold disabled:opacity-50 ${
                            action.type === 'create_task' 
                              ? 'bg-emerald-500/15 border-emerald-500/40 text-emerald-300 hover:bg-emerald-500/25'
                              : 'bg-indigo-500/15 border-indigo-500/40 text-indigo-300 hover:bg-indigo-500/25'
                          }`}
                        >
                          {actionLoading === action.type ? "⌛" : (action.type === 'create_task' ? `✨ ${action.label}` : action.label)}
                        </button>
                      ))}
                    </div>
                  )}

                  {/* MiniTaskCard'lar (suggested_tasks) */}
                  {msg.sender === "bot" && msg.suggestedTasks?.length > 0 && (
                    <div className="mt-3 flex flex-col gap-2">
                      {msg.suggestedTasks.map((task) => (
                        <MiniTaskCard
                          key={task.taskId}
                          task={task}
                          onSelect={handleSelectMiniTask}
                        />
                      ))}
                    </div>
                  )}

                  {/* Tekil görev önerisi (eski uyumluluk) */}
                  {msg.sender === "bot" && msg.suggestedTaskId && (
                    <div className="mt-3 bg-slate-900/50 rounded-xl p-3 border border-indigo-500/30">
                      <div className="flex justify-between items-start mb-2">
                        <h4 className="text-white font-bold text-sm">{msg.suggestedTaskTitle}</h4>
                        <span className="text-xs font-bold text-amber-400">+{msg.suggestedTaskXp} XP</span>
                      </div>
                      <button
                        onClick={() => handleAssignTask(msg.suggestedTaskId, msg.id)}
                        className="w-full bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-bold py-2 rounded-lg transition-colors"
                      >
                        ✅ Görevi Kabul Et
                      </button>
                    </div>
                  )}

                  {/* Quick Replies */}
                  {msg.suggestions?.length > 0 && (
                    <div className="mt-3 flex flex-wrap gap-1.5">
                      {msg.suggestions.map((s, i) => (
                        <button
                          key={i}
                          onClick={() => handleSend(s)}
                          className="text-[11px] bg-indigo-500/10 border border-indigo-500/30 text-indigo-300 px-3 py-1 rounded-full hover:bg-indigo-500/20 transition-colors font-medium"
                        >
                          {s}
                        </button>
                      ))}
                    </div>
                  )}

                  {/* Mood Selector önerisi */}
                  {msg.shouldShowMoodSelector && (
                    <div className="mt-3 p-2 bg-purple-500/10 border border-purple-500/30 rounded-lg">
                      <p className="text-[11px] text-purple-300 font-semibold mb-1.5">
                        😊 Bugünkü ruh halini güncellemek ister misin?
                      </p>
                      <a href="/daily" className="text-[11px] text-purple-400 underline">
                        Mood seçimine git →
                      </a>
                    </div>
                  )}

                  <p className={`text-[9px] mt-1.5 ${msg.sender === "user" ? "text-indigo-200" : "text-slate-500"}`}>
                    {msg.time}
                  </p>
                </div>
              </div>
            ))}

            {loading && (
              <div className="flex justify-start">
                <div className="bg-slate-800 rounded-2xl rounded-bl-md px-4 py-3 border border-slate-700/50">
                  <div className="flex gap-1.5">
                    {[0, 150, 300].map(delay => (
                      <span key={delay} className="w-2 h-2 bg-indigo-400 rounded-full animate-bounce" style={{ animationDelay: `${delay}ms` }} />
                    ))}
                  </div>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Input */}
          <div className="p-4 border-t border-slate-800 bg-slate-900/80 backdrop-blur-sm shrink-0">
            <div className="flex gap-2">
              <input
                ref={inputRef}
                type="text"
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder="Mesajınızı yazın..."
                className="flex-1 bg-slate-800 border border-slate-700 text-white text-sm rounded-xl px-4 py-3 outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all placeholder:text-slate-500"
                disabled={loading}
              />
              <button
                onClick={() => handleSend()}
                disabled={loading || !input.trim()}
                className="bg-indigo-600 hover:bg-indigo-500 disabled:bg-slate-700 disabled:text-slate-500 text-white px-4 py-3 rounded-xl font-bold transition-all active:scale-95 shrink-0"
              >
                ➤
              </button>
            </div>
          </div>
        </div>
      )}

      {/* SP-04: GoalRevisionModal */}
      <GoalRevisionModal
        isOpen={revisionModalOpen}
        category={revisionCategory}
        currentGoals={currentGoals}
        onClose={() => setRevisionModalOpen(false)}
        onUpdated={(newGoals) => {
          setRevisionModalOpen(false);
          setCurrentGoals(newGoals);
        }}
      />
    </>
  );
};

export default ChatBot;
