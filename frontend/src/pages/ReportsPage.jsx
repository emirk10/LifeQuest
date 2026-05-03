import React, { useState, useEffect } from 'react';
import MainLayout from '../layouts/MainLayout';
import reportService from '../services/reportService';
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import BehaviorInsightCard from '../components/BehaviorInsightCard';
import axiosClient from '../api/axiosClient';
import { getUserIdFromToken } from '../utils/authUtils';

const ReportsPage = () => {
  const [reports, setReports] = useState([]);
  const [loading, setLoading] = useState(true);
  const [behaviorSummary, setBehaviorSummary] = useState(null);

  useEffect(() => {
    const fetchReports = async () => {
        try {
            const res = await axiosClient.get("/reports/daily-breakdown");
            let formattedData = [];
            
            if (res.data && res.data.success && res.data.breakdown) {
                formattedData = res.data.breakdown.map(b => {
                    const d = new Date(b.date);
                    return {
                        name: d.toLocaleDateString('tr-TR', { day: '2-digit', month: 'short' }),
                        XPKazanci: b.xpKazanci ?? b.XPKazanci ?? b.XpKazanci ?? 0,
                        Tamamlanan: b.tamamlanan ?? b.Tamamlanan ?? 0
                    };
                });
            }
            
            setReports(formattedData);
        } catch (error) {
            console.error("Raporlar alınamadı:", error);
        } finally {
            setLoading(false);
        }
    };

    const fetchBehavior = async () => {
      const userId = getUserIdFromToken();
      if (!userId) return;
      try {
        const res = await axiosClient.get(`/users/${userId}/behavior-summary`);
        setBehaviorSummary(res.data);
      } catch (err) { }
    };

    fetchReports();
    fetchBehavior();
  }, []);

  return (
    <MainLayout>
      <div className="space-y-8">
        <section className="relative">
            <div className="absolute -left-10 -top-10 w-40 h-40 bg-fuchsia-600/10 blur-[80px] rounded-full"></div>
            <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2 relative z-10">RAPORLAR</h2>
            <div className="h-1.5 w-24 bg-fuchsia-600 rounded-full shadow-[0_0_20px_rgba(192,38,211,0.6)]"></div>
        </section>

        {loading ? (
            <p className="text-slate-400">Raporlar yükleniyor...</p>
        ) : reports.length === 0 ? (
            <div className="bg-slate-900/50 p-10 border border-slate-800 rounded-2xl text-center text-slate-400">
                Henüz yeterli veri yok. Raporların her hafta sonu oluşturulacak.
            </div>
        ) : (
            <div className="space-y-8">
                {/* SP-04: Adaptif Davranış Analizi */}
                {behaviorSummary && (
                   <BehaviorInsightCard summary={behaviorSummary} />
                )}

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                    {/* XP Grafiği */}
                <div className="bg-slate-900/40 border border-slate-800 p-6 rounded-3xl shadow-xl">
                    <h3 className="text-slate-300 font-bold mb-6 flex items-center gap-2">
                        <span className="w-2 h-2 bg-fuchsia-500 rounded-full"></span> 
                        Haftalık XP Gelişimi
                    </h3>
                    <div className="h-64">
                         <ResponsiveContainer width="100%" height="100%">
                            <AreaChart data={reports}>
                                <defs>
                                    <linearGradient id="colorXp" x1="0" y1="0" x2="0" y2="1">
                                        <stop offset="5%" stopColor="#c026d3" stopOpacity={0.8}/>
                                        <stop offset="95%" stopColor="#c026d3" stopOpacity={0}/>
                                    </linearGradient>
                                </defs>
                                <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" vertical={false} />
                                <XAxis dataKey="name" stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} interval={0} />
                                <YAxis stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} />
                                <Tooltip 
                                    contentStyle={{ backgroundColor: '#0f172a', border: 'none', borderRadius: '8px', color: '#fff' }}
                                    itemStyle={{ color: '#c026d3' }}
                                />
                                <Area type="monotone" dataKey="XPKazanci" stroke="#c026d3" strokeWidth={3} fillOpacity={1} fill="url(#colorXp)" />
                            </AreaChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                {/* Görev Başarı Grafiği */}
                <div className="bg-slate-900/40 border border-slate-800 p-6 rounded-3xl shadow-xl">
                    <h3 className="text-slate-300 font-bold mb-6 flex items-center gap-2">
                        <span className="w-2 h-2 bg-emerald-500 rounded-full"></span> 
                        Tamamlanan Görevler
                    </h3>
                    <div className="h-64">
                         <ResponsiveContainer width="100%" height="100%">
                            <AreaChart data={reports}>
                                <defs>
                                    <linearGradient id="colorTasks" x1="0" y1="0" x2="0" y2="1">
                                        <stop offset="5%" stopColor="#10b981" stopOpacity={0.8}/>
                                        <stop offset="95%" stopColor="#10b981" stopOpacity={0}/>
                                    </linearGradient>
                                </defs>
                                <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" vertical={false} />
                                <XAxis dataKey="name" stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} interval={0} />
                                <YAxis stroke="#64748b" fontSize={11} tickLine={false} axisLine={false} />
                                <Tooltip 
                                    contentStyle={{ backgroundColor: '#0f172a', border: 'none', borderRadius: '8px', color: '#fff' }}
                                    itemStyle={{ color: '#10b981' }}
                                />
                                <Area type="monotone" dataKey="Tamamlanan" stroke="#10b981" strokeWidth={3} fillOpacity={1} fill="url(#colorTasks)" />
                            </AreaChart>
                        </ResponsiveContainer>
                    </div>
                </div>
                </div>
            </div>
        )}
      </div>
    </MainLayout>
  );
};

export default ReportsPage;
