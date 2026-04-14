import React from "react";

const BadgeCard = ({
  name,
  description,
  iconPath,
  isEarned,
  requiredLevel,
}) => {
  const showIcon = iconPath && iconPath.length < 5;

  return (
    <div
      className={`relative p-6 rounded-3xl border transition-all duration-500 group ${
        isEarned
          ? "bg-slate-900 border-indigo-500/50 shadow-[0_0_20px_rgba(79,70,229,0.1)]"
          : "bg-slate-950 border-slate-800 opacity-60"
      }`}
    >
      {!isEarned && (
        <div className="absolute top-4 right-4 text-slate-600 text-sm">
          🔒{" "}
          <span className="text-[10px] font-bold uppercase tracking-tighter">
            LVL {requiredLevel}
          </span>
        </div>
      )}

      <div className="flex flex-col items-center text-center gap-4">
        <div
          className={`w-20 h-20 rounded-full flex items-center justify-center text-3xl mb-2 transition-transform duration-500 overflow-hidden relative ${
            isEarned
              ? "bg-indigo-600/20 shadow-[0_0_30px_rgba(79,70,229,0.3)] group-hover:scale-110 border border-indigo-500/30"
              : "bg-slate-800 grayscale border border-slate-700"
          }`}
        >
          {isEarned && (
            <div className="absolute inset-0 bg-indigo-500/10 blur-md"></div>
          )}

          {showIcon ? (
            <span className="relative z-10">{iconPath}</span>
          ) : (
            <span
              className={`relative z-10 text-3xl font-black font-mono ${
                isEarned ? "text-indigo-400" : "text-slate-600"
              }`}
            >
              {name ? name.charAt(0).toUpperCase() : "?"}
            </span>
          )}
        </div>

        <div>
          <h4
            className={`font-black tracking-tight mb-1 ${
              isEarned ? "text-white" : "text-slate-500"
            }`}
          >
            {name ? name.toUpperCase() : "ROZET"}
          </h4>
          <p className="text-xs text-slate-500 leading-relaxed px-2">
            {description}
          </p>
        </div>

        {isEarned && (
          <div className="mt-2 py-1 px-3 bg-indigo-500/10 border border-indigo-500/20 rounded-full">
            <span className="text-[10px] font-black text-indigo-400 uppercase tracking-widest">
              Kazanıldı
            </span>
          </div>
        )}
      </div>
    </div>
  );
};

export default BadgeCard;