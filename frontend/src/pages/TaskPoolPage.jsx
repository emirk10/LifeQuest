import React, { useState, useEffect, useMemo, useCallback } from "react";
import MainLayout from "../layouts/MainLayout";
import axiosClient from "../api/axiosClient";
import DailyMoodSelector from "../components/DailyMoodSelector";
import TaskPoolSelector from "../components/TaskPoolSelector";
import userService from "../services/userService";
import { moodApi, taskPoolApi } from "../api/moodApi";
import "../styles/DailyTaskPage.css";

const TaskPoolPage = () => {
  const [activeTab, setActiveTab] = useState("recommended"); // "recommended" | "all"
  const [phase, setPhase] = useState("loading"); // loading | mood | pool
  const [poolData, setPoolData] = useState(null);
  const [poolLoading, setPoolLoading] = useState(false);
  const [availableTasks, setAvailableTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(null);
  const [toast, setToast] = useState(null);
  const [userLevel, setUserLevel] = useState(1);

  // Filter & Pagination States for ALL Tasks
  const [selectedCategory, setSelectedCategory] = useState("all");
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 6;

  const showToast = (message, type = "success") => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  useEffect(() => {
    checkTodayMood();
    fetchAvailableTasks();
    const fetchUserLevel = async () => {
      try {
        const freshData = await userService.getProfile();
        setUserLevel(freshData.generalLevel ?? freshData.GeneralLevel ?? 1);
      } catch (err) {
        console.error(err);
      }
    };
    fetchUserLevel();
  }, []);

  const checkTodayMood = async () => {
    try {
      const res = await moodApi.getTodayMood();
      if (res.data?.data || res.data) {
        // Bugün mood girilmiş — direkt pool'a geç
        await loadPool();
        setPhase("pool");
      } else {
        setPhase("mood");
      }
    } catch {
      setPhase("mood");
    }
  };

  const handleMoodRecorded = async () => {
    await loadPool();
    setPhase("pool");
  };

  const loadPool = async () => {
    setPoolLoading(true);
    try {
      const res = await taskPoolApi.getDailyPool(6);
      setPoolData(res.data?.data || res.data || null);
    } catch (err) {
      console.error("Pool yüklenemedi:", err);
    } finally {
      setPoolLoading(false);
    }
  };

  const fetchAvailableTasks = async () => {
    setLoading(true);
    try {
      const availRes = await axiosClient.get("/user-tasks/available-tasks");
      if (availRes.data) {
        setAvailableTasks(availRes.data);
      } else {
        setAvailableTasks([]);
      }
    } catch (err) {
      console.error(err);
      showToast("Görevler yüklenirken bir sorun oluştu.", "error");
    } finally {
      setLoading(false);
    }
  };

  const handleAssignAppTask = async (appTaskId) => {
    setActionLoading(appTaskId);
    try {
      const res = await axiosClient.post(`/user-tasks/assign/${appTaskId}`);
      if (res.data && res.data.success) {
        showToast("🎉 Görev doğrudan listene eklendi!", "success");
        setAvailableTasks((prev) => prev.filter((t) => t.id !== appTaskId));
        loadPool();
        fetchAvailableTasks();
      } else {
        showToast(res.data?.message || "Görev alınamadı.", "error");
      }
    } catch (err) {
      console.error(err);
      showToast(err.response?.data?.message || "Görevi alırken bir hata oluştu.", "error");
    } finally {
      setActionLoading(null);
    }
  };

  // Get dynamic categories list from available tasks - MEMOIZED
  const uniqueCategories = useMemo(() => {
    return [
      "all",
      ...new Set(availableTasks.map((t) => t.categoryName || "Genel")),
    ];
  }, [availableTasks]);

  // Client-side category filter - MEMOIZED
  const filteredTasks = useMemo(() => {
    return availableTasks.filter((task) =>
      selectedCategory === "all"
        ? true
        : (task.categoryName || "Genel") === selectedCategory
    );
  }, [availableTasks, selectedCategory]);

  // Client-side pagination - MEMOIZED
  const totalPages = useMemo(() => {
    return Math.ceil(filteredTasks.length / pageSize);
  }, [filteredTasks]);

  const startIndex = (currentPage - 1) * pageSize;

  const paginatedTasks = useMemo(() => {
    return filteredTasks.slice(startIndex, startIndex + pageSize);
  }, [filteredTasks, startIndex]);

  const handleCategoryChange = useCallback((category) => {
    setSelectedCategory(category);
    setCurrentPage(1); // Reset to page 1
  }, []);

  const mostCommonCategory = useMemo(() => {
    return uniqueCategories
      .filter((c) => c !== "all")
      .map((catName) => ({
        name: catName,
        count: availableTasks.filter((t) => (t.categoryName || "Genel") === catName).length,
      }))
      .sort((a, b) => b.count - a.count)[0]?.name || "Genel";
  }, [uniqueCategories, availableTasks]);

  return (
    <MainLayout>
      {/* Toast Notification */}
      {toast && (
        <div
          className={`fixed top-6 right-6 z-50 px-6 py-4 rounded-2xl font-bold text-sm shadow-2xl transition-all animate-bounce ${
            toast.type === "success"
              ? "bg-emerald-600 text-white shadow-emerald-500/30"
              : "bg-rose-600 text-white shadow-rose-500/30"
          }`}
        >
          {toast.message}
        </div>
      )}

      <div className="max-w-6xl mx-auto space-y-10">
        {/* Header */}
        <section className="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
          <div>
            <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2">
              GÖREV HAVUZU
            </h2>
            <div className="h-1.5 w-24 bg-gradient-to-r from-fuchsia-600 to-indigo-600 rounded-full shadow-[0_0_20px_rgba(192,38,211,0.5)]"></div>
            <p className="text-slate-500 mt-2 text-sm italic font-medium max-w-lg">
              İster yapay zeka tarafından senin için özenle seçilen önerilere odaklan, ister seviyene uygun olan diğer tüm görevlerden seçerek kendi rotanı çiz.
            </p>
          </div>

          {/* Custom Tabs */}
          <div className="flex bg-slate-900/60 p-1 border border-slate-800 rounded-2xl">
            <button
              onClick={() => setActiveTab("recommended")}
              className={`px-6 py-3 rounded-xl text-sm font-black tracking-wide transition-all ${
                activeTab === "recommended"
                  ? "bg-indigo-600 text-white shadow-lg shadow-indigo-500/30"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              🤖 ÖNERİLENLER
            </button>
            <button
              onClick={() => setActiveTab("all")}
              className={`px-6 py-3 rounded-xl text-sm font-black tracking-wide transition-all ${
                activeTab === "all"
                  ? "bg-fuchsia-600 text-white shadow-lg shadow-fuchsia-500/30"
                  : "text-slate-400 hover:text-slate-200"
              }`}
            >
              📋 TÜM GÖREVLER
            </button>
          </div>
        </section>

        {/* Contents */}
        {activeTab === "recommended" ? (
          /* SECTION 1: RECOMMENDED (FROM /daily) */
          <div className="daily-task-page space-y-6">
            {phase === "loading" && (
              <div className="flex flex-col items-center justify-center p-12">
                <div className="loading-spinner w-8 h-8 border-t-2 border-indigo-500 rounded-full animate-spin mb-4" />
                <p className="text-slate-400">Yükleniyor...</p>
              </div>
            )}

            {phase === "mood" && (
              <DailyMoodSelector onMoodRecorded={handleMoodRecorded} />
            )}

            {phase === "pool" && (
              poolLoading ? (
                <div className="flex flex-col items-center justify-center p-12">
                  <div className="loading-spinner w-8 h-8 border-t-2 border-indigo-500 rounded-full animate-spin mb-4" />
                  <p className="text-slate-400">Görevler hazırlanıyor...</p>
                </div>
              ) : (
                <div className="space-y-4">
                  <div className="flex justify-between items-center bg-slate-900/40 p-4 border border-slate-800 rounded-2xl">
                    <div>
                      <h3 className="text-white font-bold text-base">💡 Ruh haline özel öneriler hazır!</h3>
                      <p className="text-slate-400 text-xs mt-1">Sana en uygun olanları seç ve listene ekle.</p>
                    </div>
                    <button
                      onClick={() => setPhase("mood")}
                      className="bg-indigo-600 hover:bg-indigo-500 text-white font-bold px-4 py-2 rounded-xl text-xs transition-colors"
                    >
                      😊 Moodunu Güncelle
                    </button>
                  </div>

                  <TaskPoolSelector
                    poolData={poolData}
                    onTaskSelect={async (task) => {
                      try {
                        const taskId = task.userTaskId ?? task.UserTaskId ?? task.id ?? task.Id;
                        await taskPoolApi.selectTask(taskId);
                        showToast("🎉 Görev başarıyla seçildi!", "success");
                        loadPool();
                        fetchAvailableTasks();
                      } catch (err) {
                        console.error("Görev seçilemedi:", err);
                        showToast("Görev seçilirken bir hata oluştu.", "error");
                      }
                    }}
                    onRefresh={loadPool}
                  />
                </div>
              )
            )}
          </div>
        ) : (
          /* SECTION 2: ALL LEVEL-APPROPRIATE TASKS */
          <div className="space-y-8">
            {/* Quick Stats Banner */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="bg-slate-900/40 p-4 border border-slate-800 rounded-2xl flex items-center justify-between">
                <div>
                  <p className="text-slate-500 text-[10px] font-black uppercase tracking-wider">Mevcut Seviyen</p>
                  <p className="text-2xl font-black text-fuchsia-400">{userLevel} Seviye</p>
                </div>
                <span className="text-3xl">🛡️</span>
              </div>
              <div className="bg-slate-900/40 p-4 border border-slate-800 rounded-2xl flex items-center justify-between">
                <div>
                  <p className="text-slate-500 text-[10px] font-black uppercase tracking-wider">Seviyene Uygun Görev</p>
                  <p className="text-2xl font-black text-white">{availableTasks.length} Görev</p>
                </div>
                <span className="text-3xl">⚔️</span>
              </div>
              <div className="bg-slate-900/40 p-4 border border-slate-800 rounded-2xl flex items-center justify-between">
                <div>
                  <p className="text-slate-500 text-[10px] font-black uppercase tracking-wider">En Popüler Kategori</p>
                  <p className="text-2xl font-black text-indigo-400">{mostCommonCategory}</p>
                </div>
                <span className="text-3xl">🔥</span>
              </div>
            </div>

            {/* Category selection */}
            <div className="flex flex-wrap gap-2 items-center">
              <span className="text-xs font-black text-slate-400 uppercase mr-2 tracking-wider">
                Kategori Seç:
              </span>
              {uniqueCategories.map((cat) => (
                <button
                  key={cat}
                  onClick={() => handleCategoryChange(cat)}
                  className={`px-4 py-2 rounded-xl text-xs font-bold transition-all border ${
                    selectedCategory === cat
                      ? "bg-fuchsia-600/20 text-fuchsia-300 border-fuchsia-500/40 shadow-[0_0_15px_rgba(217,70,239,0.2)]"
                      : "bg-slate-900 hover:bg-slate-800 text-slate-400 border-slate-800"
                  }`}
                >
                  {cat === "all" ? "Tüm Kategoriler" : cat}
                </button>
              ))}
            </div>

            {loading ? (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-pulse">
                {[1, 2, 3, 4, 5, 6].map((i) => (
                  <div key={i} className="h-48 bg-slate-900/50 rounded-2xl border border-slate-800"></div>
                ))}
              </div>
            ) : paginatedTasks.length === 0 ? (
              <div className="text-center py-16 border-2 border-slate-800/50 rounded-[2rem] bg-slate-900/20 border-dashed flex flex-col items-center gap-4">
                <span className="text-5xl">🏆</span>
                <p className="text-slate-400 font-semibold text-lg">Bu kategoride seviyene uygun görev kalmadı.</p>
                <p className="text-slate-600 text-sm max-w-sm">
                  Diğer kategorileri inceleyebilirsin!
                </p>
              </div>
            ) : (
              <>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                  {paginatedTasks.map((t) => (
                    <div
                      key={t.id}
                      className="relative bg-slate-900/40 hover:bg-slate-900/80 border border-slate-800 hover:border-fuchsia-500/50 p-6 rounded-3xl transition-all duration-300 group flex flex-col justify-between h-full gap-4 overflow-hidden shadow-2xl hover:shadow-fuchsia-500/10"
                    >
                      <div className="absolute -right-6 -top-6 w-24 h-24 bg-fuchsia-500/10 blur-[50px] rounded-full group-hover:bg-fuchsia-500/20 transition-all"></div>

                      <div className="space-y-3 relative z-10">
                        <div className="flex justify-between items-start">
                          <span className="text-xs bg-fuchsia-500/15 text-fuchsia-300 border border-fuchsia-500/20 px-3 py-1.5 rounded-full font-bold flex items-center gap-1">
                            {t.categoryIcon || "🎯"} {t.categoryName || "Genel"}
                          </span>
                          <span className="text-xs font-black text-amber-400 bg-amber-500/10 border border-amber-500/20 px-2.5 py-1 rounded-full">
                            +{t.baseXP || t.xp} XP
                          </span>
                        </div>

                        <h4 className="text-white font-black text-lg group-hover:text-fuchsia-400 transition-colors tracking-tight line-clamp-1">
                          {t.title}
                        </h4>
                        <p className="text-slate-400 text-xs font-medium line-clamp-3 leading-relaxed">
                          {t.description}
                        </p>

                        {t.whyThisTask && (
                          <p className="text-[11px] text-slate-400 pt-2 border-t border-slate-800/60 leading-relaxed italic">
                            🎯 <strong>Amaç:</strong> {t.whyThisTask}
                          </p>
                        )}
                      </div>

                      <div className="relative z-10 mt-auto pt-2 flex flex-col gap-2">
                        <div className="flex justify-between items-center text-[11px] font-bold text-slate-500">
                          <span>Zorluk: <strong className="text-slate-300">{t.difficulty || "Normal"}</strong></span>
                          <span>Min Seviye: <strong className="text-slate-300">{t.minLevel || 1}</strong></span>
                        </div>

                        <button
                          onClick={() => handleAssignAppTask(t.id)}
                          disabled={actionLoading !== null}
                          className="w-full bg-fuchsia-600 hover:bg-fuchsia-500 disabled:bg-slate-800 text-white text-xs font-black py-3 rounded-2xl transition-all duration-300 active:scale-95 shadow-lg shadow-fuchsia-600/20 hover:shadow-fuchsia-600/40 flex items-center justify-center gap-2"
                        >
                          {actionLoading === t.id ? (
                            <span className="animate-spin">↻</span>
                          ) : (
                            <>
                              <span>GÖREVİ LİSTENE EKLE</span>
                              <span>➕</span>
                            </>
                          )}
                        </button>
                      </div>
                    </div>
                  ))}
                </div>

                {/* Pagination Controls */}
                {totalPages > 1 && (
                  <div className="flex justify-center items-center gap-4 pt-4">
                    <button
                      onClick={() => setCurrentPage((prev) => Math.max(prev - 1, 1))}
                      disabled={currentPage === 1}
                      className="px-4 py-2 text-xs font-black bg-slate-900 border border-slate-800 hover:bg-slate-800 disabled:opacity-50 text-slate-300 hover:text-white rounded-xl transition-all"
                    >
                      Önceki
                    </button>
                    <span className="text-xs font-black text-slate-400">
                      Sayfa <strong className="text-fuchsia-400">{currentPage}</strong> / {totalPages}
                    </span>
                    <button
                      onClick={() => setCurrentPage((prev) => Math.min(prev + 1, totalPages))}
                      disabled={currentPage === totalPages}
                      className="px-4 py-2 text-xs font-black bg-slate-900 border border-slate-800 hover:bg-slate-800 disabled:opacity-50 text-slate-300 hover:text-white rounded-xl transition-all"
                    >
                      Sonraki
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export default TaskPoolPage;
