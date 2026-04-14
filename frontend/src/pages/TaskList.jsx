import React, { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import MainLayout from "../layouts/MainLayout";
import TaskItem from "../components/TaskItem";
import taskService from "../services/taskService";
import categoryService from "../services/categoryService";

const TaskList = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const currentCategoryName = searchParams.get("category");

  const [tasks, setTasks] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [requesting, setRequesting] = useState(false);

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
      const selectedCategory = categories.find(
        (c) => c.name === currentCategoryName
      );
      const categoryId = selectedCategory ? selectedCategory.id : null;

      const data = await taskService.getAllTasks(categoryId);

      const taskArray = Array.isArray(data) ? data : data?.tasks || [];
      setTasks(taskArray);
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
  }, [currentCategoryName, categories]);

  const handleCategoryChange = (e) => {
    const selectedName = e.target.value;
    if (selectedName === "all") {
      setSearchParams({});
    } else {
      setSearchParams({ category: selectedName });
    }
  };

  const handleCompleteTask = async (taskId) => {
    try {
      await taskService.completeTask(taskId);

      setTasks((prevTasks) =>
        prevTasks.map((t) =>
          t.id === taskId ? { ...t, isCompleted: true } : t
        )
      );

      window.dispatchEvent(new Event("userUpdated"));
    } catch (err) {
      console.error(err);
      alert("Hata: " + (err.response?.data?.message || err.message));
    }
  };

  const handleRequestNewQuests = async () => {
    setRequesting(true);
    try {
      const response = await taskService.requestNewQuests();
      if (response.success) {
        alert(`Harika! ${response.assignedCount} yeni görev eklendi.`);
        fetchTasks();
      }
    } catch (err) {
      alert(err.response?.data?.message || "Yeni görev alınamadı.");
    } finally {
      setRequesting(false);
    }
  };

  return (
    <MainLayout>
      <div className="max-w-5xl mx-auto space-y-10">
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
              disabled={requesting}
              className="bg-slate-800 border border-slate-700 text-slate-300 hover:text-white hover:bg-indigo-600 hover:border-indigo-500 px-4 py-3 rounded-xl text-sm font-bold transition-all flex items-center gap-2"
            >
              {requesting ? (
                <span className="animate-spin">↻</span>
              ) : (
                <span>⚡ Yeni Görev İste</span>
              )}
            </button>

            <div className="relative group">
              <select
                className="appearance-none bg-slate-900 border border-slate-800 text-slate-300 text-sm rounded-xl py-3 pl-4 pr-10 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none cursor-pointer hover:border-slate-700 transition-colors"
                onChange={handleCategoryChange}
                value={currentCategoryName || "all"}
              >
                <option value="all">📂 Tüm Kategoriler</option>
                {categories.map((c) => {

                  return (
                    <option key={c.id} value={c.name}>
                      {c.name}
                    </option>
                  );
                })}
              </select>
              <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-slate-500">
                ▼
              </div>
            </div>
          </div>
        </section>

        <div className="grid gap-4">
          {!loading ? (
            tasks.length > 0 ? (
              tasks.map((task) => (
                <TaskItem
                  key={task.id}
                  {...task}
                  onComplete={() => handleCompleteTask(task.id)}
                />
              ))
            ) : (
              <div className="text-center py-16 border-2 border-slate-800/50 rounded-[2rem] bg-slate-900/20 border-dashed flex flex-col items-center gap-6">
                <div className="text-6xl animate-bounce">✨</div>
                <div>
                  <h3 className="text-white font-bold text-xl">
                    Maceraya Hazır Mısın?
                  </h3>
                  <p className="text-slate-500 text-sm mt-1">
                    Şu an aktif bir görevin yok. Hemen yenilerini alabilirsin.
                  </p>
                </div>
                <button
                  onClick={handleRequestNewQuests}
                  disabled={requesting}
                  className="bg-indigo-600 hover:bg-indigo-500 text-white px-8 py-4 rounded-2xl font-black uppercase tracking-widest shadow-lg shadow-indigo-500/30 transition-all hover:scale-105 active:scale-95"
                >
                  {requesting ? "Yükleniyor..." : "YENİ GÖREVLER GETİR"}
                </button>
              </div>
            )
          ) : (
            [1, 2, 3].map((i) => (
              <div
                key={i}
                className="h-24 bg-slate-900/50 rounded-2xl animate-pulse border border-slate-800"
              ></div>
            ))
          )}
        </div>
      </div>
    </MainLayout>
  );
};

export default TaskList;
