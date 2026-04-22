import React, { useState, useEffect } from 'react';
import MainLayout from '../layouts/MainLayout';
import CategoryCard from '../components/CategoryCard';
import categoryService from '../services/categoryService';
import userService from '../services/userService';
import axiosClient from '../api/axiosClient';
import { getUserIdFromToken } from '../utils/authUtils';

const DashBoard = () => {
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
        } catch(err) {
          setMotivation("Hedeflerine bir adım daha yaklaşmak için bugün harika bir fırsat!");
        }

      } catch (error) {
        console.error("Genel Dashboard hatası:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboardData();
  }, []);

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
    </MainLayout>
  );
};

export default DashBoard;