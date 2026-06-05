import React, { useState, useEffect } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import MainLayout from "../layouts/MainLayout";
import TaskItem from "../components/TaskItem";
import taskService from "../services/taskService";
import categoryService from "../services/categoryService";
import TaskTimer from "../components/TaskTimer";

const TaskList = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();
  const currentCategoryName = searchParams.get("category");

  const [tasks, setTasks] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [requesting, setRequesting] = useState(false);
  const [toast, setToast] = useState(null);
  const [activeTimerTask, setActiveTimerTask] = useState(null);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [activeTab, setActiveTab] = useState("active");
  const pageSize = 10;

  const showToast = (message, type = "success") => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const data = await categoryService.getAllCategories();
        setCategories(Array.isArray(data) ? data : data?.categories || []);
      } catch (err) {
        console.error(err);
      }
    };
    fetchCategories();
  }, []);

  const fetchTasks = async () => {
    setLoading(true);
    try {
      const selectedCategory = categories.find((c) => c.name === currentCategoryName);
      const categoryId = selectedCategory ? selectedCategory.id : null;
      const data = await taskService.getAllTasks(categoryId, page, pageSize);
      setTasks(data.tasks || []);
      setTotalCount(data.totalCount || 0);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (categories.length > 0 || !currentCategoryName) {
      fetchTasks();
    }
  }, [currentCategoryName, categories, page]);

  // Yardımcı: Görevleri tarihe göre grupla
  const groupTasksByDate = (tasks) => {
    const groups = {};
    tasks.forEach((task) => {
      const date = new Date(task.assignedAt).toLocaleDateString("tr-TR", {
        day: "numeric",
        month: "long",
        year: "numeric",
      });
      if (!groups[date]) groups[date] = [];
      groups[date].push(task);
    });
    return groups;
  };

  const filteredTasks = tasks.filter((task) => {
    if (activeTab === "active") {
      return !task.isCompleted && task.status !== "Postponed" && task.status !== "Cancelled" && task.status !== "Abandoned";
    }
    if (activeTab === "completed") {
      return task.isCompleted || task.status === "Completed";
    }
    if (activeTab === "postponed") {
      return task.status === "Postponed";
    }
    if (activeTab === "cancelled") {
      return task.status === "Cancelled" || task.status === "Abandoned";
    }
    return true;
  });

  const taskGroups = groupTasksByDate(filteredTasks);
  const totalPages = Math.ceil(totalCount / pageSize);

  const handleCategoryChange = (e) => {
    const selectedName = e.target.value;
    setPage(1); // Kategori değişince başa dön
    if (selectedName === "all") {
      setSearchParams({});
    } else {
      setSearchParams({ category: selectedName });
    }
  };

  const handleCompleteTask = async (taskId, reflectionText = null, customTags = []) => {
    try {
      const result = await taskService.completeTask(taskId, reflectionText, customTags);
      if (result.success) {
        setTasks((prevTasks) =>
          prevTasks.map((t) => (t.id === taskId ? { ...t, isCompleted: true } : t))
        );
        window.dispatchEvent(new Event("userUpdated"));
        showToast(`🎉 Görev tamamlandı! +${result.earnedXp} XP kazandın!`, "success");
      } else {
        showToast(result.message || "Görev tamamlanamadı.", "error");
      }
    } catch (err) {
      console.error(err);
      showToast("Bir hata oluştu. Lütfen tekrar dene.", "error");
    }
  };

  const handlePostponeTask = (taskId) => {
    setTasks((prevTasks) =>
      prevTasks.map((t) =>
        t.id === taskId ? { ...t, status: 'Postponed' } : t
      )
    );
    showToast("📅 Görev yarına ertelendi.", "success");
  };

  const handleCancelPostponeTask = (taskId) => {
    setTasks((prevTasks) =>
      prevTasks.map((t) =>
        t.id === taskId ? { ...t, status: 'Selected', postponedTo: null } : t
      )
    );
    showToast("Görevin ertelemesi iptal edildi.", "success");
  };

  const handleCancelTask = async (taskId) => {
    try {
      const result = await taskService.cancelTask(taskId);
      if (result.success) {
        setTasks((prevTasks) =>
          prevTasks.map((t) => (t.id === taskId ? { ...t, status: 'Cancelled' } : t))
        );
        window.dispatchEvent(new Event("userUpdated"));
        showToast("🚫 Görev iptal edildi.", "success");
      } else {
        showToast(result.message || "Görev iptal edilemedi.", "error");
      }
    } catch (err) {
      console.error(err);
      showToast("Bir hata oluştu. Lütfen tekrar dene.", "error");
    }
  };

  const handleReassignTask = async (taskId) => {
    try {
      const result = await taskService.selectTask(taskId);
      if (result) {
        setTasks((prevTasks) =>
          prevTasks.map((t) => (t.id === taskId ? { ...t, status: 'Selected' } : t))
        );
        window.dispatchEvent(new Event("userUpdated"));
        showToast("🔄 Görev yeniden atandı!", "success");
      }
    } catch (err) {
      console.error(err);
      showToast(err.response?.data?.message || "Görev yeniden atanamadı.", "error");
    }
  };


  const handleRequestNewQuests = () => {
    navigate("/task-pool");
  };

  const handleFocusTask = (task) => {
    setActiveTimerTask(task);
  };

  const completedCount = tasks.filter((t) => t.isCompleted).length;
  const progress = totalCount > 0 ? Math.round((completedCount / totalCount) * 100) : 0;

  return (
    <MainLayout>
      {/* Toast Notification */}
      {toast && (
        <div className={`fixed top-6 right-6 z-50 px-6 py-4 rounded-2xl font-bold text-sm shadow-2xl transition-all animate-bounce ${
          toast.type === "success"
            ? "bg-emerald-600 text-white shadow-emerald-500/30"
            : "bg-rose-600 text-white shadow-rose-500/30"
        }`}>
          {toast.message}
        </div>
      )}

      <div className="max-w-5xl mx-auto space-y-10">
        {/* Header */}
        <section className="flex flex-col md:flex-row justify-between items-end gap-4">
          <div>
            <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2">
              GÖREVLER
            </h2>
            <div className="h-1.5 w-24 bg-indigo-600 rounded-full shadow-[0_0_20px_rgba(79,70,229,0.5)]"></div>
            <p className="text-slate-500 mt-2 text-sm italic font-medium">
              {currentCategoryName
                ? `${currentCategoryName.toUpperCase()} KATEGORİSİ`
                : "TÜM GÖREVLERİN LİSTESİ"}
            </p>
          </div>
          <div className="flex gap-4">
            <button
              onClick={handleRequestNewQuests}
              className="bg-slate-800 border border-slate-700 text-slate-300 hover:text-white hover:bg-indigo-600 hover:border-indigo-500 px-4 py-3 rounded-xl text-sm font-bold transition-all flex items-center gap-2"
            >
              <span>✨ Görev Havuzuna Git</span>
            </button>

            <div className="relative">
              <select
                className="appearance-none bg-slate-900 border border-slate-800 text-slate-300 text-sm rounded-xl py-3 pl-4 pr-10 focus:ring-2 focus:ring-indigo-500 outline-none cursor-pointer hover:border-slate-700 transition-colors"
                onChange={handleCategoryChange}
                value={currentCategoryName || "all"}
              >
                <option value="all">Tüm Kategoriler</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.name}>
                    {c.name}
                  </option>
                ))}
              </select>
              <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-slate-500">▾</div>
            </div>
          </div>
        </section>

        {/* Progress Bar */}
        {totalCount > 0 && !loading && (
          <div className="bg-slate-900/40 p-4 rounded-2xl border border-slate-800">
            <div className="flex justify-between text-xs font-bold text-slate-400 mb-2 uppercase tracking-wider">
              <span>Günlük İlerleme</span>
              <span className={progress === 100 ? "text-emerald-400" : "text-indigo-400"}>
                {completedCount}/{totalCount} Görev — {progress}%
              </span>
            </div>
            <div className="h-2 bg-slate-800 rounded-full overflow-hidden">
              <div
                className={`h-full rounded-full transition-all duration-1000 ${
                  progress === 100
                    ? "bg-emerald-500 shadow-[0_0_10px_rgba(16,185,129,0.5)]"
                    : "bg-gradient-to-r from-indigo-600 to-indigo-400"
                }`}
                style={{ width: `${progress}%` }}
              />
            </div>
          </div>
        )}

        {/* Tabs Filter */}
        <div className="flex border-b border-slate-800 gap-6 text-sm font-bold pb-2 overflow-x-auto">
          {[
            { id: "active", label: "Aktif Görevler", count: tasks.filter(t => !t.isCompleted && t.status !== "Postponed" && t.status !== "Cancelled" && t.status !== "Abandoned").length },
            { id: "completed", label: "Tamamlananlar", count: tasks.filter(t => t.isCompleted || t.status === "Completed").length },
            { id: "postponed", label: "Ertelenenler", count: tasks.filter(t => t.status === "Postponed").length },
            { id: "cancelled", label: "İptal Edilenler", count: tasks.filter(t => t.status === "Cancelled").length }
          ].map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`pb-4 px-2 relative transition-all duration-300 whitespace-nowrap ${
                activeTab === tab.id
                  ? "text-indigo-400 font-extrabold"
                  : "text-slate-500 hover:text-slate-300"
              }`}
            >
              <span>{tab.label}</span>
              {tab.count > 0 && (
                <span className={`ml-2 px-2 py-0.5 rounded-full text-xs font-black ${
                  activeTab === tab.id
                    ? "bg-indigo-500/20 text-indigo-400"
                    : "bg-slate-800 text-slate-400"
                }`}>
                  {tab.count}
                </span>
              )}
              {activeTab === tab.id && (
                <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-indigo-500 rounded-full shadow-[0_0_10px_rgba(99,102,241,0.8)]"></div>
              )}
            </button>
          ))}
        </div>

        {/* Task List */}
        <div className="space-y-12">
          {!loading ? (
            filteredTasks.length > 0 ? (
              Object.keys(taskGroups).map((date) => (
                <div key={date} className="space-y-4">
                  <div className="flex items-center gap-4">
                    <h3 className="text-sm font-black text-slate-500 uppercase tracking-widest whitespace-nowrap">
                      {date === new Date().toLocaleDateString("tr-TR", { day: "numeric", month: "long", year: "numeric" }) ? "BUGÜN" : date}
                    </h3>
                    <div className="h-px w-full bg-slate-800"></div>
                  </div>
                  <div className="grid gap-4">
                    {taskGroups[date].map((task) => (
                      <TaskItem
                        key={task.id}
                        id={task.id}
                        title={task.title}
                        description={task.description}
                        xp={task.baseXP || task.xp}
                        difficulty={task.difficulty}
                        isCompleted={task.isCompleted}
                        status={task.status}
                        postponedTo={task.postponedTo}
                        onComplete={handleCompleteTask}
                        onPostpone={handlePostponeTask}
                        onCancelPostpone={handleCancelPostponeTask}
                        onFocus={handleFocusTask}
                        onCancel={handleCancelTask}
                        onReassign={handleReassignTask}
                      />
                    ))}
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-16 border-2 border-slate-800/50 rounded-[2rem] bg-slate-900/20 border-dashed flex flex-col items-center gap-6">
                <div className="text-6xl">🎯</div>
                <div>
                  <h3 className="text-white font-bold text-xl">
                    {activeTab === "active" && "Maceraya Hazır Mısın?"}
                    {activeTab === "completed" && "Henüz Tamamlanan Görev Yok"}
                    {activeTab === "postponed" && "Ertelenen Görev Yok"}
                    {activeTab === "cancelled" && "İptal Edilen Görev Yok"}
                  </h3>
                  <p className="text-slate-500 text-sm mt-1">
                    {activeTab === "active" && "Şu an aktif bir görevin yok. Hemen yenilerini alabilirsin."}
                    {activeTab === "completed" && "Henüz hiçbir görevi tamamlamadın. Aktif görevleri bitirerek başlayabilirsin!"}
                    {activeTab === "postponed" && "Şu anda ertelenmiş bir görevin bulunmuyor."}
                    {activeTab === "cancelled" && "Şu anda iptal edilmiş bir görevin bulunmuyor."}
                  </p>
                </div>
                {activeTab === "active" && (
                  <button
                    onClick={handleRequestNewQuests}
                    className="bg-indigo-600 hover:bg-indigo-500 text-white px-8 py-4 rounded-2xl font-black uppercase tracking-widest shadow-lg shadow-indigo-500/30 transition-all hover:scale-105 active:scale-95"
                  >
                    GÖREV HAVUZUNA GİT
                  </button>
                )}
              </div>
            )
          ) : (
            [1, 2, 3, 4].map((i) => (
              <div
                key={i}
                className="h-20 bg-slate-900/50 rounded-2xl animate-pulse border border-slate-800"
              ></div>
            ))
          )}
        </div>

        {/* Pagination Controls */}
        {totalPages > 1 && (
          <div className="flex justify-center items-center gap-6 pt-10 border-t border-slate-800/50">
            <button
              onClick={() => setPage(p => Math.max(1, p - 1))}
              disabled={page === 1 || loading}
              className="group px-6 py-3 bg-slate-900 border border-slate-800 text-slate-400 hover:text-white hover:border-indigo-500 rounded-xl transition-all disabled:opacity-30 disabled:pointer-events-none flex items-center gap-2"
            >
              <span>←</span>
              <span className="text-xs font-bold uppercase tracking-tighter">Önceki</span>
            </button>
            
            <div className="flex items-center gap-2">
              <span className="text-xs font-black text-slate-500 uppercase">SAYFA</span>
              <span className="text-lg font-black text-white bg-indigo-600/20 border border-indigo-500/30 w-10 h-10 flex items-center justify-center rounded-lg shadow-lg shadow-indigo-500/10">
                {page}
              </span>
              <span className="text-xs font-black text-slate-500 uppercase tracking-widest">/ {totalPages}</span>
            </div>

            <button
              onClick={() => setPage(p => Math.min(totalPages, p + 1))}
              disabled={page === totalPages || loading}
              className="group px-6 py-3 bg-slate-900 border border-slate-800 text-slate-400 hover:text-white hover:border-indigo-500 rounded-xl transition-all disabled:opacity-30 disabled:pointer-events-none flex items-center gap-2"
            >
              <span className="text-xs font-bold uppercase tracking-tighter">Sonraki</span>
              <span>→</span>
            </button>
          </div>
        )}
      </div>

      {activeTimerTask && (
        <TaskTimer 
          task={activeTimerTask} 
          onClose={() => setActiveTimerTask(null)}
          onComplete={(id) => {
            handleCompleteTask(id);
            setActiveTimerTask(null);
          }}
        />
      )}
    </MainLayout>
  );
};

export default TaskList;
