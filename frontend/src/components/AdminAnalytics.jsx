import React, { useState, useEffect } from 'react';
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, 
  PieChart, Pie, Cell, LineChart, Line, AreaChart, Area
} from 'recharts';
import axiosClient from '../api/axiosClient';

const COLORS = ['#6366f1', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];

const AdminAnalytics = () => {
  const [dropoutData, setDropoutData] = useState([]);
  const [categoryData, setCategoryData] = useState([]);
  const [hourlyData, setHourlyData] = useState([]);
  const [retention, setRetention] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        const [drop, cat, hour, ret] = await Promise.all([
          axiosClient.get('/admin/analytics/task-dropout'),
          axiosClient.get('/admin/analytics/category-performance'),
          axiosClient.get('/admin/analytics/hourly-completions'),
          axiosClient.get('/admin/analytics/user-retention')
        ]);
        
        setDropoutData(drop.data);
        setCategoryData(cat.data);
        setHourlyData(hour.data);
        setRetention(ret.data);
      } catch (err) {
        console.error("Analytics fetch error:", err);
      } finally {
        setLoading(false);
      }
    };
    fetchAnalytics();
  }, []);

  if (loading) return <div className="p-8 text-center text-slate-500 animate-pulse">Analitik veriler yükleniyor...</div>;

  return (
    <div className="space-y-8 p-4">
      {/* Retention Stats */}
      {retention && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="bg-slate-900/60 border border-slate-800 p-6 rounded-3xl">
            <p className="text-[10px] text-slate-500 font-black uppercase tracking-widest mb-1">Toplam Kullanıcı</p>
            <p className="text-3xl font-black text-white">{retention.totalUsers}</p>
          </div>
          <div className="bg-slate-900/60 border border-slate-800 p-6 rounded-3xl">
            <p className="text-[10px] text-slate-500 font-black uppercase tracking-widest mb-1">Aktif (Son 7 Gün)</p>
            <p className="text-3xl font-black text-indigo-400">{retention.activeUsersLast7Days}</p>
          </div>
          <div className="bg-slate-900/60 border border-slate-800 p-6 rounded-3xl">
            <p className="text-[10px] text-slate-500 font-black uppercase tracking-widest mb-1">Retention Oranı</p>
            <p className="text-3xl font-black text-emerald-400">%{(retention.retentionRate * 100).toFixed(1)}</p>
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Task Dropout (Pie) */}
        <div className="bg-slate-900/60 border border-slate-800 p-8 rounded-[2rem]">
          <h4 className="text-sm font-black text-white uppercase tracking-widest mb-6">Görev Durum Dağılımı</h4>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={dropoutData}
                  innerRadius={60}
                  outerRadius={80}
                  paddingAngle={5}
                  dataKey="count"
                  nameKey="status"
                >
                  {dropoutData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip 
                  contentStyle={{ backgroundColor: '#0f172a', border: '1px solid #1e293b', borderRadius: '12px' }}
                  itemStyle={{ color: '#fff', fontSize: '12px', fontWeight: 'bold' }}
                />
              </PieChart>
            </ResponsiveContainer>
          </div>
          <div className="flex flex-wrap justify-center gap-4 mt-4">
            {dropoutData.map((d, i) => (
              <div key={i} className="flex items-center gap-2">
                <div className="w-2 h-2 rounded-full" style={{ backgroundColor: COLORS[i % COLORS.length] }}></div>
                <span className="text-[10px] text-slate-400 font-bold uppercase">{d.status}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Category Performance (Bar) */}
        <div className="bg-slate-900/60 border border-slate-800 p-8 rounded-[2rem]">
          <h4 className="text-sm font-black text-white uppercase tracking-widest mb-6">Kategori Bazlı Tamamlama</h4>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={categoryData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" vertical={false} />
                <XAxis dataKey="category" stroke="#475569" fontSize={10} fontWeight="bold" />
                <YAxis stroke="#475569" fontSize={10} fontWeight="bold" />
                <Tooltip 
                  cursor={{ fill: 'rgba(99, 102, 241, 0.05)' }}
                  contentStyle={{ backgroundColor: '#0f172a', border: '1px solid #1e293b', borderRadius: '12px' }}
                />
                <Bar dataKey="count" fill="#6366f1" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Hourly Completions (Area) */}
        <div className="bg-slate-900/60 border border-slate-800 p-8 rounded-[2rem] lg:col-span-2">
          <h4 className="text-sm font-black text-white uppercase tracking-widest mb-6">Saatlik Aktivite Yoğunluğu</h4>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={hourlyData}>
                <defs>
                  <linearGradient id="colorCount" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#6366f1" stopOpacity={0.3}/>
                    <stop offset="95%" stopColor="#6366f1" stopOpacity={0}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" vertical={false} />
                <XAxis dataKey="hour" stroke="#475569" fontSize={10} fontWeight="bold" />
                <YAxis stroke="#475569" fontSize={10} fontWeight="bold" />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#0f172a', border: '1px solid #1e293b', borderRadius: '12px' }}
                />
                <Area type="monotone" dataKey="count" stroke="#6366f1" strokeWidth={3} fillOpacity={1} fill="url(#colorCount)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminAnalytics;
