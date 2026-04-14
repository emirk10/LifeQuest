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

  const handleCompleteTask = async (taskId, verificationData = {}) => {
    try {
      const result = await taskService.completeTask(taskId, verificationData);

      if (result.success) {
        setTasks((prevTasks) =>
          prevTasks.map((t) =>
            t.id === taskId ? { ...t, isCompleted: true } : t
          )
        );
        window.dispatchEvent(new Event("userUpdated"));
      } else {
        alert(result.verificationMessage || result.message || "Dogrulama basarisiz.");
      }
    } catch (err) {
      console.error(err);
      const errMsg = err.response?.data?.verificationMessage
        || err.response?.data?.message
        || err.message;
      alert("Hata: " + errMsg);
    }
  };

  const handleRequestNewQuests = async () => {
    setRequesting(true);
    try {
      const response = await taskService.requestNewQuests();
      if (response.success) {
        alert(`${response.assignedCount} yeni gorev eklendi!`);
        fetchTasks();
      }
    } catch (err) {
      alert(err.response?.data?.message || "Yeni gorev alinamadi.");
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
              GOREVLER
            </h2>
            <div className="h-1.5 w-24 bg-indigo-600 rounded-full shadow-[0_0_20px_rgba(79,70,229,0.5)]"></div>
            <p className="text-slate-500 mt-2 text-sm italic font-medium">
              {currentCategoryName
                ? `${currentCategoryName.toUpperCase()} KATEGORISI`
                : "TUM GOREVLERIN LISTESI"}
            </p>
          </div>

          <div className="flex gap-4">
            <button
              onClick={handleRequestNewQuests}
              disabled={requesting}
              className="bg-slate-800 border border-slate-700 text-slate-300 hover:text-white hover:bg-indigo-600 hover:border-indigo-500 px-4 py-3 rounded-xl text-sm font-bold transition-all flex items-center gap-2"
            >
              {requesting ? (
                <span className="animate-spin">&#8635;</span>
              ) : (
                <span>&#10024; Yeni Gorev Iste</span>
              )}
            </button>

            <div className="relative group">
              <select
                className="appearance-none bg-slate-900 border border-slate-800 text-slate-300 text-sm rounded-xl py-3 pl-4 pr-10 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none cursor-pointer hover:border-slate-700 transition-colors"
                onChange={handleCategoryChange}
                value={currentCategoryName || "all"}
              >
                <option value="all">Tum Kategoriler</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.name}>
                    {c.name}
                  </option>
                ))}
              </select>
              <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-slate-500">
                &#9660;
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
                  id={task.id}
                  title={task.title}
                  xp={task.baseXP || task.xp}
                  difficulty={task.difficulty}
                  isCompleted={task.isCompleted}
                  verificationType={task.verificationType || 0}
                  verificationHint={task.verificationHint}
                  minReflectionWords={task.minReflectionWords || 10}
                  minTimerMinutes={task.minTimerMinutes || 5}
                  onComplete={handleCompleteTask}
                />
              ))
            ) : (
              <div className="text-center py-16 border-2 border-slate-800/50 rounded-[2rem] bg-slate-900/20 border-dashed flex flex-col items-center gap-6">
                <div className="text-6xl animate-bounce">&#127919;</div>
                <div>
                  <h3 className="text-white font-bold text-xl">
                    Maceraya Hazir Misin?
                  </h3>
                  <p className="text-slate-500 text-sm mt-1">
                    Su an aktif bir gorevin yok. Hemen yenilerini alabilirsin.
                  </p>
                </div>
                <button
                  onClick={handleRequestNewQuests}
                  disabled={requesting}
                  className="bg-indigo-600 hover:bg-indigo-500 text-white px-8 py-4 rounded-2xl font-black uppercase tracking-widest shadow-lg shadow-indigo-500/30 transition-all hover:scale-105 active:scale-95"
                >
                  {requesting ? "Yukleniyor..." : "YENI GOREVLER GETIR"}
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
