import React, { useState } from "react";

const DIFFICULTY_COLORS = {
  Easy: "text-emerald-400 bg-emerald-400/10 border-emerald-400/20",
  Medium: "text-amber-400 bg-amber-400/10 border-amber-400/20",
  Hard: "text-rose-400 bg-rose-400/10 border-rose-400/20",
};

const TaskItem = ({
  id,
  title,
  description,
  xp,
  difficulty,
  isCompleted,
  onComplete,
}) => {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showDesc, setShowDesc] = useState(false);

  const safeDifficulty = DIFFICULTY_COLORS[difficulty] ? difficulty : "Medium";

  const handleComplete = async () => {
    if (isSubmitting || isCompleted) return;
    setIsSubmitting(true);
    try {
      await onComplete(id);
    } catch (err) {
      console.error("Görev tamamlama hatası:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className={`rounded-2xl border transition-all duration-300 ${
        isCompleted
          ? "opacity-50 grayscale border-slate-800 bg-slate-900/20"
          : "border-slate-800 hover:border-indigo-500/40 bg-slate-900/40 hover:bg-slate-900/60 hover:shadow-lg hover:shadow-indigo-500/5"
      }`}
    >
      <div className="p-4 flex items-center justify-between gap-4">
        {/* Checkbox + Info */}
        <div
          className="flex items-center gap-4 flex-1 cursor-pointer min-w-0"
          onClick={() => description && setShowDesc(!showDesc)}
        >
          <div
            className={`w-8 h-8 rounded-full border-2 flex items-center justify-center flex-shrink-0 transition-all duration-300 ${
              isCompleted
                ? "bg-emerald-500 border-emerald-500 shadow-[0_0_10px_rgba(16,185,129,0.4)]"
                : "border-slate-700 hover:border-indigo-500"
            }`}
          >
            {isCompleted && (
              <svg className="w-4 h-4 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
              </svg>
            )}
          </div>

          <div className="min-w-0 flex-1">
            <h4
              className={`font-bold text-base truncate ${
                isCompleted ? "text-slate-400 line-through" : "text-slate-100"
              }`}
            >
              {title}
            </h4>
            <div className="flex gap-2 mt-1.5 flex-wrap items-center">
              <span
                className={`text-[10px] px-2 py-0.5 rounded-full border font-bold uppercase ${DIFFICULTY_COLORS[safeDifficulty]}`}
              >
                {difficulty || "Normal"}
              </span>
              <span className="text-[10px] text-indigo-400 font-bold uppercase tracking-wider">
                +{xp} XP
              </span>
              {description && (
                <span className="text-[10px] text-slate-600 hover:text-slate-400 transition-colors">
                  {showDesc ? "▲ Gizle" : "▼ Detay"}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Complete Button */}
        <button
          onClick={handleComplete}
          disabled={isCompleted || isSubmitting}
          className={`px-5 py-2.5 rounded-xl text-xs font-black uppercase tracking-wider transition-all flex-shrink-0 shadow-lg active:scale-95 ${
            isCompleted
              ? "bg-slate-800 text-slate-500 cursor-not-allowed shadow-none"
              : isSubmitting
              ? "bg-indigo-800 text-indigo-400 cursor-wait"
              : "bg-indigo-600 hover:bg-indigo-500 text-white shadow-indigo-500/25 hover:shadow-indigo-500/40"
          }`}
        >
          {isCompleted ? "✓ Tamamlandı" : isSubmitting ? "..." : "Bitir"}
        </button>
      </div>

      {/* Description Expandable */}
      {showDesc && description && (
        <div className="px-4 pb-4 border-t border-slate-800/50">
          <p className="text-slate-400 text-sm mt-3 leading-relaxed">{description}</p>
        </div>
      )}
    </div>
  );
};

export default TaskItem;