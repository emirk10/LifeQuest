import React, { useState, useEffect } from "react";
import MainLayout from "../layouts/MainLayout";
import aiService from "../services/aiService";

const AITestPage = () => {
  // ---- Health ----
  const [health, setHealth] = useState(null);
  const [healthLoading, setHealthLoading] = useState(false);

  // ---- Chat ----
  const [chatMessage, setChatMessage] = useState("");
  const [chatResult, setChatResult] = useState(null);
  const [chatLoading, setChatLoading] = useState(false);
  const [chatHistory, setChatHistory] = useState([]);

  // ---- Reflection ----
  const [reflectionText, setReflectionText] = useState("");
  const [reflectionMinWords, setReflectionMinWords] = useState(10);
  const [reflectionCategory, setReflectionCategory] = useState("spor");
  const [reflectionResult, setReflectionResult] = useState(null);
  const [reflectionLoading, setReflectionLoading] = useState(false);

  // ---- Image ----
  const [imageFile, setImageFile] = useState(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [imageCategory, setImageCategory] = useState("spor");
  const [imageResult, setImageResult] = useState(null);
  const [imageLoading, setImageLoading] = useState(false);

  // ---- Train ----
  const [trainResult, setTrainResult] = useState(null);
  const [trainLoading, setTrainLoading] = useState(false);

  // ---- Active Tab ----
  const [activeTab, setActiveTab] = useState("chat");

  // Sayfa yüklenince health check
  useEffect(() => {
    handleHealthCheck();
  }, []);

  // ============================================================
  //  HANDLERS
  // ============================================================

  const handleHealthCheck = async () => {
    setHealthLoading(true);
    try {
      const data = await aiService.healthCheck();
      setHealth(data);
    } catch (err) {
      setHealth({ status: "error", message: err.message });
    } finally {
      setHealthLoading(false);
    }
  };

  const handleChat = async () => {
    if (!chatMessage.trim()) return;
    setChatLoading(true);
    try {
      const data = await aiService.chat("test-user", chatMessage);
      setChatResult(data);
      setChatHistory((prev) => [
        ...prev,
        { message: chatMessage, result: data, time: new Date().toLocaleTimeString("tr-TR") },
      ]);
      setChatMessage("");
    } catch (err) {
      setChatResult({ error: err.message });
    } finally {
      setChatLoading(false);
    }
  };

  const handleReflection = async () => {
    if (!reflectionText.trim()) return;
    setReflectionLoading(true);
    try {
      const data = await aiService.verifyReflection(
        reflectionText,
        reflectionMinWords,
        reflectionCategory
      );
      setReflectionResult(data);
    } catch (err) {
      setReflectionResult({ error: err.message });
    } finally {
      setReflectionLoading(false);
    }
  };

  const handleImageUpload = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    setImageFile(file);

    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result);
    };
    reader.readAsDataURL(file);
  };

  const handleImageVerify = async () => {
    if (!imagePreview) return;
    setImageLoading(true);
    try {
      // data:image/jpeg;base64,... → base64 kısmını al
      const base64 = imagePreview.split(",")[1];
      const data = await aiService.verifyImage(
        "test-user",
        "test-task",
        imageCategory,
        base64
      );
      setImageResult(data);
    } catch (err) {
      setImageResult({ error: err.message });
    } finally {
      setImageLoading(false);
    }
  };

  const handleTrain = async () => {
    setTrainLoading(true);
    try {
      const data = await aiService.trainModels();
      setTrainResult(data);
    } catch (err) {
      setTrainResult({ error: err.message });
    } finally {
      setTrainLoading(false);
    }
  };

  // ============================================================
  //  TABS CONFIG
  // ============================================================
  const tabs = [
    { id: "chat", name: "Chatbot", icon: "🤖" },
    { id: "reflection", name: "Yansıma Doğrulama", icon: "📝" },
    { id: "image", name: "Görüntü Doğrulama", icon: "📸" },
    { id: "train", name: "Model Eğitimi", icon: "🧠" },
  ];

  // Quick test messages
  const quickMessages = [
    { text: "merhaba nasılsın", label: "Selamlama" },
    { text: "moralim bozuk yapamıyorum", label: "Motivasyon -" },
    { text: "bugün çok enerjiktim harika", label: "Motivasyon +" },
    { text: "bugünkü görevlerim ne", label: "Görev" },
    { text: "kaç XP kazandım", label: "İstatistik" },
    { text: "bana görev öner", label: "Öneri" },
    { text: "egzersiz öner", label: "Sağlık" },
    { text: "rozetlerim neler", label: "Rozet" },
  ];

  // ============================================================
  //  RENDER
  // ============================================================

  return (
    <MainLayout>
      <div className="space-y-8 max-w-6xl mx-auto">
        {/* Header */}
        <section className="relative">
          <div className="absolute -left-10 -top-10 w-40 h-40 bg-purple-600/10 blur-[80px] rounded-full"></div>
          <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2 relative z-10">
            AI TEST PANEL
          </h2>
          <div className="h-1.5 w-24 bg-purple-600 rounded-full shadow-[0_0_20px_rgba(147,51,234,0.6)]"></div>
          <p className="text-slate-500 mt-2 text-sm italic">
            Tüm yapay zeka özelliklerini buradan test edebilirsin
          </p>
        </section>

        {/* Status Bar */}
        <div className="flex items-center gap-4 p-4 bg-slate-900/60 rounded-2xl border border-slate-800">
          <div className="flex items-center gap-2">
            <span
              className={`w-3 h-3 rounded-full ${
                health?.status === "ok"
                  ? "bg-emerald-400 animate-pulse"
                  : health?.status === "error"
                  ? "bg-rose-400"
                  : "bg-amber-400 animate-pulse"
              }`}
            ></span>
            <span className="text-sm font-bold text-slate-300">
              AI Servis:{" "}
              {health?.status === "ok"
                ? "Çevrimiçi"
                : health?.status === "error"
                ? "Bağlantı Hatası"
                : "Kontrol ediliyor..."}
            </span>
          </div>
          {health?.status === "ok" && (
            <>
              <span className="text-slate-700">|</span>
              <span className="text-xs text-slate-500">
                Intent Model: {health.intent_model_loaded ? "✅" : "❌"} | Sentiment
                Model: {health.sentiment_model_loaded ? "✅" : "❌"} | v{health.version}
              </span>
            </>
          )}
          <button
            onClick={handleHealthCheck}
            disabled={healthLoading}
            className="ml-auto text-xs bg-slate-800 hover:bg-slate-700 text-slate-300 px-3 py-1.5 rounded-lg transition-colors font-medium"
          >
            {healthLoading ? "..." : "🔄 Yenile"}
          </button>
        </div>

        {/* Tabs */}
        <div className="flex gap-2 p-1 bg-slate-900/40 rounded-2xl border border-slate-800">
          {tabs.map((tab) => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex-1 flex items-center justify-center gap-2 py-3 rounded-xl text-sm font-bold transition-all ${
                activeTab === tab.id
                  ? "bg-purple-600/20 text-purple-400 border border-purple-600/30 shadow-lg"
                  : "text-slate-500 hover:text-slate-300 hover:bg-slate-800/50"
              }`}
            >
              <span>{tab.icon}</span>
              <span className="hidden sm:inline">{tab.name}</span>
            </button>
          ))}
        </div>

        {/* ======================== CHAT TAB ======================== */}
        {activeTab === "chat" && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Input Panel */}
            <div className="lg:col-span-2 bg-slate-900/60 rounded-[2rem] border border-slate-800 p-6 space-y-4">
              <h3 className="text-white font-black text-sm uppercase tracking-widest flex items-center gap-2">
                <span className="w-2 h-2 bg-purple-500 rounded-full"></span>
                Chatbot Test
              </h3>

              {/* Quick Buttons */}
              <div className="flex flex-wrap gap-2">
                {quickMessages.map((q, i) => (
                  <button
                    key={i}
                    onClick={() => {
                      setChatMessage(q.text);
                    }}
                    className="text-[11px] bg-slate-800 border border-slate-700 text-slate-400 hover:text-white hover:border-purple-500/50 px-3 py-1.5 rounded-lg transition-all font-medium"
                  >
                    {q.label}
                  </button>
                ))}
              </div>

              {/* Input */}
              <div className="flex gap-3">
                <input
                  type="text"
                  value={chatMessage}
                  onChange={(e) => setChatMessage(e.target.value)}
                  onKeyDown={(e) => e.key === "Enter" && handleChat()}
                  placeholder="Mesajını yaz... (örn: beni motive et)"
                  className="flex-1 bg-slate-950/50 border border-slate-800 rounded-xl px-4 py-3 text-white text-sm outline-none focus:ring-2 focus:ring-purple-500/50 focus:border-purple-500 transition-all placeholder:text-slate-600"
                  disabled={chatLoading}
                />
                <button
                  onClick={handleChat}
                  disabled={chatLoading || !chatMessage.trim()}
                  className="bg-purple-600 hover:bg-purple-500 disabled:bg-slate-700 text-white px-6 py-3 rounded-xl font-black text-sm transition-all active:scale-95 uppercase tracking-wider"
                >
                  {chatLoading ? "⏳" : "Gönder"}
                </button>
              </div>

              {/* Result */}
              {chatResult && !chatResult.error && (
                <div className="bg-slate-950/50 rounded-2xl border border-slate-800 p-5 space-y-3 animate-fade-in">
                  <p className="text-white text-sm leading-relaxed">{chatResult.response}</p>
                  <div className="flex flex-wrap gap-2 pt-2 border-t border-slate-800">
                    <span className="text-[10px] bg-purple-500/20 text-purple-300 px-2.5 py-1 rounded-full font-bold">
                      Intent: {chatResult.intent}
                    </span>
                    <span className="text-[10px] bg-slate-700/50 text-slate-300 px-2.5 py-1 rounded-full">
                      Güven: %{(chatResult.confidence * 100).toFixed(1)}
                    </span>
                    <span
                      className={`text-[10px] px-2.5 py-1 rounded-full font-bold ${
                        chatResult.sentiment === "positive"
                          ? "bg-emerald-500/20 text-emerald-300"
                          : chatResult.sentiment === "negative"
                          ? "bg-rose-500/20 text-rose-300"
                          : "bg-slate-700/50 text-slate-400"
                      }`}
                    >
                      Duygu: {chatResult.sentiment} (%
                      {(chatResult.sentiment_confidence * 100).toFixed(0)})
                    </span>
                  </div>
                  {chatResult.suggestions?.length > 0 && (
                    <div className="flex flex-wrap gap-1.5">
                      {chatResult.suggestions.map((s, i) => (
                        <button
                          key={i}
                          onClick={() => setChatMessage(s)}
                          className="text-[11px] bg-purple-500/10 border border-purple-500/30 text-purple-300 px-3 py-1 rounded-full hover:bg-purple-500/20 transition-colors"
                        >
                          {s}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              )}
              {chatResult?.error && (
                <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-4 text-rose-300 text-sm">
                  ⚠️ Hata: {chatResult.error}
                </div>
              )}
            </div>

            {/* History Panel */}
            <div className="bg-slate-900/60 rounded-[2rem] border border-slate-800 p-6 max-h-[500px] overflow-y-auto">
              <h3 className="text-white font-black text-sm uppercase tracking-widest mb-4 flex items-center gap-2">
                <span className="w-2 h-2 bg-indigo-500 rounded-full"></span>
                Geçmiş ({chatHistory.length})
              </h3>
              {chatHistory.length === 0 ? (
                <p className="text-slate-600 text-xs italic">Henüz test yapılmadı</p>
              ) : (
                <div className="space-y-3">
                  {chatHistory.map((item, i) => (
                    <div
                      key={i}
                      className="bg-slate-950/40 rounded-xl p-3 border border-slate-800 space-y-1"
                    >
                      <p className="text-[11px] text-slate-300 font-medium truncate">
                        "{item.message}"
                      </p>
                      <div className="flex flex-wrap gap-1.5">
                        <span className="text-[9px] bg-purple-500/15 text-purple-300 px-2 py-0.5 rounded-full font-bold">
                          {item.result.intent}
                        </span>
                        <span className="text-[9px] bg-slate-700/50 text-slate-400 px-2 py-0.5 rounded-full">
                          %{(item.result.confidence * 100).toFixed(0)}
                        </span>
                        <span
                          className={`text-[9px] px-2 py-0.5 rounded-full ${
                            item.result.sentiment === "positive"
                              ? "bg-emerald-500/15 text-emerald-300"
                              : item.result.sentiment === "negative"
                              ? "bg-rose-500/15 text-rose-300"
                              : "bg-slate-700/50 text-slate-400"
                          }`}
                        >
                          {item.result.sentiment}
                        </span>
                      </div>
                      <p className="text-[9px] text-slate-600">{item.time}</p>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}

        {/* =================== REFLECTION TAB =================== */}
        {activeTab === "reflection" && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="bg-slate-900/60 rounded-[2rem] border border-slate-800 p-6 space-y-4">
              <h3 className="text-white font-black text-sm uppercase tracking-widest flex items-center gap-2">
                <span className="w-2 h-2 bg-amber-500 rounded-full"></span>
                Yansıma Metni Doğrulama
              </h3>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-[10px] text-slate-500 font-bold uppercase tracking-widest mb-1 block">
                    Kategori
                  </label>
                  <select
                    value={reflectionCategory}
                    onChange={(e) => setReflectionCategory(e.target.value)}
                    className="w-full bg-slate-950/50 border border-slate-800 rounded-xl p-3 text-white text-sm outline-none focus:ring-2 focus:ring-amber-500/50"
                  >
                    <option value="spor">Spor</option>
                    <option value="eğitim">Eğitim</option>
                    <option value="sağlık">Sağlık</option>
                    <option value="kariyer">Kariyer</option>
                    <option value="sosyal">Sosyal</option>
                  </select>
                </div>
                <div>
                  <label className="text-[10px] text-slate-500 font-bold uppercase tracking-widest mb-1 block">
                    Min Kelime
                  </label>
                  <input
                    type="number"
                    value={reflectionMinWords}
                    onChange={(e) => setReflectionMinWords(Number(e.target.value))}
                    className="w-full bg-slate-950/50 border border-slate-800 rounded-xl p-3 text-white text-sm outline-none focus:ring-2 focus:ring-amber-500/50"
                    min={1}
                  />
                </div>
              </div>

              <textarea
                value={reflectionText}
                onChange={(e) => setReflectionText(e.target.value)}
                placeholder="Görev tamamlama yansıma metnini buraya yazın..."
                className="w-full h-40 bg-slate-950/50 border border-slate-800 rounded-xl p-4 text-white text-sm outline-none focus:ring-2 focus:ring-amber-500/50 resize-none placeholder:text-slate-600"
              />

              {/* Quick test buttons */}
              <div className="flex flex-wrap gap-2">
                <button
                  onClick={() =>
                    setReflectionText(
                      "Bugün parkta 5 kilometre koştum. Hava çok güzeldi ve kendimi çok enerjik hissettim. Tempo olarak da kendimi zorlayabildim, son 1 km'de hızımı artırdım."
                    )
                  }
                  className="text-[10px] bg-emerald-500/10 border border-emerald-500/30 text-emerald-300 px-3 py-1 rounded-lg"
                >
                  ✅ Geçerli Metin
                </button>
                <button
                  onClick={() =>
                    setReflectionText("asdf asdf asdf asdf asdf asdf asdf asdf asdf asdf asdf")
                  }
                  className="text-[10px] bg-rose-500/10 border border-rose-500/30 text-rose-300 px-3 py-1 rounded-lg"
                >
                  ❌ Spam Metin
                </button>
                <button
                  onClick={() => setReflectionText("yaptım tamam")}
                  className="text-[10px] bg-amber-500/10 border border-amber-500/30 text-amber-300 px-3 py-1 rounded-lg"
                >
                  ⚠️ Kısa Metin
                </button>
              </div>

              <button
                onClick={handleReflection}
                disabled={reflectionLoading || !reflectionText.trim()}
                className="w-full bg-amber-600 hover:bg-amber-500 disabled:bg-slate-700 text-white py-3 rounded-xl font-black text-sm uppercase tracking-widest transition-all active:scale-[0.98]"
              >
                {reflectionLoading ? "Analiz Ediliyor..." : "METNİ DOĞRULA"}
              </button>
            </div>

            {/* Result */}
            <div className="bg-slate-900/60 rounded-[2rem] border border-slate-800 p-6">
              <h3 className="text-white font-black text-sm uppercase tracking-widest mb-4 flex items-center gap-2">
                <span className="w-2 h-2 bg-amber-500 rounded-full"></span>
                Sonuç
              </h3>
              {reflectionResult && !reflectionResult.error ? (
                <div className="space-y-4">
                  {/* Status */}
                  <div
                    className={`p-4 rounded-xl border text-center ${
                      reflectionResult.is_valid
                        ? "bg-emerald-500/10 border-emerald-500/30 text-emerald-300"
                        : "bg-rose-500/10 border-rose-500/30 text-rose-300"
                    }`}
                  >
                    <p className="text-3xl mb-1">
                      {reflectionResult.is_valid ? "✅" : "❌"}
                    </p>
                    <p className="font-bold text-sm">
                      {reflectionResult.is_valid ? "DOĞRULANDI" : "REDDEDİLDİ"}
                    </p>
                  </div>

                  {/* Metrics */}
                  <div className="grid grid-cols-2 gap-3">
                    <div className="bg-slate-950/50 rounded-xl p-4 border border-slate-800 text-center">
                      <p className="text-[10px] text-slate-500 font-bold uppercase">Kelime Sayısı</p>
                      <p className="text-2xl font-black text-white">{reflectionResult.word_count}</p>
                    </div>
                    <div className="bg-slate-950/50 rounded-xl p-4 border border-slate-800 text-center">
                      <p className="text-[10px] text-slate-500 font-bold uppercase">Kalite Skoru</p>
                      <p className="text-2xl font-black text-white">
                        %{(reflectionResult.quality_score * 100).toFixed(0)}
                      </p>
                    </div>
                    <div className="bg-slate-950/50 rounded-xl p-4 border border-slate-800 text-center">
                      <p className="text-[10px] text-slate-500 font-bold uppercase">İlgililik</p>
                      <p className="text-2xl font-black text-white">
                        %{(reflectionResult.relevance_score * 100).toFixed(0)}
                      </p>
                    </div>
                    <div className="bg-slate-950/50 rounded-xl p-4 border border-slate-800 text-center">
                      <p className="text-[10px] text-slate-500 font-bold uppercase">Spam</p>
                      <p className={`text-2xl font-black ${reflectionResult.spam_detected ? "text-rose-400" : "text-emerald-400"}`}>
                        {reflectionResult.spam_detected ? "EVET" : "HAYIR"}
                      </p>
                    </div>
                  </div>

                  {/* Issues */}
                  {reflectionResult.issues?.length > 0 && (
                    <div className="bg-rose-500/5 border border-rose-500/20 rounded-xl p-4">
                      <p className="text-[10px] text-rose-400 font-bold uppercase mb-2">Sorunlar:</p>
                      {reflectionResult.issues.map((issue, i) => (
                        <p key={i} className="text-rose-300 text-xs mb-1">
                          • {issue}
                        </p>
                      ))}
                    </div>
                  )}
                </div>
              ) : reflectionResult?.error ? (
                <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-4 text-rose-300 text-sm">
                  ⚠️ {reflectionResult.error}
                </div>
              ) : (
                <div className="text-center py-12 text-slate-600">
                  <p className="text-4xl mb-3">📝</p>
                  <p className="text-sm">Doğrulama sonuçları burada görünecek</p>
                </div>
              )}
            </div>
          </div>
        )}

        {/* ==================== IMAGE TAB ==================== */}
        {activeTab === "image" && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="bg-slate-900/60 rounded-[2rem] border border-slate-800 p-6 space-y-4">
              <h3 className="text-white font-black text-sm uppercase tracking-widest flex items-center gap-2">
                <span className="w-2 h-2 bg-cyan-500 rounded-full"></span>
                Görüntü Doğrulama
              </h3>

              <div>
                <label className="text-[10px] text-slate-500 font-bold uppercase tracking-widest mb-1 block">
                  Beklenen Kategori
                </label>
                <select
                  value={imageCategory}
                  onChange={(e) => setImageCategory(e.target.value)}
                  className="w-full bg-slate-950/50 border border-slate-800 rounded-xl p-3 text-white text-sm outline-none focus:ring-2 focus:ring-cyan-500/50"
                >
                  <option value="spor">Spor</option>
                  <option value="eğitim">Eğitim</option>
                  <option value="sağlık">Sağlık</option>
                  <option value="kariyer">Kariyer</option>
                  <option value="sosyal">Sosyal</option>
                  <option value="meditasyon">Meditasyon</option>
                </select>
              </div>

              <div
                className="border-2 border-dashed border-slate-700 rounded-2xl p-8 text-center cursor-pointer hover:border-cyan-500/50 transition-colors relative"
                onClick={() => document.getElementById("image-upload").click()}
              >
                <input
                  id="image-upload"
                  type="file"
                  accept="image/*"
                  onChange={handleImageUpload}
                  className="hidden"
                />
                {imagePreview ? (
                  <img
                    src={imagePreview}
                    alt="Yüklenen görüntü"
                    className="max-h-48 mx-auto rounded-xl object-contain"
                  />
                ) : (
                  <>
                    <p className="text-4xl mb-2">📸</p>
                    <p className="text-slate-500 text-sm">
                      Fotoğraf yüklemek için tıklayın
                    </p>
                    <p className="text-slate-600 text-xs mt-1">JPG, PNG, WebP</p>
                  </>
                )}
              </div>

              <button
                onClick={handleImageVerify}
                disabled={imageLoading || !imagePreview}
                className="w-full bg-cyan-600 hover:bg-cyan-500 disabled:bg-slate-700 text-white py-3 rounded-xl font-black text-sm uppercase tracking-widest transition-all active:scale-[0.98]"
              >
                {imageLoading ? "Analiz Ediliyor..." : "GÖRÜNTÜYÜ DOĞRULA"}
              </button>
            </div>

            {/* Image Result */}
            <div className="bg-slate-900/60 rounded-[2rem] border border-slate-800 p-6">
              <h3 className="text-white font-black text-sm uppercase tracking-widest mb-4 flex items-center gap-2">
                <span className="w-2 h-2 bg-cyan-500 rounded-full"></span>
                Sonuç
              </h3>
              {imageResult && !imageResult.error ? (
                <div className="space-y-4">
                  <div
                    className={`p-4 rounded-xl border text-center ${
                      imageResult.is_valid
                        ? "bg-emerald-500/10 border-emerald-500/30 text-emerald-300"
                        : "bg-rose-500/10 border-rose-500/30 text-rose-300"
                    }`}
                  >
                    <p className="text-3xl mb-1">
                      {imageResult.is_valid ? "✅" : "❌"}
                    </p>
                    <p className="font-bold text-sm">
                      {imageResult.is_valid ? "EŞLEŞME BULUNDU" : "EŞLEŞME BULUNAMADI"}
                    </p>
                    <p className="text-xs mt-1 opacity-75">{imageResult.message}</p>
                  </div>

                  <div className="bg-slate-950/50 rounded-xl p-4 border border-slate-800 text-center">
                    <p className="text-[10px] text-slate-500 font-bold uppercase">Güven Skoru</p>
                    <p className="text-2xl font-black text-white">
                      %{(imageResult.confidence * 100).toFixed(1)}
                    </p>
                  </div>

                  {imageResult.detected_labels?.length > 0 && (
                    <div>
                      <p className="text-[10px] text-slate-500 font-bold uppercase mb-2">
                        Tespit Edilen Etiketler (Top-10):
                      </p>
                      <div className="space-y-1.5">
                        {imageResult.detected_labels.slice(0, 10).map((label, i) => (
                          <div
                            key={i}
                            className="flex items-center gap-3 text-xs"
                          >
                            <span className="text-slate-400 w-4 text-right">{i + 1}.</span>
                            <span className="text-white font-medium flex-1">{label.label}</span>
                            <div className="w-24 h-2 bg-slate-800 rounded-full overflow-hidden">
                              <div
                                className="h-full bg-cyan-500 rounded-full"
                                style={{ width: `${label.confidence * 100}%` }}
                              ></div>
                            </div>
                            <span className="text-slate-500 w-14 text-right">
                              %{(label.confidence * 100).toFixed(1)}
                            </span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              ) : imageResult?.error ? (
                <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-4 text-rose-300 text-sm">
                  ⚠️ {imageResult.error}
                </div>
              ) : (
                <div className="text-center py-12 text-slate-600">
                  <p className="text-4xl mb-3">📸</p>
                  <p className="text-sm">Görüntü doğrulama sonuçları burada görünecek</p>
                </div>
              )}
            </div>
          </div>
        )}

        {/* ==================== TRAIN TAB ==================== */}
        {activeTab === "train" && (
          <div className="max-w-xl mx-auto bg-slate-900/60 rounded-[2rem] border border-slate-800 p-8 space-y-6">
            <h3 className="text-white font-black text-sm uppercase tracking-widest flex items-center gap-2">
              <span className="w-2 h-2 bg-rose-500 rounded-full"></span>
              Model Yeniden Eğitimi
            </h3>

            <div className="bg-amber-500/5 border border-amber-500/20 rounded-xl p-4 text-amber-300 text-xs">
              ⚠️ Bu işlem mevcut modelleri yeniden eğitir. Eğitim verilerinde değişiklik
              yaptıysanız bu butona basın.
            </div>

            <button
              onClick={handleTrain}
              disabled={trainLoading}
              className="w-full bg-rose-600 hover:bg-rose-500 disabled:bg-slate-700 text-white py-4 rounded-xl font-black text-sm uppercase tracking-widest transition-all active:scale-[0.98] shadow-lg shadow-rose-900/20"
            >
              {trainLoading ? "Eğitiliyor..." : "MODELLERİ YENİDEN EĞİT"}
            </button>

            {trainResult && !trainResult.error && (
              <div className="bg-emerald-500/10 border border-emerald-500/30 rounded-xl p-4 space-y-3">
                <p className="text-emerald-300 font-bold text-sm">✅ Modeller başarıyla eğitildi!</p>
                <div className="grid grid-cols-2 gap-3">
                  <div className="bg-slate-950/50 rounded-lg p-3 border border-slate-800">
                    <p className="text-[10px] text-slate-500 font-bold uppercase">Intent Model</p>
                    <p className="text-white text-sm font-bold mt-1">
                      {trainResult.intent_result?.samples} örnek
                    </p>
                    <p className="text-slate-400 text-[10px]">
                      {trainResult.intent_result?.classes?.length} sınıf
                    </p>
                  </div>
                  <div className="bg-slate-950/50 rounded-lg p-3 border border-slate-800">
                    <p className="text-[10px] text-slate-500 font-bold uppercase">Sentiment Model</p>
                    <p className="text-white text-sm font-bold mt-1">
                      {trainResult.sentiment_result?.samples} örnek
                    </p>
                    <p className="text-slate-400 text-[10px]">
                      {trainResult.sentiment_result?.classes?.length} sınıf
                    </p>
                  </div>
                </div>
              </div>
            )}
            {trainResult?.error && (
              <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-4 text-rose-300 text-sm">
                ⚠️ {trainResult.error}
              </div>
            )}
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export default AITestPage;
