import React, { useState, useEffect, useCallback } from 'react';
import MainLayout from '../layouts/MainLayout';
import goalService from '../services/goalService';
import GoalCard from '../components/GoalCard';

const GOAL_TYPES = [
  { value: 0, label: 'Genel', emoji: '⭐' },
  { value: 1, label: 'Haftalık', emoji: '📅' },
  { value: 2, label: 'Aylık', emoji: '🗓️' },
  { value: 3, label: 'Proje', emoji: '🚀' },
];

const ICONS = ['🎯', '💪', '📚', '🧘', '🏃', '💰', '🎨', '🤝', '🌱', '⚡', '🏋️', '🎓'];
const COLORS = ['#7c3aed', '#0ea5e9', '#10b981', '#f59e0b', '#ef4444', '#ec4899', '#8b5cf6', '#06b6d4'];

const GoalsPage = () => {
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('active');
  const [showModal, setShowModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [newGoal, setNewGoal] = useState({
    title: '',
    description: '',
    type: 0,
    targetValue: 30,
    bonusXP: 100,
    icon: '🎯',
    color: '#7c3aed',
    why: '',
    progressUnit: 'görev',
    autoTrackByCategory: false,
    autoTrackByXP: false,
    reminderEnabled: true,
    reminderFrequency: 'daily',
    startDate: new Date().toISOString(),
    endDate: new Date(Date.now() + 30 * 86400000).toISOString(),
    durationDays: 30,
  });

  const fetchDashboard = useCallback(async () => {
    try {
      setLoading(true);
      const data = await goalService.getGoalDashboard();
      setDashboard(data);
    } catch (err) {
      console.error('Dashboard alınamadı:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchDashboard(); }, [fetchDashboard]);

  const handleCreateGoal = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      const endDate = new Date(Date.now() + newGoal.durationDays * 86400000).toISOString();
      await goalService.createGoal({ ...newGoal, endDate });
      setShowModal(false);
      fetchDashboard();
      setNewGoal(prev => ({ ...prev, title: '', description: '', why: '' }));
    } catch (err) {
      console.error('Hedef oluşturulamadı:', err);
    } finally {
      setSubmitting(false);
    }
  };

  const tabs = [
    { id: 'active', label: 'Aktif', count: dashboard?.totalActiveGoals || 0, color: '#7c3aed' },
    { id: 'completed', label: 'Tamamlanan', count: dashboard?.totalCompletedThisMonth || 0, color: '#10b981' },
    { id: 'archived', label: 'Arşiv', count: dashboard?.archivedGoals?.length || 0, color: '#64748b' },
  ];

  const goalList = activeTab === 'active'
    ? dashboard?.activeGoals || []
    : activeTab === 'completed'
    ? dashboard?.completedGoals || []
    : dashboard?.archivedGoals || [];

  const lagGoals = dashboard?.activeGoals?.filter(g => g.isOnLag) || [];

  return (
    <MainLayout>
      <div className="space-y-8">
        {/* Header */}
        <div className="flex justify-between items-end">
          <section className="relative">
            <div className="absolute -left-10 -top-10 w-40 h-40 bg-teal-600/10 blur-[80px] rounded-full pointer-events-none"></div>
            <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2 relative z-10">HEDEFLER</h2>
            <div className="h-1.5 w-24 bg-teal-600 rounded-full shadow-[0_0_20px_rgba(13,148,136,0.6)]"></div>
          </section>
          <button
            onClick={() => setShowModal(true)}
            className="bg-indigo-600 hover:bg-indigo-500 text-white px-6 py-3 rounded-xl font-bold transition-all shadow-[0_0_15px_rgba(79,70,229,0.3)]"
          >
            + Yeni Hedef
          </button>
        </div>

        {/* Lag uyarısı */}
        {lagGoals.length > 0 && (
          <div className="bg-amber-500/10 border border-amber-500/30 rounded-2xl p-4 flex items-center gap-3">
            <span className="text-2xl">⚠️</span>
            <div>
              <div className="text-amber-400 font-bold text-sm">{lagGoals.length} hedef geride kalıyor</div>
              <div className="text-amber-400/60 text-xs mt-0.5">
                {lagGoals.map(g => g.title).join(', ')} — biraz daha ilerleme gerekiyor!
              </div>
            </div>
          </div>
        )}

        {/* KPI Özet Paneli */}
        {!loading && dashboard && (() => {
          const activeGoals = dashboard.activeGoals || [];
          const completedGoals = dashboard.completedGoals || [];
          const allMeasured = [...activeGoals, ...completedGoals];
          const avgProgress = dashboard.avgProgress ?? dashboard.AvgProgress ??
            (allMeasured.length > 0
              ? allMeasured.reduce((acc, g) => acc + (g.progressPercent ?? (g.targetValue > 0 ? Math.round((g.currentValue / g.targetValue) * 100) : 0)), 0) / allMeasured.length
              : 0);

          const kpis = [
            { label: "Aktif Hedef", value: dashboard.totalActiveGoals ?? activeGoals.length, icon: "📈", color: "from-indigo-600/20 to-indigo-500/5", border: "border-indigo-500/30", text: "text-indigo-400" },
            { label: "Bu Ay Tamamlanan", value: dashboard.totalCompletedThisMonth ?? completedGoals.length, icon: "✅", color: "from-emerald-600/20 to-emerald-500/5", border: "border-emerald-500/30", text: "text-emerald-400" },
            { label: "Ort. İlerleme", value: `%${Math.round(avgProgress)}`, icon: "📊", color: "from-amber-600/20 to-amber-500/5", border: "border-amber-500/30", text: "text-amber-400" },
          ];
          return (
            <div className="grid grid-cols-3 gap-3">
              {kpis.map(kpi => (
                <div key={kpi.label} className={`bg-gradient-to-br ${kpi.color} border ${kpi.border} rounded-2xl p-4 flex flex-col gap-1`}>
                  <span className="text-xl">{kpi.icon}</span>
                  <p className={`font-black text-2xl ${kpi.text}`}>{kpi.value}</p>
                  <p className="text-slate-400 text-xs font-semibold">{kpi.label}</p>
                </div>
              ))}
            </div>
          );
        })()}

        {/* Tabs */}
        <div className="flex gap-2">
          {tabs.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`px-4 py-2 rounded-xl text-xs font-black uppercase tracking-wider transition-all relative z-20 cursor-pointer ${
                activeTab === tab.id
                  ? 'text-white shadow-lg'
                  : 'bg-slate-900/60 text-slate-400 hover:text-white border border-slate-800'
              }`}
              style={activeTab === tab.id ? { background: `${tab.color}cc` } : {}}
            >
              {tab.label}
              {tab.count > 0 && (
                <span className="ml-1.5 text-[9px] px-1.5 py-0.5 rounded-full font-black"
                  style={{ background: 'rgba(255,255,255,0.15)' }}>
                  {tab.count}
                </span>
              )}
            </button>
          ))}
        </div>

        {/* Content */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {[1, 2, 3].map(i => (
              <div key={i} className="h-32 bg-slate-900/40 rounded-2xl animate-pulse border border-slate-800" />
            ))}
          </div>
        ) : goalList.length === 0 ? (
          <div className="bg-slate-900/40 rounded-2xl border border-dashed border-slate-700 p-12 text-center">
            <p className="text-4xl mb-4">🎯</p>
            <p className="text-slate-400 font-bold">
              {activeTab === 'active' ? 'Aktif hedef yok.' : activeTab === 'completed' ? 'Bu ay tamamlanan yok.' : 'Arşivlenmiş hedef yok.'}
            </p>
            {activeTab === 'active' && (
              <button
                onClick={() => setShowModal(true)}
                className="mt-4 px-6 py-2 rounded-xl bg-indigo-600 text-white font-bold text-sm hover:bg-indigo-500 transition-all"
              >
                İlk Hedefini Oluştur →
              </button>
            )}
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {goalList.map(goal => (
              <GoalCard key={goal.id} goal={goal} onUpdate={fetchDashboard} />
            ))}
          </div>
        )}
      </div>

      {/* Hedef Oluştur Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-slate-700 p-8 rounded-3xl w-full max-w-lg shadow-2xl max-h-[90vh] overflow-y-auto">
            <div className="flex justify-between items-center mb-6">
              <h3 className="text-2xl font-bold text-white">Yeni Hedef</h3>
              <button onClick={() => setShowModal(false)} className="text-slate-500 hover:text-white text-xl">✕</button>
            </div>

            <form onSubmit={handleCreateGoal} className="space-y-4">
              {/* Icon + Color seçici */}
              <div className="flex gap-4">
                <div className="flex-1">
                  <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-2 block">İkon</label>
                  <div className="flex flex-wrap gap-2">
                    {ICONS.map(ic => (
                      <button
                        type="button" key={ic}
                        className={`text-xl w-9 h-9 rounded-lg transition-all ${newGoal.icon === ic ? 'bg-indigo-600/40 ring-2 ring-indigo-500' : 'bg-slate-800 hover:bg-slate-700'}`}
                        onClick={() => setNewGoal(p => ({ ...p, icon: ic }))}
                      >{ic}</button>
                    ))}
                  </div>
                </div>
                <div>
                  <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-2 block">Renk</label>
                  <div className="flex flex-wrap gap-2">
                    {COLORS.map(c => (
                      <button
                        type="button" key={c}
                        className={`w-7 h-7 rounded-full transition-all ${newGoal.color === c ? 'ring-2 ring-white scale-110' : 'opacity-70 hover:opacity-100'}`}
                        style={{ background: c }}
                        onClick={() => setNewGoal(p => ({ ...p, color: c }))}
                      />
                    ))}
                  </div>
                </div>
              </div>

              <div>
                <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Hedef Başlığı *</label>
                <input required type="text" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none text-sm"
                  placeholder="Örn: 30 Günde 30 Kitap"
                  value={newGoal.title} onChange={e => setNewGoal(p => ({ ...p, title: e.target.value }))} />
              </div>

              <div>
                <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Neden yapıyorum? (Motivasyon)</label>
                <input type="text" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none text-sm"
                  placeholder="Bu hedef benim için ne ifade ediyor?"
                  value={newGoal.why} onChange={e => setNewGoal(p => ({ ...p, why: e.target.value }))} />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Hedef Değer</label>
                  <input type="number" min="1" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none text-sm"
                    value={newGoal.targetValue} onChange={e => setNewGoal(p => ({ ...p, targetValue: parseInt(e.target.value) }))} />
                </div>
                <div>
                  <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Birim</label>
                  <input type="text" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none text-sm"
                    placeholder="görev, km, sayfa..." 
                    value={newGoal.progressUnit} onChange={e => setNewGoal(p => ({ ...p, progressUnit: e.target.value }))} />
                </div>
              </div>

              <div className="grid grid-cols-1 gap-4">
                <div>
                  <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Süre (Gün)</label>
                  <input type="number" min="1" max="365" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none text-sm"
                    value={newGoal.durationDays}
                    onChange={e => setNewGoal(p => ({ ...p, durationDays: parseInt(e.target.value) }))} />
                </div>
              </div>

              {/* Otomatik takip */}
              <div className="flex gap-4">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" className="w-4 h-4 accent-indigo-600"
                    checked={newGoal.autoTrackByCategory}
                    onChange={e => setNewGoal(p => ({ ...p, autoTrackByCategory: e.target.checked }))} />
                  <span className="text-xs text-slate-300 font-bold">Kategori ile takip</span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" className="w-4 h-4 accent-indigo-600"
                    checked={newGoal.autoTrackByXP}
                    onChange={e => setNewGoal(p => ({ ...p, autoTrackByXP: e.target.checked }))} />
                  <span className="text-xs text-slate-300 font-bold">XP ile takip</span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" className="w-4 h-4 accent-indigo-600"
                    checked={newGoal.reminderEnabled}
                    onChange={e => setNewGoal(p => ({ ...p, reminderEnabled: e.target.checked }))} />
                  <span className="text-xs text-slate-300 font-bold">Hatırlatma</span>
                </label>
              </div>

              <div className="flex justify-end gap-3 mt-6">
                <button type="button" onClick={() => setShowModal(false)}
                  className="px-5 py-2.5 rounded-xl font-bold text-slate-400 hover:text-white transition-colors text-sm">
                  İptal
                </button>
                <button type="submit" disabled={submitting}
                  className="px-6 py-2.5 rounded-xl font-bold bg-teal-600 text-white hover:bg-teal-500 shadow-lg shadow-teal-900/50 text-sm transition-all disabled:opacity-50">
                  {submitting ? '⏳ Oluşturuluyor...' : '✨ Hedefi Oluştur'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </MainLayout>
  );
};

export default GoalsPage;
