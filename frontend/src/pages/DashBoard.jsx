import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';
import CategoryCard from '../components/CategoryCard';
import categoryService from '../services/categoryService';
import userService from '../services/userService';
import { moodApi } from '../api/moodApi';
import axiosClient from '../api/axiosClient';
import { getUserIdFromToken } from '../utils/authUtils';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import * as signalR from '@microsoft/signalr';


const DashBoard = () => {
  const navigate = useNavigate();
  const [categories, setCategories] = useState([]);
  const [recentActivities, setRecentActivities] = useState([]);
  const [stats, setStats] = useState({
    streak: [],
    streakCount: 0,
    totalCompleted: 0,
    weeklyGrowth: 0
  });
  const [loading, setLoading] = useState(true);
  const [motivation, setMotivation] = useState("Günlük motivasyon yükleniyor...");
  const [todayMood, setTodayMood] = useState(null);
  const [showMoodModal, setShowMoodModal] = useState(false);
  const [taskToAccept, setTaskToAccept] = useState(null);
  const [showTaskModal, setShowTaskModal] = useState(false);
  const connectionRef = useRef(null);

  // SP-03: SignalR proaktif nudge bağlantısı
  useEffect(() => {
    const userId = getUserIdFromToken();
    if (!userId) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${import.meta.env.VITE_API_URL?.replace('/api', '') ?? 'http://localhost:5000'}/hubs/notification`, {
        accessTokenFactory: () => sessionStorage.getItem('token') ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    connectionRef.current = connection;

    connection.on('ReceiveNotification', (notif) => {
      if (notif?.type === 'ProactiveNudge') {
        toast(
          <div>
            <p className="font-bold text-sm mb-1">LifeQuest AI 🤖</p>
            <p className="text-xs">{notif.message}</p>
          </div>,
          {
            autoClose: 8000,
            className: 'bg-slate-800 border border-indigo-500/40 text-white rounded-2xl shadow-xl',
            progressClassName: 'bg-indigo-500',
            onClick: () => {
              // ChatBot açılsun — window event üzerinden ChatBot dinler
              window.dispatchEvent(new CustomEvent('openChatBot', { detail: { message: notif.message } }));
            }
          }
        );
      }
    });

    connection.start().catch(err => console.warn('SignalR bağlanamıyor:', err));

    return () => { connection.stop(); };
  }, []);

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        try {
          const catsRes = await categoryService.getAllCategories();
          setCategories(Array.isArray(catsRes) ? catsRes : (catsRes?.categories || []));
        } catch (err) {
          console.error("Kategoriler hatası:", err);
        }

        try {
          const actsRes = await userService.getActivities();
          const activitiesList = actsRes.activities || actsRes.Activities || actsRes || [];
          setRecentActivities(Array.isArray(activitiesList) ? activitiesList : []);
        } catch (err) {
          console.error("Aktiviteler hatası:", err);
          setRecentActivities([]);
        }

        try {
          const statsRes = await userService.getUserStats();
          setStats(statsRes || { streak: [], streakCount: 0, totalCompleted: 0, weeklyGrowth: 0 });
        } catch (err) {
          console.error("İstatistik hatası:", err);
        }

        // AI Motivasyon Çekme
        try {
          const aiResponse = await axiosClient.post('/chat/send', { 
            message: "Beni motive et" 
          });
          if (aiResponse.data && aiResponse.data.response) {
            setMotivation(aiResponse.data.response);
          } else {
             setMotivation("Bugün hedeflerine ulaşmak için harika bir gün! Küçük adımlar büyük sonuçlar getirir.");
          }
        } catch (err) {
          setMotivation("Bugün hedeflerine ulaşmak için harika bir gün! Küçük adımlar büyük sonuçlar getirir.");
        }

        // Mood Check
        try {
          const moodRes = await moodApi.getTodayMood();
          if (moodRes.data?.data) {
            setTodayMood(moodRes.data.data);
          } else {
            setShowMoodModal(true);
          }
        } catch (err) {
          setShowMoodModal(true);
        }

      } catch (error) {
        console.error("Genel Dashboard hatası:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

  useEffect(() => {
    const checkAcceptTask = async () => {
      const acceptTask = sessionStorage.getItem("acceptTask");
      if (acceptTask) {
        try {
          const res = await axiosClient.get("/tasks/getall");
          const allTasks = res.data?.tasks || res.data?.Tasks || res.data || [];
          const matchingTask = allTasks.find(t => t.id === acceptTask);
          if (matchingTask) {
            setTaskToAccept(matchingTask);
            setShowTaskModal(true);
          } else {
            sessionStorage.removeItem("acceptTask");
          }
        } catch (err) {
          console.error("Görev detayları yüklenirken hata:", err);
          sessionStorage.removeItem("acceptTask");
        }
      }
    };
    checkAcceptTask();
  }, []);

  const handleAcceptTask = async () => {
    if (!taskToAccept) return;
    try {
      await axiosClient.post(`/user-tasks/assign/${taskToAccept.id}`);
      toast.success("Görev başarıyla kabul edildi ve havuzunuza eklendi! 🎉");
      sessionStorage.removeItem("acceptTask");
      setShowTaskModal(false);
      setTimeout(() => window.location.reload(), 1500);
    } catch (err) {
      console.error(err);
      toast.error(err.response?.data?.message || "Görev kabul edilirken bir hata oluştu.");
    }
  };

  const handleDeclineTask = () => {
    sessionStorage.removeItem("acceptTask");
    setShowTaskModal(false);
    toast.info("Görev paylaşımı reddedildi.");
  };

  return (
    <MainLayout>
      <div className="space-y-8">
        <section className="relative flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
          <div>
            <div className="absolute -left-10 -top-10 w-40 h-40 bg-indigo-600/10 blur-[80px] rounded-full"></div>
            <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2 relative z-10">DASHBOARD</h2>
            <div className="h-1.5 w-24 bg-indigo-600 rounded-full shadow-[0_0_20px_rgba(79,70,229,0.6)]"></div>
          </div>
          
          {/* AI Motivasyon Widget */}
          <div className="bg-gradient-to-r from-indigo-900/50 to-violet-900/50 border border-indigo-500/30 rounded-2xl p-4 flex items-center gap-4 max-w-md shadow-lg backdrop-blur-sm relative overflow-hidden group">
            <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-500/10 blur-3xl rounded-full group-hover:bg-indigo-500/20 transition-all"></div>
            <div className="text-3xl relative z-10 animate-bounce">🤖</div>
            <div className="relative z-10">
              <p className="text-[10px] text-indigo-300 font-black uppercase tracking-widest mb-1">Yapay Zeka Asistanı</p>
              <p className="text-sm font-medium text-slate-200 leading-tight italic">"{motivation}"</p>
            </div>
          </div>
        </section>

        {/* Günlük Özet Bar */}
        {todayMood && (
          <div className="bg-slate-900/40 p-4 rounded-xl border border-slate-800 flex items-center justify-between mb-4">
            <div className="flex items-center gap-3">
              <span className="text-2xl">{todayMood.moodMode === 0 ? '⚡' : todayMood.moodMode === 1 ? '😊' : todayMood.moodMode === 2 ? '😴' : todayMood.moodMode === 3 ? '😰' : '⏰'}</span>
              <div>
                <p className="text-xs text-slate-400">Bugünkü Enerjin</p>
                <p className="text-sm font-bold text-white">{todayMood.energyLevel}/5 Seviye</p>
              </div>
            </div>
            <div className="flex items-center gap-3 border-l border-slate-700 pl-4">
              <span className="text-xl">⏱️</span>
              <div>
                <p className="text-xs text-slate-400">Müsait Süren</p>
                <p className="text-sm font-bold text-white">{todayMood.availableMinutes} dk</p>
              </div>
            </div>
            <button onClick={() => navigate('/task-pool')} className="bg-indigo-600 hover:bg-indigo-500 text-white text-xs px-4 py-2 rounded-lg font-bold transition-colors">
              Görev Havuzuna Git
            </button>
          </div>
        )}

        <section>
          <h3 className="text-slate-500 text-[10px] font-black uppercase tracking-[0.3em] mb-4 ml-1">Görev Kategorileri</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {!loading && categories.length > 0 ? (
              categories.map((cat) => (
                <CategoryCard key={cat.id} {...cat} />
              ))
            ) : (
              !loading && <p className="text-slate-500 text-sm italic col-span-4 text-center py-4">Kategori bulunamadı.</p>
            )}
            {loading && [1,2,3,4].map(i => <div key={i} className="h-32 bg-slate-900/40 rounded-3xl animate-pulse border border-slate-800"></div>)}
          </div>
        </section>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 grid grid-cols-1 md:grid-cols-2 gap-6">
            
            <div className="bg-slate-900/40 p-8 rounded-[2rem] border border-slate-800 hover:border-indigo-500/30 transition-all duration-500 group relative">
               <p className="text-slate-500 font-bold text-xs uppercase tracking-widest mb-1">Haftalık Seri</p>
               <p className="text-5xl font-black text-white group-hover:scale-105 transition-transform origin-left">
                 {stats.streakCount} GÜN
               </p>
               <div className="mt-4 flex gap-1 h-2">
                 {stats.streak && stats.streak.length > 0 ? stats.streak.map((isActive, i) => (
                   <div key={i} className={`h-full w-full rounded-full transition-all ${isActive ? 'bg-indigo-500 shadow-[0_0_8px_rgba(79,70,229,0.5)]' : 'bg-slate-800'}`}></div>
                 )) : (
                   <span className="text-[10px] text-slate-600">Veri yok</span>
                 )}
               </div>
            </div>
            
            <div className="bg-slate-900/40 p-8 rounded-[2rem] border border-slate-800 hover:border-emerald-500/30 transition-all duration-500 group">
               <p className="text-slate-500 font-bold text-xs uppercase tracking-widest mb-1">Biten Görevler</p>
               <p className="text-5xl font-black text-white group-hover:scale-105 transition-transform origin-left">
                 {stats.totalCompleted}
               </p>
               <p className="text-emerald-500 text-[10px] mt-4 font-black uppercase tracking-tighter">
                 Bu hafta %{stats.weeklyGrowth} artış
               </p>
            </div>
          </div>

          <div className="bg-slate-900/60 p-8 rounded-[2rem] border border-slate-800 backdrop-blur-sm">
            <h3 className="text-white font-black text-sm uppercase tracking-widest mb-6 flex items-center gap-2">
              <span className="w-2 h-2 bg-indigo-500 rounded-full animate-pulse"></span>
              Son Aktiviteler
            </h3>
            <div className="space-y-6">
              {loading ? (
                 <div className="space-y-4">
                   {[1,2,3].map(i => <div key={i} className="h-10 bg-slate-900/40 animate-pulse rounded-lg"></div>)}
                 </div>
              ) : recentActivities.length > 0 ? (
                recentActivities.map((act) => (
                  <div key={act.id} className="flex gap-4 relative group">
                    <div className="w-[2px] bg-slate-800 absolute left-[11px] top-8 bottom-[-24px] group-last:hidden"></div>
                    <div className={`w-6 h-6 rounded-full shrink-0 flex items-center justify-center text-[10px] z-10 ${
                      act.type === 'TASK' ? 'bg-indigo-600' : act.type === 'BADGE' ? 'bg-amber-500' : 'bg-emerald-500'
                    }`}>
                      {act.type === 'TASK' ? '✓' : act.type === 'BADGE' ? '🏆' : '⭐'}
                    </div>
                    <div>
                      <p className="text-xs font-bold text-slate-200">{act.text}</p>
                      <div className="flex items-center gap-2 mt-1">
                        <span className="text-[10px] text-slate-500 font-medium">{act.time}</span>
                        {act.xp && <span className="text-[10px] text-indigo-400 font-black">{act.xp}</span>}
                      </div>
                    </div>
                  </div>
                ))
              ) : (
                 <p className="text-slate-500 text-xs italic">Henüz aktivite kaydı yok.</p>
              )}
            </div>
          </div>
        </div>
      </div>
      {/* SP-03: Nudge toast bildirimleri */}
      <ToastContainer
        position="bottom-right"
        theme="dark"
        toastClassName="!rounded-2xl !shadow-2xl"
        progressClassName="!bg-indigo-500"
      />

      {/* Mood Modal */}
      {showMoodModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-3xl w-full max-w-lg overflow-hidden shadow-2xl">
            <div className="p-6">
              <h2 className="text-2xl font-black text-white mb-2">Güne Hazır Mısın?</h2>
              <p className="text-slate-400 text-sm mb-6">Sana en uygun görevleri önerebilmemiz için bugünkü durumunu paylaş.</p>
              <button 
                onClick={() => navigate('/task-pool')}
                className="w-full bg-indigo-600 hover:bg-indigo-500 text-white py-3 rounded-xl font-bold transition-colors"
              >
                Durumunu Belirle & Görevleri Al
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Görev Kabul/Red Etme Modalı */}
      {showTaskModal && taskToAccept && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/85 backdrop-blur-md p-4">
          <div className="bg-slate-900/90 border border-indigo-500/30 rounded-3xl w-full max-w-md overflow-hidden shadow-[0_0_50px_rgba(79,70,229,0.2)] p-6 relative">
            <div className="text-center mb-6">
              <div className="w-16 h-16 bg-indigo-600/20 border border-indigo-500/40 rounded-2xl mx-auto flex items-center justify-center text-3xl shadow-inner mb-4 animate-pulse">
                ✉️
              </div>
              <p className="text-indigo-400 font-black text-[10px] uppercase tracking-wider mb-1">GÖREV PAYLAŞIMI ALDINIZ</p>
              <h2 className="text-2xl font-black text-white italic tracking-tighter uppercase mb-2">{taskToAccept.title}</h2>
              <p className="text-slate-400 text-sm">{taskToAccept.description}</p>
            </div>

            <div className="bg-slate-800/40 rounded-2xl p-4 border border-slate-800 mb-6 flex justify-around text-center">
              <div>
                <p className="text-xs text-slate-500">Zorluk</p>
                <p className="text-sm font-bold text-slate-200">{taskToAccept.difficulty}</p>
              </div>
              <div className="border-l border-slate-700 pl-4 h-full"></div>
              <div>
                <p className="text-xs text-slate-500">Ödül</p>
                <p className="text-sm font-bold text-indigo-400">{taskToAccept.baseXP || taskToAccept.baseXp} XP</p>
              </div>
            </div>

            <div className="flex gap-4">
              <button 
                onClick={handleDeclineTask}
                className="flex-1 bg-slate-800 hover:bg-slate-700 border border-slate-700 hover:border-slate-600 text-slate-300 py-3.5 rounded-2xl font-black tracking-wider text-xs uppercase transition-all duration-300"
              >
                Reddet
              </button>
              <button 
                onClick={handleAcceptTask}
                className="flex-1 bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 text-white py-3.5 rounded-2xl font-black tracking-wider text-xs uppercase shadow-[0_4px_20px_rgba(79,70,229,0.4)] hover:shadow-[0_4px_25px_rgba(79,70,229,0.6)] transition-all duration-300"
              >
                Kabul Et
              </button>
            </div>
          </div>
        </div>
      )}
    </MainLayout>
  );
};

export default DashBoard;