import React, { useState, useEffect } from "react";
import MainLayout from "../layouts/MainLayout";
import categoryService from "../services/categoryService";
import taskService from "../services/taskService";
import badgeService from "../services/badgeService";
import axiosClient from "../api/axiosClient";

const sampleJson = {
  categories: [
    { "name": "Eğitim", "description": "Akademik ve kişisel gelişim", "iconPath": "📚" },
    { "name": "Sağlık", "description": "Fiziksel ve zihinsel sağlık", "iconPath": "💪" },
    { "name": "Kariyer", "description": "İş hayatı ve profesyonel hedefler", "iconPath": "💼" },
    { "name": "Sosyal Sorumluluk", "description": "Topluma fayda ve gönüllülük", "iconPath": "🤝" }
  ],
  tasks: [
    { "title": "Kitap Oku", "description": "En az 20 sayfa kitap oku. Sevdiğin bir konudan başlayabilirsin.", "categoryName": "Eğitim", "baseXP": 50, "minLevel": 1, "maxLevel": 99, "difficulty": 0, "isDaily": true, "estimatedMinutes": 20, "whyThisTask": "Kişisel gelişimin için önemli." },
    { "title": "Su İç", "description": "Günde 2 litre su hedefini tamamla. Sağlıklı bir vücut için su şart!", "categoryName": "Sağlık", "baseXP": 20, "minLevel": 1, "maxLevel": 99, "difficulty": 0, "isDaily": true, "estimatedMinutes": 10, "whyThisTask": "Vücudun su dengesini korur." },
    { "title": "LinkedIn Güncelle", "description": "LinkedIn profilini veya portföyünü güncelle.", "categoryName": "Kariyer", "baseXP": 80, "minLevel": 1, "maxLevel": 99, "difficulty": 1, "isDaily": false, "estimatedMinutes": 30, "whyThisTask": "Yeni fırsatlar için önemli." }
  ],
  badges: [
    { "name": "Yeni Başlayan", "description": "İlk adımı attın! (Level 1)", "iconPath": "🌱", "requiredLevel": 1 },
    { "name": "Efsane", "description": "Efsanevi seviyeye ulaştın! (Level 20)", "iconPath": "👑", "requiredLevel": 20 }
  ],
  achievements: [
    { "title": "İlk Adım", "description": "İlk görevini tamamla!", "iconPath": "🎯", "type": 0, "requiredCount": 1, "bonusXP": 50, "isHidden": false }
  ]
};

const AdminPage = () => {
  const [activeTab, setActiveTab] = useState("triggers");
  const [categories, setCategories] = useState([]);
  const [jsonInput, setJsonInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [wipeDatabase, setWipeDatabase] = useState(false);

  const [taskData, setTaskData] = useState({
    title: "",
    description: "",
    categoryId: "",
    baseXP: 100,
    minLevel: 1,
    difficulty: 0,
  });
  const [catData, setCatData] = useState({
    name: "",
    description: "",
    iconPath: "🔹",
  });
  const [badgeData, setBadgeData] = useState({
    name: "",
    description: "",
    iconPath: "🏆",
    requiredLevel: 1,
    categoryId: "",
  });

  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    try {
      const data = await categoryService.getAllCategories();
      const catList = Array.isArray(data) ? data : data?.categories || [];
      setCategories(catList);
    } catch (err) {
      console.error(err);
    }
  };

  const handleSingleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (activeTab === "tasks") await taskService.createTask(taskData);
      else if (activeTab === "categories")
        await categoryService.createCategory(catData);
      else if (activeTab === "badges") {
        if (!badgeData.categoryId) {
          alert("Lütfen rozet için bir kategori seçin!");
          setLoading(false);
          return;
        }
        await badgeService.createBadge(badgeData);
      }

      alert("✅ Başarıyla eklendi!");
      if (activeTab === "categories") loadCategories();
    } catch (err) {
      alert("Hata: " + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const handleBulkSubmit = async () => {
    if (!jsonInput) return;
    setLoading(true);
    try {
      const parsed = JSON.parse(jsonInput);
      if (activeTab === "tasks") await taskService.createBulkTasks(parsed);
      else if (activeTab === "categories")
        await categoryService.createBulkCategories(parsed);
      else if (activeTab === "badges")
        await badgeService.createBulkBadges(parsed);

      alert("🚀 Toplu yükleme başarılı!");
      setJsonInput("");
      if (activeTab === "categories") loadCategories();
    } catch (err) {
      alert("JSON hatası veya API reddetti: " + err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleWipeAndSeed = async () => {
    if (!jsonInput) return;
    if (wipeDatabase && !window.confirm("⚠️ DİKKAT! Bu işlem veritabanındaki tüm görev, kategori, rozet ve başarımları SİLECEK ve yeni verileri yükleyecek. Devam etmek istiyor musunuz?")) return;
    
    setLoading(true);
    try {
      const parsed = JSON.parse(jsonInput);
      parsed.wipeDatabase = wipeDatabase;
      const res = await axiosClient.post("/admin/wipe-and-seed", parsed);
      alert("🎉 " + (res.data?.message || "Veriler başarıyla işlendi!"));
      setJsonInput("");
      loadCategories();
    } catch (err) {
      alert("Hata: " + (err.response?.data?.message || err.message));
    } finally {
      setLoading(false);
    }
  };

  const inputClass =
    "w-full bg-slate-950 border border-slate-800 rounded-xl p-4 text-white outline-none focus:border-indigo-500 transition-colors placeholder:text-slate-600";
  const labelClass =
    "text-[10px] font-black text-slate-500 uppercase tracking-widest ml-1 mb-2 block";

  return (
    <MainLayout>
      <div className="max-w-6xl mx-auto space-y-10">
        <section className="flex flex-col md:flex-row md:items-center justify-between gap-6 border-b border-slate-800/80 pb-8 relative overflow-hidden">
          <div>
            <h2 className="text-4xl font-black text-white italic tracking-tighter uppercase flex items-center gap-3">
              🚀 GELİŞTİRİCİ TEST PANELİ
            </h2>
            <p className="text-slate-400 text-xs mt-2 max-w-xl">
              Sistem testlerini ve arka plan görevlerini tek tıkla yürütün, verileri anında yönetin.
            </p>
          </div>
          <div className="flex flex-wrap gap-2 p-1.5 bg-slate-900/60 border border-slate-800/80 rounded-2xl backdrop-blur-xl w-fit h-fit shadow-inner">
            {[
              { id: "triggers", label: "⚙️ Tetikleyiciler" },
              { id: "json-seed", label: "🛠️ JSON Sıfırla & Doldur" },
              { id: "categories", label: "📂 Kategoriler" },
              { id: "tasks", label: "⚔️ Görevler" },
              { id: "badges", label: "🏆 Rozetler" },
            ].map((tab) => (
              <button
                key={tab.id}
                onClick={() => {
                  setActiveTab(tab.id);
                  if (tab.id === "json-seed") {
                    setJsonInput(JSON.stringify(sampleJson, null, 2));
                  } else {
                    setJsonInput("");
                  }
                }}
                className={`px-5 py-3 rounded-xl font-black uppercase text-[10px] tracking-widest transition-all duration-300 active:scale-[0.98] ${
                  activeTab === tab.id
                    ? "bg-indigo-600 text-white shadow-[0_0_20px_rgba(79,70,229,0.3)]"
                    : "text-slate-400 hover:text-slate-200 hover:bg-slate-800/50"
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>
        </section>

        {activeTab === "triggers" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="bg-slate-900/40 border border-slate-800 hover:border-slate-700/80 rounded-[2.5rem] p-10 flex flex-col justify-between gap-6 relative overflow-hidden group transition-all duration-300">
              <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-500/10 blur-[70px] rounded-full"></div>
              <div className="space-y-3 relative z-10">
                <div className="flex items-center gap-3">
                  <span className="text-3xl">📊</span>
                  <h3 className="text-white font-black italic uppercase text-xl tracking-widest">
                    Haftalık Rapor Üret
                  </h3>
                </div>
                <p className="text-slate-400 text-xs leading-relaxed">
                  Pazar günü saat 23:00 kuralını beklemeden, sistemdeki tüm aktif kullanıcılara hemen haftalık rapor hesaplayıp kaydeder.
                </p>
              </div>
              <button
                onClick={async () => {
                  try {
                    setLoading(true);
                    const res = await axiosClient.post("/reports/trigger-weekly-report");
                    alert(res.data?.message || "Haftalık raporlar başarıyla üretildi! 🎉");
                  } catch (err) {
                    alert("Hata: " + (err.response?.data?.message || err.message));
                  } finally {
                    setLoading(false);
                  }
                }}
                disabled={loading}
                className="w-full bg-indigo-600 hover:bg-indigo-500 text-white py-4 rounded-xl font-bold uppercase tracking-wider text-xs transition-all duration-300 active:scale-[0.98] shadow-lg shadow-indigo-600/20"
              >
                {loading ? "İşleniyor..." : "Rapor Tetikle"}
              </button>
            </div>

            <div className="bg-slate-900/40 border border-slate-800 hover:border-slate-700/80 rounded-[2.5rem] p-10 flex flex-col justify-between gap-6 relative overflow-hidden group transition-all duration-300">
              <div className="absolute top-0 right-0 w-32 h-32 bg-emerald-500/10 blur-[70px] rounded-full"></div>
              <div className="space-y-3 relative z-10">
                <div className="flex items-center gap-3">
                  <span className="text-3xl">🎯</span>
                  <h3 className="text-white font-black italic uppercase text-xl tracking-widest">
                    Görev Dağıtımı Yap
                  </h3>
                </div>
                <p className="text-slate-400 text-xs leading-relaxed">
                  Gece yarısını beklemeden o gün için henüz görev almamış tüm kullanıcılara akıllı öneri sistemi üzerinden yeni 6'şar görev atar.
                </p>
              </div>
              <button
                onClick={async () => {
                  try {
                    setLoading(true);
                    const res = await axiosClient.post("/user-tasks/trigger-daily-quests");
                    alert(res.data?.message || "Günlük görevler başarıyla dağıtıldı! 🎉");
                  } catch (err) {
                    alert("Hata: " + (err.response?.data?.message || err.message));
                  } finally {
                    setLoading(false);
                  }
                }}
                disabled={loading}
                className="w-full bg-emerald-600 hover:bg-emerald-500 text-white py-4 rounded-xl font-bold uppercase tracking-wider text-xs transition-all duration-300 active:scale-[0.98] shadow-lg shadow-emerald-600/20"
              >
                {loading ? "İşleniyor..." : "Görevleri Dağıt"}
              </button>
            </div>
          </div>
        )}

        {activeTab === "json-seed" && (
          <div className="bg-slate-900/40 p-10 rounded-[3rem] border border-slate-800 flex flex-col justify-between relative overflow-hidden min-h-[600px]">
            <div className="absolute top-0 left-0 w-32 h-32 bg-indigo-500/10 blur-[60px] rounded-full"></div>
            <div className="flex justify-between items-center mb-8 border-b border-slate-800 pb-4 relative z-10">
              <div>
                <h3 className="text-white font-black italic uppercase text-lg tracking-widest">
                  🛠️ Tüm Verileri JSON ile Sıfırla & Doldur
                </h3>
                <p className="text-slate-400 text-xs mt-1">
                  Aşağıdaki JSON şablonunu dilediğiniz gibi düzenleyin. Gönderdiğinizde veritabanını isteğinize göre temizleyebilir veya mevcut verileri koruyarak üstüne ekleyebilirsiniz.
                </p>
              </div>
              <span className="text-[10px] bg-slate-800 text-slate-400 px-2 py-1 rounded border border-slate-700 font-mono">
                Full Configuration Format
              </span>
            </div>

            <textarea
              value={jsonInput}
              onChange={(e) => setJsonInput(e.target.value)}
              className="flex-1 min-h-[350px] bg-slate-950/80 border border-slate-800 rounded-3xl p-6 text-emerald-400 font-mono text-xs outline-none focus:border-indigo-500 transition-colors relative z-10 resize-none mb-6"
              placeholder={`{\n  "categories": [...],\n  "tasks": [...],\n  "badges": [...],\n  "achievements": [...]\n}`}
            />

            <div className="flex items-center gap-3 mb-6 relative z-10 bg-slate-950/60 p-4 border border-slate-800/80 rounded-2xl w-fit">
              <input
                type="checkbox"
                id="wipeDbCheck"
                checked={wipeDatabase}
                onChange={(e) => setWipeDatabase(e.target.checked)}
                className="w-5 h-5 accent-rose-600 rounded cursor-pointer"
              />
              <label htmlFor="wipeDbCheck" className="text-xs text-slate-300 font-bold select-none cursor-pointer">
                ⚠️ Veritabanını tamamen temizle (Wipe Database)
              </label>
            </div>

            <button
              disabled={loading}
              onClick={handleWipeAndSeed}
              className={`w-full font-black py-4 rounded-2xl text-xs uppercase tracking-widest shadow-lg transition-colors relative z-10 active:scale-[0.98] ${
                wipeDatabase ? "bg-rose-600 hover:bg-rose-500 text-white" : "bg-indigo-600 hover:bg-indigo-500 text-white"
              }`}
            >
              {loading ? "İŞLENİYOR..." : wipeDatabase ? "🚀 VERİTABANINI SIFIRLA VE JSON İLE DOLDUR" : "➕ MEVCUT VERİLERE JSON EKLE"}
            </button>
          </div>
        )}

        {activeTab !== "triggers" && activeTab !== "json-seed" && (
          <div className="grid grid-cols-1 xl:grid-cols-2 gap-10">
            <div className="bg-slate-900/40 p-10 rounded-[3rem] border border-slate-800 relative overflow-hidden flex flex-col justify-between">
              <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-500/10 blur-[60px] rounded-full"></div>
              <h3 className="text-white font-black italic mb-8 uppercase text-lg tracking-widest border-b border-slate-800 pb-4">
                Tekli{" "}
                {activeTab === "tasks"
                  ? "Görev"
                  : activeTab === "categories"
                  ? "Kategori"
                  : "Rozet"}{" "}
                Ekle
              </h3>

              <form onSubmit={handleSingleSubmit} className="space-y-6 relative z-10 flex-1 flex flex-col justify-between">
                {activeTab === "tasks" && (
                  <div className="space-y-4">
                    <div>
                      <label className={labelClass}>Görev Başlığı</label>
                      <input
                        type="text"
                        placeholder="Örn: 5KM Koşu"
                        className={inputClass}
                        onChange={(e) =>
                          setTaskData({ ...taskData, title: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div>
                      <label className={labelClass}>Açıklama</label>
                      <textarea
                        rows="2"
                        placeholder="Detaylar..."
                        className={inputClass}
                        onChange={(e) =>
                          setTaskData({
                            ...taskData,
                            description: e.target.value,
                          })
                        }
                      />
                    </div>
                    <div>
                      <label className={labelClass}>Kategori</label>
                      <select
                        className={inputClass}
                        onChange={(e) =>
                          setTaskData({ ...taskData, categoryId: e.target.value })
                        }
                        required
                      >
                        <option value="">Seçiniz...</option>
                        {categories.map((c) => (
                          <option key={c.id} value={c.id}>
                            {c.name}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className={labelClass}>XP Ödülü</label>
                        <input
                          type="number"
                          placeholder="100"
                          className={inputClass}
                          onChange={(e) =>
                            setTaskData({
                              ...taskData,
                              baseXP: parseInt(e.target.value),
                            })
                          }
                        />
                      </div>
                      <div>
                        <label className={labelClass}>Min. Level</label>
                        <input
                          type="number"
                          placeholder="1"
                          className={inputClass}
                          onChange={(e) =>
                            setTaskData({
                              ...taskData,
                              minLevel: parseInt(e.target.value),
                            })
                          }
                        />
                      </div>
                    </div>
                    <div>
                      <label className={labelClass}>Zorluk Seviyesi</label>
                      <select
                        className={inputClass}
                        onChange={(e) =>
                          setTaskData({
                            ...taskData,
                            difficulty: parseInt(e.target.value),
                          })
                        }
                      >
                        <option value="0">Kolay (Easy)</option>
                        <option value="1">Orta (Medium)</option>
                        <option value="2">Zor (Hard)</option>
                      </select>
                    </div>
                  </div>
                )}

                {activeTab === "categories" && (
                  <div className="space-y-4">
                    <div>
                      <label className={labelClass}>Kategori Adı</label>
                      <input
                        type="text"
                        placeholder="Örn: Yazılım"
                        className={inputClass}
                        onChange={(e) =>
                          setCatData({ ...catData, name: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div>
                      <label className={labelClass}>Açıklama</label>
                      <input
                        type="text"
                        placeholder="Kategori açıklaması"
                        className={inputClass}
                        onChange={(e) =>
                          setCatData({ ...catData, description: e.target.value })
                        }
                      />
                    </div>
                    <div>
                      <label className={labelClass}>İkon (Emoji)</label>
                      <input
                        type="text"
                        placeholder="💻"
                        className={inputClass}
                        onChange={(e) =>
                          setCatData({ ...catData, iconPath: e.target.value })
                        }
                      />
                    </div>
                  </div>
                )}

                {activeTab === "badges" && (
                  <div className="space-y-4">
                    <div>
                      <label className={labelClass}>Rozet Adı</label>
                      <input
                        type="text"
                        placeholder="Örn: Maratoncu"
                        className={inputClass}
                        onChange={(e) =>
                          setBadgeData({ ...badgeData, name: e.target.value })
                        }
                        required
                      />
                    </div>
                    <div>
                      <label className={labelClass}>Açıklama</label>
                      <input
                        type="text"
                        placeholder="Nasıl kazanılır?"
                        className={inputClass}
                        onChange={(e) =>
                          setBadgeData({
                            ...badgeData,
                            description: e.target.value,
                          })
                        }
                      />
                    </div>

                    <div>
                      <label className={labelClass}>İlgili Kategori</label>
                      <select
                        className={inputClass}
                        onChange={(e) =>
                          setBadgeData({
                            ...badgeData,
                            categoryId: e.target.value,
                          })
                        }
                        required
                      >
                        <option value="">Seçiniz...</option>
                        {categories.map((c) => (
                          <option key={c.id} value={c.id}>
                            {c.name}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className={labelClass}>İkon</label>
                        <input
                          type="text"
                          placeholder="🥇"
                          className={inputClass}
                          onChange={(e) =>
                            setBadgeData({
                              ...badgeData,
                              iconPath: e.target.value,
                            })
                          }
                        />
                      </div>
                      <div>
                        <label className={labelClass}>Gerekli Level</label>
                        <input
                          type="number"
                          placeholder="5"
                          className={inputClass}
                          onChange={(e) =>
                            setBadgeData({
                              ...badgeData,
                              requiredLevel: parseInt(e.target.value),
                            })
                          }
                        />
                      </div>
                    </div>
                  </div>
                )}

                <button
                  disabled={loading}
                  type="submit"
                  className="w-full bg-emerald-600 hover:bg-emerald-500 text-white py-4 rounded-xl font-bold transition-all shadow-lg shadow-emerald-900/20 active:scale-[0.98] uppercase tracking-widest mt-4"
                >
                  {loading ? "İşleniyor..." : "Veritabanına Kaydet"}
                </button>
              </form>
            </div>

            <div className="bg-slate-900/40 p-10 rounded-[3rem] border border-slate-800 flex flex-col justify-between relative overflow-hidden">
              <div className="absolute top-0 left-0 w-32 h-32 bg-rose-500/10 blur-[60px] rounded-full"></div>
              <div className="flex justify-between items-center mb-8 border-b border-slate-800 pb-4 relative z-10">
                <h3 className="text-white font-black italic uppercase text-lg tracking-widest">
                  Toplu{" "}
                  {activeTab === "tasks"
                    ? "Görev"
                    : activeTab === "categories"
                    ? "Kategori"
                    : "Rozet"}{" "}
                  Yükle
                </h3>
                <span className="text-[10px] bg-slate-800 text-slate-400 px-2 py-1 rounded border border-slate-700 font-mono">
                  JSON Format
                </span>
              </div>

              <textarea
                value={jsonInput}
                onChange={(e) => setJsonInput(e.target.value)}
                className="flex-1 min-h-[400px] bg-slate-950/80 border border-slate-800 rounded-3xl p-6 text-emerald-400 font-mono text-xs outline-none focus:border-rose-500 transition-colors relative z-10 resize-none"
                placeholder={`[\n  { "name": "Örnek Veri", ... }\n]`}
              />

              <button
                disabled={loading}
                onClick={handleBulkSubmit}
                className="mt-6 w-full bg-white hover:bg-slate-200 text-slate-950 font-black py-4 rounded-2xl text-xs uppercase tracking-widest shadow-lg transition-colors relative z-10 active:scale-[0.98]"
              >
                {loading
                  ? "YÜKLENİYOR..."
                  : `TOPLU ${activeTab.toUpperCase()} GÖNDER`}
              </button>
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export default AdminPage;