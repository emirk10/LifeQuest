import { useState } from "react";

/**
 * MiniTaskCard — SP-02: Chatbot içinde görev önerisi için kompakt kart.
 */
const MiniTaskCard = ({ task, onSelect, isSelected: initialSelected = false }) => {
  const [selected, setSelected] = useState(initialSelected);
  const [loading, setLoading] = useState(false);

  const handleSelect = async () => {
    if (selected || loading) return;
    setLoading(true);
    try {
      await onSelect?.(task.taskId);
      setSelected(true);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={`rounded-xl p-3 border transition-all ${
      selected
        ? "bg-emerald-500/10 border-emerald-500/30"
        : "bg-slate-900/60 border-slate-700/50 hover:border-indigo-500/40"
    }`}>
      <div className="flex items-start justify-between gap-2">
        <div className="flex-1 min-w-0">
          <h4 className="text-white font-bold text-xs leading-tight truncate">{task.title}</h4>
          <div className="flex items-center gap-2 mt-1">
            <span className="text-[10px] bg-indigo-500/15 text-indigo-300 px-1.5 py-0.5 rounded-full">
              {task.categoryName}
            </span>
            {task.estimatedMinutes && (
              <span className="text-[10px] text-slate-400">⏱️ {task.estimatedMinutes}dk</span>
            )}
            <span className="text-[10px] text-amber-400 font-bold">+{task.baseXP} XP</span>
          </div>
          {task.whyThisTask && (
            <p className="text-[10px] text-slate-500 mt-1 leading-tight line-clamp-1 italic">
              💡 {task.whyThisTask}
            </p>
          )}
        </div>
        <button
          onClick={handleSelect}
          disabled={selected || loading}
          className={`shrink-0 text-[11px] font-bold px-2.5 py-1.5 rounded-lg transition-all ${
            selected
              ? "bg-emerald-500/20 text-emerald-400 cursor-default"
              : "bg-indigo-600 text-white hover:bg-indigo-500 active:scale-95"
          }`}
        >
          {loading ? "⌛" : selected ? "✓ Seçildi" : "Seç"}
        </button>
      </div>
    </div>
  );
};

export default MiniTaskCard;
