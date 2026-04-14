import React, { useState, useEffect, useRef } from 'react';

const TaskTimer = ({ minimumMinutes, onTimerComplete, taskTitle }) => {
  const [seconds, setSeconds] = useState(0);
  const [isRunning, setIsRunning] = useState(false);
  const [isCompleted, setIsCompleted] = useState(false);
  const intervalRef = useRef(null);
  const requiredSeconds = minimumMinutes * 60;

  useEffect(() => {
    if (isRunning) {
      intervalRef.current = setInterval(() => {
        setSeconds(prev => {
          const next = prev + 1;
          if (next >= requiredSeconds && !isCompleted) {
            setIsCompleted(true);
            onTimerComplete(next);
          }
          return next;
        });
      }, 1000);
    }
    return () => clearInterval(intervalRef.current);
  }, [isRunning, requiredSeconds, isCompleted, onTimerComplete]);

  const formatTime = (s) => {
    const mins = Math.floor(s / 60);
    const secs = s % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const progress = Math.min((seconds / requiredSeconds) * 100, 100);
  const circumference = 2 * Math.PI * 45;
  const strokeDashoffset = circumference - (progress / 100) * circumference;

  return (
    <div className="bg-slate-900/60 p-5 rounded-2xl border border-slate-800 mt-3">
      <p className="text-slate-500 text-xs mb-4 text-center">
        Minimum {minimumMinutes} dakika gerekli
      </p>

      <div className="relative w-28 h-28 mx-auto mb-4">
        <svg className="w-full h-full -rotate-90" viewBox="0 0 100 100">
          <circle cx="50" cy="50" r="45" fill="none"
                  stroke="#1e293b" strokeWidth="6"/>
          <circle cx="50" cy="50" r="45" fill="none"
                  stroke={isCompleted ? "#10b981" : "#6366f1"}
                  strokeWidth="6"
                  strokeDasharray={circumference}
                  strokeDashoffset={strokeDashoffset}
                  strokeLinecap="round"
                  style={{ transition: 'stroke-dashoffset 1s ease' }}/>
        </svg>
        <div className="absolute inset-0 flex items-center justify-center">
          <span className="text-xl font-black text-white font-mono">
            {formatTime(seconds)}
          </span>
        </div>
      </div>

      <button
        onClick={() => setIsRunning(!isRunning)}
        disabled={isCompleted}
        className={`w-full py-2.5 rounded-xl text-xs font-bold uppercase tracking-wider transition-all ${
          isCompleted
            ? 'bg-emerald-600 text-white cursor-default'
            : isRunning
            ? 'bg-amber-500 hover:bg-amber-400 text-black'
            : 'bg-indigo-600 hover:bg-indigo-500 text-white'
        }`}
      >
        {isCompleted ? 'Tamamlandi' : isRunning ? 'Duraklat' : 'Baslat'}
      </button>
    </div>
  );
};

export default TaskTimer;
