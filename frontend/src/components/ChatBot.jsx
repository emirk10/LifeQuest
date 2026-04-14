import React, { useState, useRef, useEffect } from "react";
import aiService from "../services/aiService";

const ChatBot = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState([
    {
      id: 1,
      sender: "bot",
      text: "Merhaba! 👋 Ben LifeQuest AI asistanın. Görevlerin, istatistiklerin veya motivasyon hakkında konuşabiliriz!",
      intent: null,
      sentiment: null,
      time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
    },
  ]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [aiStatus, setAiStatus] = useState("unknown");
  const messagesEndRef = useRef(null);
  const inputRef = useRef(null);

  // AI Servis sağlık kontrolü
  useEffect(() => {
    const checkHealth = async () => {
      try {
        const health = await aiService.healthCheck();
        setAiStatus(health.status === "ok" ? "online" : "offline");
      } catch {
        setAiStatus("offline");
      }
    };
    checkHealth();
    const interval = setInterval(checkHealth, 30000);
    return () => clearInterval(interval);
  }, []);

  // Otomatik scroll
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  // Açılınca input focus
  useEffect(() => {
    if (isOpen) inputRef.current?.focus();
  }, [isOpen]);

  const handleSend = async () => {
    const trimmed = input.trim();
    if (!trimmed || loading) return;

    const userMsg = {
      id: Date.now(),
      sender: "user",
      text: trimmed,
      time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
    };

    setMessages((prev) => [...prev, userMsg]);
    setInput("");
    setLoading(true);

    try {
      const userId = localStorage.getItem("user_data")
        ? JSON.parse(localStorage.getItem("user_data")).id || "anonymous"
        : "anonymous";

      const response = await aiService.chat(userId, trimmed);

      const botMsg = {
        id: Date.now() + 1,
        sender: "bot",
        text: response.response,
        intent: response.intent,
        confidence: response.confidence,
        sentiment: response.sentiment,
        sentimentConfidence: response.sentiment_confidence,
        suggestions: response.suggestions || [],
        time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
      };

      setMessages((prev) => [...prev, botMsg]);
    } catch (err) {
      const errorMsg = {
        id: Date.now() + 1,
        sender: "bot",
        text: "⚠️ AI servisine bağlanılamadı. Servisin çalıştığından emin olun (port 5001).",
        isError: true,
        time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
      };
      setMessages((prev) => [...prev, errorMsg]);
    } finally {
      setLoading(false);
    }
  };

  const handleSuggestionClick = (suggestion) => {
    setInput(suggestion);
    setTimeout(() => {
      handleSendWithText(suggestion);
    }, 100);
  };

  const handleSendWithText = async (text) => {
    const userMsg = {
      id: Date.now(),
      sender: "user",
      text,
      time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
    };
    setMessages((prev) => [...prev, userMsg]);
    setInput("");
    setLoading(true);

    try {
      const userId = localStorage.getItem("user_data")
        ? JSON.parse(localStorage.getItem("user_data")).id || "anonymous"
        : "anonymous";
      const response = await aiService.chat(userId, text);
      const botMsg = {
        id: Date.now() + 1,
        sender: "bot",
        text: response.response,
        intent: response.intent,
        confidence: response.confidence,
        sentiment: response.sentiment,
        sentimentConfidence: response.sentiment_confidence,
        suggestions: response.suggestions || [],
        time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }),
      };
      setMessages((prev) => [...prev, botMsg]);
    } catch {
      setMessages((prev) => [
        ...prev,
        { id: Date.now() + 1, sender: "bot", text: "⚠️ Bağlantı hatası.", isError: true, time: new Date().toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" }) },
      ]);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const sentimentColors = {
    positive: "text-emerald-400",
    negative: "text-rose-400",
    neutral: "text-slate-400",
  };

  const intentLabels = {
    motivasyon: "💪 Motivasyon",
    gorev_sorgula: "📋 Görev Sorgulama",
    istatistik: "📊 İstatistik",
    oneri: "🎯 Öneri",
    selamlama: "👋 Selamlama",
    saglik: "💊 Sağlık",
    rozet: "🏆 Rozet",
  };

  return (
    <>
      {/* Floating Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className={`fixed bottom-6 right-6 z-50 w-16 h-16 rounded-full flex items-center justify-center shadow-2xl transition-all duration-300 hover:scale-110 active:scale-95 ${
          isOpen
            ? "bg-slate-700 rotate-0"
            : "bg-indigo-600 shadow-indigo-500/40 hover:shadow-indigo-500/60"
        }`}
      >
        {isOpen ? (
          <span className="text-2xl text-white">✕</span>
        ) : (
          <div className="relative">
            <span className="text-2xl">🤖</span>
            {aiStatus === "online" && (
              <span className="absolute -top-1 -right-1 w-3 h-3 bg-emerald-400 rounded-full border-2 border-slate-950 animate-pulse"></span>
            )}
          </div>
        )}
      </button>

      {/* Chat Window */}
      {isOpen && (
        <div className="fixed bottom-24 right-6 z-50 w-[420px] h-[600px] bg-slate-900 rounded-3xl border border-slate-700/50 shadow-2xl shadow-black/50 flex flex-col overflow-hidden animate-in">
          {/* Header */}
          <div className="p-5 bg-gradient-to-r from-indigo-600 to-indigo-700 flex items-center justify-between shrink-0">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center text-lg backdrop-blur-sm">
                🤖
              </div>
              <div>
                <h3 className="text-white font-black text-sm tracking-wide">
                  LifeQuest AI
                </h3>
                <div className="flex items-center gap-1.5">
                  <span
                    className={`w-2 h-2 rounded-full ${
                      aiStatus === "online" ? "bg-emerald-400 animate-pulse" : "bg-rose-400"
                    }`}
                  ></span>
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
              <div
                key={msg.id}
                className={`flex ${msg.sender === "user" ? "justify-end" : "justify-start"}`}
              >
                <div
                  className={`max-w-[85%] rounded-2xl px-4 py-3 ${
                    msg.sender === "user"
                      ? "bg-indigo-600 text-white rounded-br-md"
                      : msg.isError
                      ? "bg-rose-500/10 border border-rose-500/30 text-rose-300 rounded-bl-md"
                      : "bg-slate-800 text-slate-100 rounded-bl-md border border-slate-700/50"
                  }`}
                >
                  <p className="text-sm leading-relaxed whitespace-pre-wrap">{msg.text}</p>

                  {/* AI Metadata (bot mesajlarında) */}
                  {msg.sender === "bot" && msg.intent && (
                    <div className="mt-2 pt-2 border-t border-slate-700/50 flex flex-wrap gap-2 text-[10px]">
                      <span className="bg-indigo-500/20 text-indigo-300 px-2 py-0.5 rounded-full font-bold">
                        {intentLabels[msg.intent] || msg.intent}
                      </span>
                      <span className="bg-slate-700/50 text-slate-400 px-2 py-0.5 rounded-full">
                        %{(msg.confidence * 100).toFixed(0)} güven
                      </span>
                      <span
                        className={`px-2 py-0.5 rounded-full font-bold ${
                          msg.sentiment === "positive"
                            ? "bg-emerald-500/20 text-emerald-300"
                            : msg.sentiment === "negative"
                            ? "bg-rose-500/20 text-rose-300"
                            : "bg-slate-700/50 text-slate-400"
                        }`}
                      >
                        {msg.sentiment === "positive" ? "😊 Pozitif" : msg.sentiment === "negative" ? "😔 Negatif" : "😐 Nötr"}
                      </span>
                    </div>
                  )}

                  {/* Suggestions */}
                  {msg.suggestions && msg.suggestions.length > 0 && (
                    <div className="mt-3 flex flex-wrap gap-1.5">
                      {msg.suggestions.map((s, i) => (
                        <button
                          key={i}
                          onClick={() => handleSuggestionClick(s)}
                          className="text-[11px] bg-indigo-500/10 border border-indigo-500/30 text-indigo-300 px-3 py-1 rounded-full hover:bg-indigo-500/20 transition-colors font-medium"
                        >
                          {s}
                        </button>
                      ))}
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
                    <span className="w-2 h-2 bg-indigo-400 rounded-full animate-bounce" style={{ animationDelay: "0ms" }}></span>
                    <span className="w-2 h-2 bg-indigo-400 rounded-full animate-bounce" style={{ animationDelay: "150ms" }}></span>
                    <span className="w-2 h-2 bg-indigo-400 rounded-full animate-bounce" style={{ animationDelay: "300ms" }}></span>
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
                onClick={handleSend}
                disabled={loading || !input.trim()}
                className="bg-indigo-600 hover:bg-indigo-500 disabled:bg-slate-700 disabled:text-slate-500 text-white px-4 py-3 rounded-xl font-bold transition-all active:scale-95 shrink-0"
              >
                ➤
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default ChatBot;
