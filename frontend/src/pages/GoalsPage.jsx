import React, { useState, useEffect } from 'react';
import MainLayout from '../layouts/MainLayout';
import goalService from '../services/goalService';

const GoalsPage = () => {
  const [goals, setGoals] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [newGoal, setNewGoal] = useState({
    title: '',
    description: '',
    type: 1, // Weekly
    targetValue: 10,
    bonusXP: 50
  });

  useEffect(() => {
    fetchGoals();
  }, []);

  const fetchGoals = async () => {
    try {
        setLoading(true);
        const data = await goalService.getGoals();
        setGoals(data);
    } catch (error) {
        console.error("Hedefler alınamadı:", error);
    } finally {
        setLoading(false);
    }
  };

  const handleCreateGoal = async (e) => {
    e.preventDefault();
    try {
        await goalService.createGoal({
            ...newGoal,
            startDate: new Date().toISOString(),
            endDate: new Date(new Date().setDate(new Date().getDate() + 7)).toISOString() // basit bitiş
        });
        setShowModal(false);
        fetchGoals();
    } catch (err) {
        console.error("Hedef oluşturulamadı", err);
    }
  };

  return (
    <MainLayout>
      <div className="space-y-8">
        <div className="flex justify-between items-end">
            <section className="relative">
                <div className="absolute -left-10 -top-10 w-40 h-40 bg-teal-600/10 blur-[80px] rounded-full"></div>
                <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2 relative z-10">HEDEFLER</h2>
                <div className="h-1.5 w-24 bg-teal-600 rounded-full shadow-[0_0_20px_rgba(13,148,136,0.6)]"></div>
            </section>

            <button 
                onClick={() => setShowModal(true)}
                className="bg-indigo-600 hover:bg-indigo-500 text-white px-6 py-3 rounded-xl font-bold transition-all shadow-[0_0_15px_rgba(79,70,229,0.3)]">
                + Yeni Hedef
            </button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {loading ? (
                <p className="text-slate-400">Yükleniyor...</p>
            ) : goals.length === 0 ? (
                <div className="col-span-2 bg-slate-900/50 p-10 border border-slate-800 rounded-2xl text-center text-slate-400">
                    Henüz aktif bir hedefin yok. Başlamak için yeni bir hedef oluştur!
                </div>
            ) : (
                goals.map(goal => {
                    const percent = Math.min(100, (goal.currentValue / goal.targetValue) * 100);
                    return (
                        <div key={goal.id} className="bg-slate-900/60 p-6 rounded-2xl border border-slate-700/50 hover:border-teal-500/50 transition-colors group">
                            <div className="flex justify-between items-start mb-4">
                                <div>
                                    <h3 className="text-xl font-bold text-slate-100">{goal.title}</h3>
                                    <p className="text-sm text-slate-400">{goal.description}</p>
                                </div>
                                <span className="bg-indigo-500/20 text-indigo-300 text-xs px-2 py-1 rounded-md font-bold">
                                    +{goal.bonusXP} XP
                                </span>
                            </div>

                            <div className="mt-6">
                                <div className="flex justify-between text-xs text-slate-400 mb-2 font-bold uppercase tracking-wider">
                                    <span>İlerleme</span>
                                    <span>{goal.currentValue} / {goal.targetValue}</span>
                                </div>
                                <div className="h-3 w-full bg-slate-800 rounded-full overflow-hidden">
                                     <div 
                                        className="h-full bg-teal-500 rounded-full group-hover:shadow-[0_0_10px_rgba(20,184,166,0.8)] transition-all duration-1000"
                                        style={{ width: `${percent}%` }}
                                     ></div>
                                </div>
                            </div>
                        </div>
                    );
                })
            )}
        </div>

        {/* Modal */}
        {showModal && (
            <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50">
                <div className="bg-slate-900 border border-slate-700 p-8 rounded-3xl w-full max-w-md">
                    <h3 className="text-2xl font-bold text-white mb-6">Yeni Hedef</h3>
                    <form onSubmit={handleCreateGoal} className="space-y-4">
                        <div>
                            <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Hedef Adı</label>
                            <input required type="text" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none" value={newGoal.title} onChange={e => setNewGoal({...newGoal, title: e.target.value})} />
                        </div>
                        <div>
                            <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Açıklama</label>
                            <input type="text" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none" value={newGoal.description} onChange={e => setNewGoal({...newGoal, description: e.target.value})} />
                        </div>
                        <div className="flex gap-4">
                             <div className="flex-1">
                                <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Hedef Sayı</label>
                                <input type="number" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none" value={newGoal.targetValue} onChange={e => setNewGoal({...newGoal, targetValue: parseInt(e.target.value)})} />
                            </div>
                            <div className="flex-1">
                                <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Ödül (XP)</label>
                                <input type="number" className="w-full bg-slate-800 border-none rounded-xl text-white p-3 focus:ring-2 focus:ring-teal-500 outline-none" value={newGoal.bonusXP} onChange={e => setNewGoal({...newGoal, bonusXP: parseInt(e.target.value)})} />
                            </div>
                        </div>

                        <div className="flex justify-end gap-3 mt-8">
                            <button type="button" onClick={() => setShowModal(false)} className="px-5 py-2.5 rounded-xl font-bold text-slate-400 hover:text-white transition-colors">İptal</button>
                            <button type="submit" className="px-5 py-2.5 rounded-xl font-bold bg-teal-600 text-white hover:bg-teal-500 shadow-lg shadow-teal-900/50">Oluştur</button>
                        </div>
                    </form>
                </div>
            </div>
        )}
      </div>
    </MainLayout>
  );
};

export default GoalsPage;
