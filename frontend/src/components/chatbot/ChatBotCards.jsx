
import React from "react";

// ─── Renk paleti yardımcıları ─────────────────────────────────────────────────
const difficultyColor = (d) => ({
  Easy: "text-emerald-400 bg-emerald-500/10 border-emerald-500/30",
  Medium: "text-amber-400 bg-amber-500/10 border-amber-500/30",
  Hard: "text-rose-400 bg-rose-500/10 border-rose-500/30",
})[d] || "text-slate-400 bg-slate-700/30 border-slate-600/30";

const moodModeLabel = (m) => ({
  0: { label: "Normal", icon: "😊", color: "text-emerald-400" },
  1: { label: "Hafif Mod", icon: "🌿", color: "text-sky-400" },
  2: { label: "Tatil Modu", icon: "🏖️", color: "text-purple-400" },
})[m] || { label: "Belirsiz", icon: "❓", color: "text-slate-400" };

const energyBar = (level) => {
  const pct = Math.round((level / 10) * 100);
  const col = level >= 7 ? "bg-emerald-500" : level >= 4 ? "bg-amber-500" : "bg-rose-500";
  return { pct, col };
};

// ─── Görev Kartı ─────────────────────────────────────────────────────────────
export const TaskCard = ({ task, onAction, actionLabel, actionIcon, actionColor = "indigo" }) => {
  const colors = {
    indigo: "bg-indigo-600 hover:bg-indigo-500",
    emerald: "bg-emerald-600 hover:bg-emerald-500",
    amber: "bg-amber-600 hover:bg-amber-500",
  };
  return (
    <div className="bg-slate-800/80 rounded-xl border border-slate-700/50 p-3 flex flex-col gap-2">
      <div className="flex items-start justify-between gap-2">
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold text-white leading-tight truncate">{task.title}</p>
          <p className="text-[11px] text-slate-400 mt-0.5">
            {task.categoryIcon || "🎯"} {task.categoryName || "Genel"}
          </p>
        </div>
        <span className="shrink-0 text-xs font-bold text-amber-400 bg-amber-500/10 px-2 py-0.5 rounded-full">
          +{task.baseXP || task.earnedXp || task.xp || 0} XP
        </span>
      </div>
      <div className="flex items-center gap-2 flex-wrap">
        {task.difficulty && (
          <span className={`text-[10px] font-bold px-2 py-0.5 rounded-full border ${difficultyColor(task.difficulty)}`}>
            {task.difficulty === "Easy" ? "🌱 Kolay" : task.difficulty === "Medium" ? "⚡ Orta" : "🔥 Zor"}
          </span>
        )}
        {task.estimatedMinutes > 0 && (
          <span className="text-[10px] text-slate-400">⏱️ {task.estimatedMinutes} dk</span>
        )}
        {task.status && (
          <span className="text-[10px] text-slate-500">{task.status}</span>
        )}
      </div>
      {task.whyThisTask && (
        <p className="text-[11px] text-indigo-300 italic">"{ task.whyThisTask}"</p>
      )}
      {onAction && (
        <button
          onClick={() => onAction(task)}
          className={`w-full text-xs font-bold text-white py-2 rounded-lg transition-colors ${colors[actionColor]}`}
        >
          {actionIcon} {actionLabel}
        </button>
      )}
    </div>
  );
};

// ─── Tamamlanan Görev Kartı ───────────────────────────────────────────────────
export const CompletedTaskCard = ({ task }) => (
  <div className="bg-emerald-500/5 rounded-xl border border-emerald-500/20 p-3 flex items-center gap-3">
    <div className="w-8 h-8 bg-emerald-500/20 rounded-lg flex items-center justify-center shrink-0">
      <span className="text-sm">✅</span>
    </div>
    <div className="flex-1 min-w-0">
      <p className="text-sm font-semibold text-white truncate">{task.title}</p>
      <p className="text-[11px] text-slate-400">{task.categoryName || "Genel"}</p>
    </div>
    <span className="text-xs font-bold text-emerald-400 shrink-0">+{task.earnedXp || 0} XP</span>
  </div>
);

// ─── Rozet Kartı ─────────────────────────────────────────────────────────────
export const BadgeCard = ({ badge }) => (
  <div className={`rounded-xl border p-3 flex items-center gap-3 ${
    badge.isUnlocked
      ? "bg-amber-500/10 border-amber-500/30"
      : "bg-slate-800/50 border-slate-700/30 opacity-50"
  }`}>
    <div className="w-10 h-10 rounded-lg flex items-center justify-center text-xl shrink-0 bg-slate-700/50">
      {badge.iconPath || badge.icon || "🏆"}
    </div>
    <div className="flex-1 min-w-0">
      <p className="text-sm font-semibold text-white truncate">{badge.name}</p>
      <p className="text-[11px] text-slate-400 truncate">{badge.description}</p>
    </div>
    {badge.isUnlocked && (
      <span className="text-[10px] font-bold text-amber-400 bg-amber-500/10 px-2 py-0.5 rounded-full shrink-0">
        ✓ Kazanıldı
      </span>
    )}
  </div>
);

// ─── Profil Kartı ─────────────────────────────────────────────────────────────
export const ProfileCard = ({ profile }) => {
  const level = Math.floor(profile?.generalLevel || 1);
  const xpProgress = ((profile?.generalLevel || 1) % 1) * 100;

  return (
    <div className="bg-slate-800/80 rounded-xl border border-slate-700/50 p-4 space-y-3">
      <div className="flex items-center gap-3">
        <div className="w-12 h-12 bg-indigo-500/20 rounded-full flex items-center justify-center text-2xl border-2 border-indigo-500/40">
          {profile?.profilePicture || "👤"}
        </div>
        <div>
          <p className="text-white font-bold">{profile?.userName || "Kullanıcı"}</p>
          <p className="text-indigo-300 text-xs">Seviye {level} · {profile?.totalXp?.toLocaleString() || 0} XP</p>
        </div>
      </div>
      {/* XP Progress */}
      <div>
        <div className="flex justify-between text-[10px] text-slate-400 mb-1">
          <span>Seviye {level}</span>
          <span>Seviye {level + 1}</span>
        </div>
        <div className="h-2 bg-slate-700 rounded-full overflow-hidden">
          <div
            className="h-full bg-gradient-to-r from-indigo-500 to-purple-500 rounded-full transition-all"
            style={{ width: `${xpProgress}%` }}
          />
        </div>
      </div>
      {/* Streak */}
      {profile?.currentStreak > 0 && (
        <div className="flex items-center gap-2 bg-orange-500/10 rounded-lg px-3 py-2 border border-orange-500/20">
          <span className="text-lg">🔥</span>
          <div>
            <p className="text-orange-400 text-xs font-bold">{profile.currentStreak} Gün Seri</p>
            <p className="text-slate-400 text-[10px]">Devam et, bitmeden dur!</p>
          </div>
        </div>
      )}
      {/* Stats */}
      <div className="grid grid-cols-2 gap-2">
        {[
          { 
            label: "Tamamlanan", 
            value: profile?.completedTasksCount ?? (Array.isArray(profile?.completedTasks) ? profile.completedTasks.length : 0), 
            icon: "✅" 
          },
          { 
            label: "Terk Edilen", 
            value: profile?.abandonedTasksCount ?? (Array.isArray(profile?.abandonedTasks) ? profile.abandonedTasks.length : 0), 
            icon: "❌" 
          },
        ].map(s => (
          <div key={s.label} className="bg-slate-700/40 rounded-lg p-2 text-center">
            <p className="text-lg">{s.icon}</p>
            <p className="text-white font-bold text-sm">{s.value}</p>
            <p className="text-slate-400 text-[10px]">{s.label}</p>
          </div>
        ))}
      </div>
    </div>
  );
};

// ─── Mood Özet Kartı ─────────────────────────────────────────────────────────
export const MoodSavedCard = ({ mood, message }) => {
  const modeInfo = moodModeLabel(mood?.moodMode);
  const energy = energyBar(mood?.energyLevel || 5);
  return (
    <div className="bg-gradient-to-br from-purple-500/10 to-indigo-500/10 rounded-xl border border-purple-500/30 p-4 space-y-3">
      <div className="flex items-center gap-2">
        <span className="text-2xl">{modeInfo.icon}</span>
        <div>
          <p className="text-white font-bold text-sm">Ruh Halin Kaydedildi! ✅</p>
          <p className={`text-xs ${modeInfo.color}`}>{modeInfo.label}</p>
        </div>
      </div>
      <div>
        <div className="flex justify-between text-[10px] text-slate-400 mb-1">
          <span>Enerji Seviyesi</span>
          <span>{mood?.energyLevel || 5}/10</span>
        </div>
        <div className="h-1.5 bg-slate-700 rounded-full overflow-hidden">
          <div className={`h-full ${energy.col} rounded-full`} style={{ width: `${energy.pct}%` }} />
        </div>
      </div>
      {mood?.availableMinutes && (
        <p className="text-[11px] text-slate-400">⏰ Bugün {mood.availableMinutes} dakikan var</p>
      )}
      <p className="text-[11px] text-purple-300 italic">{message || "Görevlerin ruh haline göre ayarlandı!"}</p>
    </div>
  );
};

// ─── Motivasyon Kartı ─────────────────────────────────────────────────────────
export const MotivationCard = ({ message, isAI, mood }) => (
  <div className="bg-gradient-to-br from-indigo-600/20 to-purple-600/20 rounded-xl border border-indigo-500/30 p-4 space-y-2">
    <div className="flex items-center gap-2 mb-1">
      <span className="text-xl">💪</span>
      <p className="text-indigo-300 text-[10px] font-bold uppercase tracking-wider">
        {isAI ? "AI Motivasyon" : "Motivasyon"}
      </p>
    </div>
    <p className="text-white text-sm leading-relaxed">{message}</p>
  </div>
);

// ─── Haftalık Rapor ───────────────────────────────────────────────────────────
export const WeeklyStatsCard = ({ report }) => {
  if (!report) return (
    <div className="bg-slate-800/50 rounded-xl border border-slate-700/50 p-4 text-center">
      <p className="text-slate-400 text-sm">Henüz haftalık rapor yok.</p>
      <p className="text-slate-500 text-xs mt-1">Görevleri tamamladıkça rapor oluşur!</p>
    </div>
  );
  const perf = Math.round(report.overallPerformanceScore || 0);
  const perfColor = perf >= 70 ? "text-emerald-400" : perf >= 40 ? "text-amber-400" : "text-rose-400";
  
  const compCount = Array.isArray(report.tasksCompleted) ? report.tasksCompleted.length : (report.tasksCompleted || 0);
  const failCount = Array.isArray(report.tasksFailed) ? report.tasksFailed.length : (report.tasksFailed || 0);

  return (
    <div className="bg-slate-800/80 rounded-xl border border-slate-700/50 p-4 space-y-3">
      <p className="text-white font-bold text-sm">📊 Bu Haftaki Performans</p>
      <div className="grid grid-cols-3 gap-2 text-center">
        {[
          { label: "Tamamlanan", value: compCount, icon: "✅", color: "text-emerald-400" },
          { label: "Başarısız", value: failCount, icon: "❌", color: "text-rose-400" },
          { label: "Toplam XP", value: report.totalXPEarned, icon: "⭐", color: "text-amber-400" },
        ].map(s => (
          <div key={s.label} className="bg-slate-700/40 rounded-lg p-2">
            <p className="text-lg">{s.icon}</p>
            <p className={`font-bold text-sm ${s.color}`}>{s.value}</p>
            <p className="text-slate-400 text-[10px]">{s.label}</p>
          </div>
        ))}
      </div>
      <div>
        <div className="flex justify-between text-[10px] mb-1">
          <span className="text-slate-400">Performans Skoru</span>
          <span className={`font-bold ${perfColor}`}>%{perf}</span>
        </div>
        <div className="h-2 bg-slate-700 rounded-full overflow-hidden">
          <div
            className={`h-full rounded-full ${perf >= 70 ? "bg-emerald-500" : perf >= 40 ? "bg-amber-500" : "bg-rose-500"}`}
            style={{ width: `${perf}%` }}
          />
        </div>
      </div>
      {report.aiInsight && (
        <p className="text-[11px] text-indigo-300 italic">💡 {report.aiInsight}</p>
      )}
    </div>
  );
};

// ─── XP Grafik (Mini Bar Chart) ───────────────────────────────────────────────
export const XPBreakdownChart = ({ breakdown }) => {
  if (!breakdown || breakdown.length === 0) return (
    <p className="text-slate-400 text-sm text-center py-2">Henüz XP verisi yok.</p>
  );
  const maxXP = Math.max(...breakdown.map(d => d.XPKazanci || 0), 1);
  const days = ["Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz"];
  return (
    <div className="bg-slate-800/80 rounded-xl border border-slate-700/50 p-4">
      <p className="text-white font-bold text-sm mb-3">🔥 Son 7 Gün XP Kazanımı</p>
      <div className="flex items-end gap-1.5 h-24">
        {breakdown.map((d, i) => {
          const pct = ((d.XPKazanci || 0) / maxXP) * 100;
          const isToday = i === breakdown.length - 1;
          return (
            <div key={i} className="flex-1 flex flex-col items-center gap-1">
              <span className="text-[9px] text-slate-400">{d.XPKazanci || 0}</span>
              <div className="w-full flex flex-col justify-end" style={{ height: "60px" }}>
                <div
                  className={`w-full rounded-t transition-all ${isToday ? "bg-indigo-500" : "bg-slate-600"}`}
                  style={{ height: `${Math.max(pct, 3)}%`, minHeight: "3px" }}
                />
              </div>
              <span className={`text-[9px] ${isToday ? "text-indigo-400 font-bold" : "text-slate-500"}`}>
                {new Date(d.date).toLocaleDateString("tr-TR", { weekday: "short" })}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
};

// ─── Genel İstatistik Kartı ───────────────────────────────────────────────────
export const StatsCard = ({ stats }) => {
  if (!stats) return null;

  const totalTasksCount = Array.isArray(stats.totalTasks) ? stats.totalTasks.length : (stats.totalTasks ?? "—");
  const badgesEarnedCount = Array.isArray(stats.badgesEarned) ? stats.badgesEarned.length : (stats.badgesEarned ?? "—");

  return (
    <div className="bg-slate-800/80 rounded-xl border border-slate-700/50 p-4 space-y-2">
      <p className="text-white font-bold text-sm">📊 Genel İstatistikler</p>
      <div className="grid grid-cols-2 gap-2">
        {[
          { label: "Toplam Görev", value: totalTasksCount, icon: "📋" },
          { label: "Başarı Oranı", value: stats.successRate ? `%${Math.round(stats.successRate)}` : "—", icon: "🎯" },
          { label: "En Uzun Seri", value: stats.longestStreak ?? "—", icon: "🔥" },
          { label: "Kazanılan Rozet", value: badgesEarnedCount, icon: "🏆" },
        ].map(s => (
          <div key={s.label} className="bg-slate-700/40 rounded-lg p-2.5 flex items-center gap-2">
            <span className="text-base">{s.icon}</span>
            <div>
              <p className="text-white font-bold text-sm">{s.value}</p>
              <p className="text-slate-400 text-[10px]">{s.label}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// ─── Hedef Kartı ─────────────────────────────────────────────────────────────
export const GoalCard = ({ goal }) => {
  const progress = Math.min(Math.round(((goal.currentValue || 0) / (goal.targetValue || 1)) * 100), 100);
  return (
    <div className="bg-slate-800/80 rounded-xl border border-slate-700/50 p-3 space-y-2">
      <div className="flex items-center justify-between">
        <p className="text-white font-semibold text-sm truncate">{goal.title}</p>
        <span className="text-[10px] text-indigo-300 bg-indigo-500/10 px-2 py-0.5 rounded-full shrink-0">
          {goal.category}
        </span>
      </div>
      <div>
        <div className="flex justify-between text-[10px] text-slate-400 mb-1">
          <span>{goal.currentValue || 0} / {goal.targetValue || 100}</span>
          <span className="font-bold text-indigo-300">%{progress}</span>
        </div>
        <div className="h-1.5 bg-slate-700 rounded-full overflow-hidden">
          <div
            className="h-full bg-gradient-to-r from-indigo-500 to-purple-500 rounded-full"
            style={{ width: `${progress}%` }}
          />
        </div>
      </div>
    </div>
  );
};

// ─── Hedef Dashboard Özet ─────────────────────────────────────────────────────
export const GoalsDashboardCard = ({ dashboard }) => {
  if (!dashboard) return null;
  
  const activeCount = Array.isArray(dashboard.activeGoals) ? dashboard.activeGoals.length : (dashboard.activeGoals || 0);
  const completedCount = Array.isArray(dashboard.completedGoals) ? dashboard.completedGoals.length : (dashboard.completedGoals || 0);
  const progress = dashboard.avgProgress || dashboard.AvgProgress || 0;

  return (
    <div className="bg-slate-800/80 rounded-xl border border-slate-700/50 p-4 space-y-3">
      <p className="text-white font-bold text-sm">🎯 Hedef Özeti</p>
      <div className="grid grid-cols-3 gap-2 text-center">
        {[
          { label: "Aktif", value: activeCount, icon: "📈", color: "text-indigo-400" },
          { label: "Tamamlanan", value: completedCount, icon: "✅", color: "text-emerald-400" },
          { label: "Ort. İlerleme", value: `%${Math.round(progress)}`, icon: "📊", color: "text-amber-400" },
        ].map(s => (
          <div key={s.label} className="bg-slate-700/40 rounded-lg p-2">
            <p className="text-base">{s.icon}</p>
            <p className={`font-bold text-sm ${s.color}`}>{s.value}</p>
            <p className="text-slate-400 text-[10px]">{s.label}</p>
          </div>
        ))}
      </div>
    </div>
  );
};
