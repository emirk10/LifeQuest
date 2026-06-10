import React, { useState, useEffect, useRef } from 'react';

const TaskTimer = ({ task, onComplete, onClose }) => {
  const [timeLeft, setTimeLeft] = useState(25 * 60); 
  const [isActive, setIsActive] = useState(false);
  const [isPaused, setIsPaused] = useState(false);
  const timerRef = useRef(null);

  useEffect(() => {
    if (isActive && !isPaused && timeLeft > 0) {
      timerRef.current = setInterval(() => {
        setTimeLeft((prev) => prev - 1);
      }, 1000);
    } else if (timeLeft === 0) {
      clearInterval(timerRef.current);
      
    } else {
      clearInterval(timerRef.current);
    }
    return () => clearInterval(timerRef.current);
  }, [isActive, isPaused, timeLeft]);

  const toggleTimer = () => {
    if (!isActive) {
      setIsActive(true);
    } else {
      setIsPaused(!isPaused);
    }
  };

  const resetTimer = () => {
    setIsActive(false);
    setIsPaused(false);
    setTimeLeft(25 * 60);
  };

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const progress = ((25 * 60 - timeLeft) / (25 * 60)) * 100;

  return (
    <div className="fixed inset-0 z-[60] bg-slate-950 flex flex-col items-center justify-center p-6 sm:p-12 animate-in fade-in zoom-in duration-300">
      <button 
        onClick={onClose}
        className="absolute top-8 right-8 text-slate-500 hover:text-white transition-colors p-3 rounded-full hover:bg-slate-900"
      >
        <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>

      <div className="max-w-2xl w-full text-center space-y-12">
        <div className="space-y-4">
          <p className="text-indigo-500 font-black tracking-[0.3em] uppercase text-xs">Şu an Odaklanılan Görev</p>
          <h2 className="text-3xl sm:text-5xl font-black text-white italic tracking-tighter uppercase leading-tight">
            {task?.title || "Odaklanma Zamanı"}
          </h2>
          <div className="flex justify-center gap-3">
             <span className="bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 px-3 py-1 rounded-full text-[10px] font-black uppercase">
               {task?.difficulty || "Medium"}
             </span>
             <span className="bg-slate-900 text-slate-400 px-3 py-1 rounded-full text-[10px] font-bold">
               +{task?.xp || 100} XP
             </span>
          </div>
        </div>

        {}
        <div className="relative w-64 h-64 sm:w-80 sm:h-80 mx-auto flex items-center justify-center">
          <svg className="absolute inset-0 w-full h-full -rotate-90" viewBox="0 0 100 100">
            <circle
              cx="50"
              cy="50"
              r="45"
              className="stroke-slate-900/50 fill-none"
              strokeWidth="4"
            />
            <circle
              cx="50"
              cy="50"
              r="45"
              className="stroke-indigo-600 fill-none transition-all duration-1000"
              strokeWidth="4"
              strokeDasharray="282.7"
              strokeDashoffset={282.7 - (282.7 * progress) / 100}
              style={{ strokeLinecap: 'round' }}
            />
          </svg>
          <div className="text-7xl sm:text-8xl font-black text-white tabular-nums tracking-tighter">
            {formatTime(timeLeft)}
          </div>
        </div>

        {}
        <div className="flex flex-col sm:flex-row items-center justify-center gap-6">
          <button
            onClick={toggleTimer}
            className={`w-full sm:w-48 py-5 rounded-[2rem] font-black uppercase tracking-widest text-sm transition-all shadow-2xl active:scale-95 ${
              !isActive || isPaused 
                ? "bg-indigo-600 hover:bg-indigo-500 text-white shadow-indigo-500/40" 
                : "bg-slate-800 hover:bg-slate-700 text-slate-300"
            }`}
          >
            {!isActive ? "Süreyi Başlat" : isPaused ? "Devam Et" : "Durdur"}
          </button>
          
          <button
            onClick={resetTimer}
            className="w-full sm:w-48 py-5 rounded-[2rem] bg-slate-900 hover:bg-slate-800 text-slate-500 hover:text-slate-300 font-black uppercase tracking-widest text-sm transition-all active:scale-95 border border-slate-800"
          >
            Sıfırla
          </button>
        </div>

        <div className="pt-12 border-t border-slate-900">
           <button
             onClick={() => onComplete(task.id)}
             className="text-emerald-500 hover:text-emerald-400 font-black uppercase tracking-widest text-xs flex items-center gap-2 mx-auto transition-all hover:scale-110"
           >
             <span className="text-xl">✅</span> Görevi Hemen Bitir
           </button>
        </div>
      </div>
    </div>
  );
};

export default TaskTimer;
