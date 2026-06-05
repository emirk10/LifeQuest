import React from "react";
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell
} from "recharts";

const CustomTooltip = ({ active, payload, label }) => {
  if (active && payload?.length) {
    return (
      <div className="bg-slate-800 border border-slate-700 rounded-xl px-3 py-2 text-xs shadow-xl">
        <p className="text-white font-bold mb-1">{label}</p>
        <p className="text-emerald-400">✅ {payload[0]?.value} tamamlandı</p>
      </div>
    );
  }
  return null;
};

const BehaviorInsightCard = ({ summary }) => {
  if (!summary) return null;

  const {
    weeklyProgress = [],
    completionRateLast30Days = 0,
    totalCompletedLast30Days = 0,
    totalPostponeCount = 0,
    abandonedTaskCount = 0,
    bestTimeOfDay = "Bilinmiyor",
    mostCompletedCategory,
    mostSkippedCategory,
    currentStreak = 0,
    longestStreak = 0,
    insightMessage,
    aiSystemInsights = [],
  } = summary;

  const completionPct = Math.round(completionRateLast30Days * 100);

  const barColors = weeklyProgress.map((d) =>
    d.completedCount >= 3
      ? "#10b981"
      : d.completedCount >= 1
      ? "#6366f1"
      : "#334155"
  );

  return (
    <div className="bg-gradient-to-br from-slate-900/80 to-slate-800/60 border border-slate-700/50 rounded-[2rem] p-6 space-y-6">
      {/* Başlık */}
      <div className="flex items-center justify-between">
        <h3 className="text-base font-black text-white tracking-wider uppercase italic flex items-center gap-2">
          <span className="text-lg">📊</span> Haftalık Davranış Özeti
        </h3>
        {currentStreak > 0 && (
          <span className="bg-orange-500/20 text-orange-400 text-xs font-bold px-3 py-1 rounded-full border border-orange-500/30">
            🔥 {currentStreak} günlük seri
          </span>
        )}
      </div>

      {/* Mini istatistik row */}
      <div className="grid grid-cols-3 gap-3">
        {[
          { label: "Tamamlandı", value: totalCompletedLast30Days, icon: "✅", color: "text-emerald-400" },
          { label: "Ertelendi", value: totalPostponeCount, icon: "⏸️", color: "text-yellow-400" },
          { label: "Vazgeçildi", value: abandonedTaskCount, icon: "❌", color: "text-red-400" },
        ].map(({ label, value, icon, color }) => (
          <div
            key={label}
            className="bg-slate-900/60 rounded-2xl p-3 text-center border border-slate-800"
          >
            <p className="text-lg mb-0.5">{icon}</p>
            <p className={`text-xl font-black ${color}`}>{value}</p>
            <p className="text-[10px] text-slate-500 mt-0.5">{label}</p>
          </div>
        ))}
      </div>

      {/* Mini bar chart — son 7 gün */}
      {weeklyProgress.length > 0 && (
        <div>
          <p className="text-xs text-slate-400 mb-2 font-medium">Son 7 Gün</p>
          <ResponsiveContainer width="100%" height={70}>
            <BarChart data={weeklyProgress} barSize={20}>
              <XAxis
                dataKey="dayName"
                tick={{ fill: "#94a3b8", fontSize: 10 }}
                axisLine={false}
                tickLine={false}
              />
              <YAxis hide />
              <Tooltip content={<CustomTooltip />} cursor={{ fill: "rgba(255,255,255,0.03)" }} />
              <Bar dataKey="completedCount" radius={[4, 4, 0, 0]}>
                {weeklyProgress.map((_, idx) => (
                  <Cell key={idx} fill={barColors[idx]} />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* 30 gün tamamlama oranı */}
      <div>
        <div className="flex justify-between items-center mb-2">
          <span className="text-xs text-slate-400 font-medium">30 Gün Tamamlama Oranı</span>
          <span
            className={`text-sm font-black ${
              completionPct >= 70
                ? "text-emerald-400"
                : completionPct >= 40
                ? "text-yellow-400"
                : "text-red-400"
            }`}
          >
            %{completionPct}
          </span>
        </div>
        <div className="h-2 bg-slate-800 rounded-full overflow-hidden">
          <div
            className={`h-full rounded-full transition-all duration-700 ${
              completionPct >= 70
                ? "bg-emerald-500"
                : completionPct >= 40
                ? "bg-yellow-500"
                : "bg-red-500"
            }`}
            style={{ width: `${completionPct}%` }}
          />
        </div>
      </div>

      {/* Kategori & zaman bilgisi */}
      <div className="grid grid-cols-2 gap-3 text-xs">
        {mostCompletedCategory && (
          <div className="bg-emerald-500/10 border border-emerald-500/20 rounded-2xl p-3">
            <p className="text-emerald-400 font-bold mb-0.5">🏆 En iyi kategori</p>
            <p className="text-white font-semibold">{mostCompletedCategory}</p>
          </div>
        )}
        {bestTimeOfDay !== "Bilinmiyor" && (
          <div className="bg-indigo-500/10 border border-indigo-500/20 rounded-2xl p-3">
            <p className="text-indigo-400 font-bold mb-0.5">⏰ En verimli zaman</p>
            <p className="text-white font-semibold">{bestTimeOfDay}</p>
          </div>
        )}
        {mostSkippedCategory && (
          <div className="bg-orange-500/10 border border-orange-500/20 rounded-2xl p-3 col-span-2">
            <p className="text-orange-400 font-bold mb-0.5">⚠️ Gelişim fırsatı</p>
            <p className="text-white font-semibold">
              {mostSkippedCategory} — bu kategoride küçük adımlar at
            </p>
          </div>
        )}
      </div>

      {/* En uzun seri */}
      {longestStreak > 0 && (
        <div className="text-center text-xs text-slate-500">
          En uzun serim: <span className="text-white font-bold">{longestStreak} gün</span>
        </div>
      )}

      {/* AI & Chatbot Kapsamlı Öğrenim Analizi */}
      {((aiSystemInsights && aiSystemInsights.length > 0) || insightMessage) && (
        <div className="bg-gradient-to-r from-indigo-950/40 to-slate-900/60 border border-indigo-500/20 rounded-[1.5rem] p-6 space-y-4 relative overflow-hidden">
          <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-500/5 blur-[50px] rounded-full"></div>
          
          <h4 className="text-xs font-black text-indigo-400 uppercase tracking-widest flex items-center gap-2">
            🤖 Asistan ve Sistem Öğrenim Raporu
          </h4>
          
          {aiSystemInsights && aiSystemInsights.length > 0 ? (
            <ul className="space-y-3 text-xs text-slate-300 leading-relaxed">
              {aiSystemInsights.map((insight, idx) => (
                <li key={idx} className="flex gap-2.5 items-start">
                  <span className="text-indigo-400 flex-shrink-0 mt-0.5">•</span>
                  <span>{insight}</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="text-xs text-indigo-200 leading-relaxed">
              🤖 {insightMessage}
            </p>
          )}
        </div>
      )}
    </div>
  );
};

export default BehaviorInsightCard;
