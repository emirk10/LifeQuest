import React, { useState } from "react";
import axiosClient from "../api/axiosClient";
import { getUserIdFromToken } from "../utils/authUtils";













const GoalRevisionModal = ({ isOpen, category, currentGoals = [], onClose, onUpdated }) => {
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);
  const [resultMsg, setResultMsg] = useState("");

  if (!isOpen) return null;

  const handleUpdate = async () => {
    setLoading(true);
    try {
      const userId = getUserIdFromToken();
      const res = await axiosClient.put(`/users/${userId}/goals`, {
        newGoals: [],
        removedGoals: [category],
      });
      setResultMsg("Hedefiniz güncellendi! Yeni görevler hazırlanıyor. 🎯");
      setDone(true);
      onUpdated?.(res.data?.updatedGoals || currentGoals.filter(g => g !== category));
    } catch {
      setResultMsg("Bir hata oluştu. Tekrar dene.");
    } finally {
      setLoading(false);
    }
  };

  const handleKeep = () => {
    setResultMsg("Harika! Bu hedefle devam ediyoruz. 💪");
    setDone(true);
    setTimeout(onClose, 2000);
  };

  return (
    <div className="fixed inset-0 bg-black/70 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div className="bg-slate-900 border border-slate-700/50 rounded-[2rem] shadow-2xl max-w-md w-full p-6 space-y-5 animate-fade-in">
        {}
        <div className="text-center">
          <span className="text-4xl block mb-2">🎯</span>
          <h2 className="text-xl font-black text-white">Hedef Revizyonu</h2>
          <p className="text-slate-400 text-sm mt-1">
            LifeQuest AI bir öneri hazırladı
          </p>
        </div>

        {}
        <div className="bg-amber-500/10 border border-amber-500/20 rounded-2xl p-4">
          <p className="text-amber-200 text-sm leading-relaxed">
            <span className="font-bold text-amber-400">'{category}'</span> kategorisini
            son 2 haftadır zorlanıyor gibisin. Bu hedefi güncellemek ister misin?
          </p>
        </div>

        {}
        <div>
          <p className="text-xs text-slate-500 mb-2 font-medium uppercase tracking-wider">
            Mevcut Hedeflerin
          </p>
          <div className="flex flex-wrap gap-2">
            {currentGoals.map((g) => (
              <span
                key={g}
                className={`px-3 py-1 rounded-full text-xs font-bold border transition-all ${
                  g.toLowerCase() === category.toLowerCase()
                    ? "bg-red-500/20 border-red-500/40 text-red-300 line-through"
                    : "bg-slate-800 border-slate-700 text-slate-300"
                }`}
              >
                {g}
              </span>
            ))}
          </div>
          {currentGoals.some(g => g.toLowerCase() === category.toLowerCase()) && (
            <p className="text-xs text-red-400 mt-2">
              ↑ <span className="font-semibold">{category}</span> kaldırılacak
            </p>
          )}
        </div>

        {}
        {done ? (
          <div className="bg-emerald-500/10 border border-emerald-500/20 rounded-2xl p-4 text-center">
            <p className="text-emerald-300 font-semibold text-sm">{resultMsg}</p>
          </div>
        ) : (
          <div className="flex gap-3">
            <button
              onClick={handleUpdate}
              disabled={loading}
              className="flex-1 bg-indigo-600 hover:bg-indigo-500 text-white font-bold py-3 rounded-2xl text-sm transition-all disabled:opacity-50 shadow-lg shadow-indigo-500/20"
            >
              {loading ? "Güncelleniyor..." : "✅ Evet, güncelle"}
            </button>
            <button
              onClick={handleKeep}
              disabled={loading}
              className="flex-1 bg-slate-800 hover:bg-slate-700 text-slate-300 font-bold py-3 rounded-2xl text-sm transition-all border border-slate-700"
            >
              💪 Hayır, devam et
            </button>
          </div>
        )}

        {}
        <button
          onClick={onClose}
          className="w-full text-xs text-slate-600 hover:text-slate-400 transition-colors"
        >
          Kapat
        </button>
      </div>
    </div>
  );
};

export default GoalRevisionModal;
