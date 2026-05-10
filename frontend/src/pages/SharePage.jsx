import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import axiosClient from '../api/axiosClient';

const SharePage = () => {
  const { token } = useParams();
  const [task, setTask] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchSharedTask = async () => {
      try {
        const res = await axiosClient.get(`/user-tasks/shared/${token}`);
        if (res.data.success) {
          setTask(res.data);
        } else {
          setError(res.data.message || "Görev bulunamadı.");
        }
      } catch (err) {
        setError("Paylaşılan görev yüklenirken bir hata oluştu.");
      } finally {
        setLoading(false);
      }
    };
    fetchSharedTask();
  }, [token]);

  if (loading) {
    return (
      <div className="min-h-screen bg-slate-950 flex items-center justify-center">
        <div className="w-12 h-12 border-4 border-indigo-500 border-t-transparent rounded-full animate-spin"></div>
      </div>
    );
  }

  if (error || !task) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col items-center justify-center p-4">
        <div className="text-6xl mb-4">🚫</div>
        <h1 className="text-2xl font-black text-white mb-2 uppercase italic tracking-tighter">HATA!</h1>
        <p className="text-slate-500 text-center max-w-xs">{error || "Bu paylaşım linki geçersiz veya süresi dolmuş."}</p>
        <a href="/" className="mt-8 text-indigo-400 font-bold hover:underline">LifeQuest'e Dön</a>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 selection:bg-indigo-500/30 py-12 px-4">
      <div className="max-w-2xl mx-auto">
        {/* Brand Header */}
        <div className="flex flex-col items-center mb-12">
          <div className="w-16 h-16 bg-indigo-600 rounded-2xl flex items-center justify-center shadow-2xl shadow-indigo-500/20 mb-4 transform rotate-3 hover:rotate-0 transition-transform">
            <span className="text-3xl">⚔️</span>
          </div>
          <h1 className="text-3xl font-black italic tracking-tighter text-white">LIFEQUEST</h1>
          <div className="h-1 w-12 bg-indigo-500 rounded-full mt-1"></div>
        </div>

        {/* Card */}
        <div className="bg-slate-900/60 border border-slate-800 rounded-[2.5rem] p-8 md:p-12 shadow-2xl relative overflow-hidden">
          {/* Accent decoration */}
          <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-600/10 blur-3xl -mr-16 -mt-16 rounded-full"></div>
          
          <div className="relative z-10">
            <div className="flex items-center gap-4 mb-8">
              <div className="w-14 h-14 bg-slate-800 rounded-2xl flex items-center justify-center text-3xl">
                {task.categoryIcon || "🎯"}
              </div>
              <div>
                <p className="text-[10px] text-indigo-400 font-black uppercase tracking-[0.2em] mb-1">
                  {task.categoryName} BAŞARISI
                </p>
                <h2 className="text-2xl font-black text-white tracking-tight">{task.taskTitle}</h2>
              </div>
            </div>

            <div className="space-y-6">
              <div className="bg-slate-800/30 rounded-3xl p-6 border border-slate-800/50">
                <p className="text-slate-400 text-sm leading-relaxed italic">
                  "{task.taskDescription}"
                </p>
              </div>

              {task.reflectionText && (
                <div className="bg-indigo-500/5 border border-indigo-500/20 rounded-3xl p-6">
                  <p className="text-[10px] text-indigo-400 font-black uppercase tracking-widest mb-2">KİŞİSEL NOT</p>
                  <p className="text-slate-200 text-sm leading-relaxed font-medium">
                    {task.reflectionText}
                  </p>
                </div>
              )}

              <div className="flex flex-wrap gap-2">
                {task.tags?.map((tag, i) => (
                  <span key={i} className="bg-slate-800 text-slate-400 text-[10px] font-bold px-3 py-1.5 rounded-xl border border-slate-700">
                    #{tag}
                  </span>
                ))}
              </div>

              <div className="pt-8 border-t border-slate-800 flex items-center justify-between">
                <div>
                  <p className="text-[10px] text-slate-500 font-black uppercase tracking-widest">TAMAMLAYAN</p>
                  <p className="text-white font-bold">{task.userName}</p>
                </div>
                <div className="text-right">
                  <p className="text-[10px] text-slate-500 font-black uppercase tracking-widest">TARİH</p>
                  <p className="text-slate-300 font-medium text-xs">
                    {new Date(task.completedAt).toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' })}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Call to action */}
        <div className="mt-12 text-center">
          <p className="text-slate-500 text-sm mb-6 font-medium">Sende hayatını bir RPG oyununa çevirmek ister misin?</p>
          <a
            href="/register"
            className="inline-flex items-center gap-3 bg-white text-slate-950 hover:bg-indigo-50 text-sm font-black px-8 py-4 rounded-2xl transition-all shadow-xl shadow-white/5 active:scale-95 uppercase tracking-widest"
          >
            Hemen Maceraya Başla 🚀
          </a>
        </div>
      </div>
    </div>
  );
};

export default SharePage;
