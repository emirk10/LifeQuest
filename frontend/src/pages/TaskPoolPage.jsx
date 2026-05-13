import React, { useState, useEffect, useMemo, useCallback } from "react";
import { useNavigate } from "react-router-dom";
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
  const [userLimits, setUserLimits] = useState({ daily: 3, today: 0, active: 0 });
  const [selectedInSession, setSelectedInSession] = useState(0);
  const [pendingSelections, setPendingSelections] = useState(new Map()); // Map<taskId, {task, type}>
  const pageSize = 6;
  const navigate = useNavigate();

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
        setUserLimits({
          daily: freshData.dailyTaskLimit ?? 3,
          today: freshData.todayTaskCount ?? 0,
          active: freshData.activeTaskCount ?? 0
        });
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

  const toggleSelection = (task, type = 'all') => {
    const taskId = task.userTaskId ?? task.UserTaskId ?? task.id ?? task.Id;
    
    setPendingSelections(prev => {
      const newMap = new Map(prev);
      if (newMap.has(taskId)) {
        newMap.delete(taskId);
      } else {
        // Limit check BEFORE adding
        if (userLimits.today + newMap.size >= userLimits.daily) {
          showToast(`⚠️ Günlük limitine (${userLimits.daily}) ulaştın!`, "error");
          return prev; // Do not add
        }
        newMap.set(taskId, { task, type });
      }
      return newMap;
    });
  };

  const handleAssignAppTask = async (appTaskId) => {
    // This is no longer used directly on button click, we use toggleSelection now.
    // Keeping it here just in case, but refactored to toggleSelection.
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

  const handleFinishSelection = async () => {
    if (pendingSelections.size === 0) {
      navigate("/tasks");
      return;
    }

    setActionLoading('finish');
    let successCount = 0;
    
    try {
      // Process all pending selections
      for (const [taskId, data] of pendingSelections.entries()) {
        if (data.type === 'recommended') {
          await taskPoolApi.selectTask(taskId);
        } else {
          await axiosClient.post(`/user-tasks/assign/${taskId}`);
        }
        successCount++;
      }
      
      showToast(`🎉 ${successCount} görev başarıyla listene eklendi!`, "success");
      setPendingSelections(new Map());
      setTimeout(() => navigate("/tasks"), 1000);
      
    } catch (err) {
      console.error("Toplu atama sırasında hata:", err);
      const msg = err.response?.data?.message || "Görevler atanırken bir hata oluştu.";
      showToast(msg, "error");
      
      // If some succeeded, we still want to clear them and maybe navigate
      if (successCount > 0) {
        showToast(`⚠️ Sadece ${successCount} görev atanabildi.`, "warning");
        setTimeout(() => navigate("/tasks"), 2000);
      }
    } finally {
      setActionLoading(null);
    }
  };

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
        
        {/* Limit Warning Banner */}
        {userLimits.today >= userLimits.daily && (
          <div className="bg-amber-500/10 border border-amber-500/30 p-6 rounded-[2rem] flex flex-col md:flex-row items-center gap-6 animate-pulse">
            <div className="text-4xl">⚠️</div>
            <div className="flex-1 text-center md:text-left">
              <h3 className="text-amber-400 font-black text-lg tracking-tight">BUGÜNLÜK BU KADAR!</h3>
              <p className="text-amber-500/80 text-sm font-medium">
                Günlük görev limitine ({userLimits.daily}/{userLimits.daily}) ulaştın. Harika bir iş çıkardın! Yeni hakların <strong>gece yarısı (00:00)</strong> otomatik olarak tanımlanacaktır.
              </p>
            </div>
            <button
              onClick={handleFinishSelection}
              className="bg-amber-500 hover:bg-amber-400 text-slate-950 font-black px-6 py-3 rounded-2xl text-xs transition-all shadow-lg shadow-amber-500/20"
            >
              GÖREVLERİME GİT
            </button>
          </div>
        )}

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
                    isPoolMode={true}
                    isLimitReached={userLimits.today + pendingSelections.size >= userLimits.daily}
                    selectedIds={new Set(pendingSelections.keys())}
                    onTaskSelect={(task) => toggleSelection(task, 'recommended')}
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
                          onClick={() => toggleSelection(t, 'all')}
                          disabled={actionLoading !== null || (!pendingSelections.has(t.id) && userLimits.today + pendingSelections.size >= userLimits.daily)}
                          className={`w-full ${pendingSelections.has(t.id) ? 'bg-slate-700 hover:bg-slate-600' : 'bg-fuchsia-600 hover:bg-fuchsia-500'} disabled:bg-slate-800 disabled:opacity-50 text-white text-xs font-black py-3 rounded-2xl transition-all duration-300 active:scale-95 shadow-lg shadow-fuchsia-600/20 hover:shadow-fuchsia-600/40 flex items-center justify-center gap-2`}
                        >
                          {actionLoading === t.id ? (
                            <span className="animate-spin">↻</span>
                          ) : pendingSelections.has(t.id) ? (
                            <>
                              <span>SEÇİMİ KALDIR</span>
                              <span>❌</span>
                            </>
                          ) : (
                            <>
                              <span>{userLimits.today + pendingSelections.size >= userLimits.daily ? "LİMİT DOLDU" : "GÖREVİ LİSTENE EKLE"}</span>
                              <span>{userLimits.today + pendingSelections.size >= userLimits.daily ? "🔒" : "➕"}</span>
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

        {/* Global Redirection Button */}
        {(selectedInSession > 0 || pendingSelections.size > 0 || userLimits.active > 0) && (
          <div className="flex justify-center py-12 border-t border-slate-800/50 mt-12">
            <button
              onClick={handleFinishSelection}
              disabled={actionLoading === 'finish'}
              className="group relative px-12 py-6 bg-gradient-to-r from-indigo-600 via-purple-600 to-fuchsia-600 rounded-3xl font-black text-white shadow-[0_20px_50px_rgba(79,70,229,0.3)] hover:shadow-indigo-500/50 transition-all hover:scale-105 active:scale-95 overflow-hidden disabled:opacity-75 disabled:scale-100"
            >
              <div className="absolute inset-0 bg-white/20 translate-y-full group-hover:translate-y-0 transition-transform duration-300"></div>
              <span className="relative z-10 flex items-center gap-4 text-lg tracking-wider">
                {actionLoading === 'finish' ? (
                  <>
                    <span className="animate-spin">↻</span>
                    ATAMALAR YAPILIYOR...
                  </>
                ) : pendingSelections.size > 0 ? (
                  <>
                    SEÇİLEN {pendingSelections.size} GÖREVİ ATAYIP BAŞLA
                    <span className="text-2xl group-hover:translate-x-2 transition-transform">⚔️</span>
                  </>
                ) : (
                  <>
                    GÖREV LİSTEME GİT VE BAŞLA
                    <span className="text-2xl group-hover:translate-x-2 transition-transform">⚔️</span>
                  </>
                )}
              </span>
            </button>
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export default TaskPoolPage;
