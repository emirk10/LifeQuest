import React, { useState } from 'react';

const ReflectionEditor = ({ minWords, onTextChange, taskCategory }) => {
  const [text, setText] = useState('');

  const wordCount = text.trim() === '' ? 0 : text.trim().split(/\s+/).length;
  const isEnough = wordCount >= minWords;
  const progress = Math.min((wordCount / minWords) * 100, 100);

  const handleChange = (e) => {
    const value = e.target.value;
    setText(value);
    onTextChange(value);
  };

  return (
    <div className="bg-slate-900/60 p-5 rounded-2xl border border-slate-800 mt-3">
      <div className="flex items-center justify-between mb-3">
        <p className="text-slate-400 text-xs font-bold">Yansima Yazisi</p>
        <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${
          isEnough
            ? 'text-emerald-400 bg-emerald-400/10'
            : 'text-amber-400 bg-amber-400/10'
        }`}>
          {wordCount}/{minWords} kelime
        </span>
      </div>

      <textarea
        value={text}
        onChange={handleChange}
        placeholder={`Bu gorev hakkindaki deneyimlerinizi yazin... (min ${minWords} kelime)`}
        className="w-full h-28 bg-slate-800/60 border border-slate-700 rounded-xl p-3 text-slate-200 text-sm resize-none focus:outline-none focus:border-indigo-500 transition-colors placeholder:text-slate-600"
      />

      {/* Progress bar */}
      <div className="w-full h-1.5 bg-slate-800 rounded-full mt-2 overflow-hidden">
        <div
          className={`h-full rounded-full transition-all duration-500 ${
            isEnough ? 'bg-emerald-500' : 'bg-indigo-500'
          }`}
          style={{ width: `${progress}%` }}
        />
      </div>

      {isEnough && (
        <p className="text-emerald-400 text-[10px] mt-2 font-bold">
          Minimum kelime sayisina ulasildi
        </p>
      )}
    </div>
  );
};

export default ReflectionEditor;
