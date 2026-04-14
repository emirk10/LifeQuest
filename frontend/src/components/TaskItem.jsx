import React, { useState } from "react";
import TaskTimer from "./TaskTimer";
import PhotoUpload from "./PhotoUpload";
import ReflectionEditor from "./ReflectionEditor";

const VERIFICATION_LABELS = {
  0: null,          // None
  1: "Fotograf",    // Photo
  2: "Yansima",     // Reflection
  3: "Zamanlayici", // Timer
  4: "Quiz",        // Quiz
  5: "Foto+Yansima" // PhotoAndReflection
};

const TaskItem = ({
  id,
  title,
  xp,
  difficulty,
  isCompleted,
  verificationType = 0,
  verificationHint,
  minReflectionWords = 10,
  minTimerMinutes = 5,
  onComplete,
}) => {
  const [expanded, setExpanded] = useState(false);
  const [photoBase64, setPhotoBase64] = useState(null);
  const [reflectionText, setReflectionText] = useState('');
  const [timerSeconds, setTimerSeconds] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const difficultyColors = {
    Easy: "text-emerald-400 bg-emerald-400/10 border-emerald-400/20",
    Medium: "text-amber-400 bg-amber-400/10 border-amber-400/20",
    Hard: "text-rose-400 bg-rose-400/10 border-rose-400/20",
  };

  const safeDifficulty = difficultyColors[difficulty] ? difficulty : "Medium";
  const verLabel = VERIFICATION_LABELS[verificationType];
  const needsVerification = verificationType > 0;

  const handleComplete = async () => {
    if (isSubmitting) return;
    setIsSubmitting(true);

    try {
      const body = {};
      if (verificationType === 1 || verificationType === 5) body.photoBase64 = photoBase64;
      if (verificationType === 2 || verificationType === 5) body.reflectionText = reflectionText;
      if (verificationType === 3) body.timerSeconds = timerSeconds;

      await onComplete(id, body);
    } catch (err) {
      console.error("Gorev tamamlama hatasi:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const canSubmit = () => {
    if (isCompleted) return false;
    switch (verificationType) {
      case 1: return !!photoBase64;                                  // Photo
      case 2: return reflectionText.trim().split(/\s+/).length >= minReflectionWords; // Reflection
      case 3: return timerSeconds !== null && timerSeconds >= minTimerMinutes * 60;   // Timer
      case 5: return !!photoBase64 && reflectionText.trim().split(/\s+/).length >= minReflectionWords; // PhotoAndReflection
      default: return true; // None, Quiz
    }
  };

  return (
    <div
      className={`rounded-2xl border bg-slate-900/40 transition-all ${
        isCompleted
          ? "opacity-50 grayscale border-slate-800"
          : "border-slate-800 hover:border-slate-700"
      }`}
    >
      {/* Header */}
      <div className="p-4 flex items-center justify-between group">
        <div className="flex items-center gap-4 flex-1" onClick={() => needsVerification && !isCompleted && setExpanded(!expanded)} style={{ cursor: needsVerification && !isCompleted ? 'pointer' : 'default' }}>
          <div
            className={`w-8 h-8 rounded-full border-2 flex items-center justify-center transition-colors flex-shrink-0 ${
              isCompleted
                ? "bg-emerald-500 border-emerald-500"
                : "border-slate-700 group-hover:border-indigo-500"
            }`}
          >
            {isCompleted && <span className="text-white font-bold text-sm">&#10003;</span>}
          </div>

          <div className="min-w-0">
            <h4 className={`font-bold text-base ${isCompleted ? "text-slate-400 line-through" : "text-slate-100"}`}>
              {title}
            </h4>
            <div className="flex gap-2 mt-1.5 flex-wrap">
              <span className={`text-[10px] px-2 py-0.5 rounded-full border font-bold uppercase ${difficultyColors[safeDifficulty]}`}>
                {difficulty || "Normal"}
              </span>
              <span className="text-[10px] text-indigo-400 font-bold uppercase tracking-wider flex items-center gap-1">
                +{xp} XP
              </span>
              {verLabel && (
                <span className="text-[10px] px-2 py-0.5 rounded-full border font-bold uppercase text-violet-400 bg-violet-400/10 border-violet-400/20">
                  {verLabel}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Bitir butonu - dogrulama gerektirmeyenler icin or expanded panel */}
        {!needsVerification && (
          <button
            onClick={() => onComplete(id, {})}
            disabled={isCompleted}
            className={`px-5 py-2.5 rounded-xl text-xs font-black uppercase tracking-wider transition-all shadow-lg flex-shrink-0 ${
              isCompleted
                ? "bg-slate-800 text-slate-500 cursor-not-allowed shadow-none"
                : "bg-indigo-600 hover:bg-indigo-500 text-white shadow-indigo-500/20 active:scale-95"
            }`}
          >
            {isCompleted ? "Tamamlandi" : "Bitir"}
          </button>
        )}

        {needsVerification && !isCompleted && (
          <button
            onClick={() => setExpanded(!expanded)}
            className="text-slate-500 hover:text-slate-300 transition-colors flex-shrink-0 ml-2"
          >
            <span className={`text-lg transition-transform inline-block ${expanded ? 'rotate-180' : ''}`}>
              &#9660;
            </span>
          </button>
        )}
      </div>

      {/* Dogrulama paneli */}
      {expanded && !isCompleted && (
        <div className="px-4 pb-4 border-t border-slate-800/50">
          {verificationHint && (
            <p className="text-amber-400/80 text-xs mt-3 flex items-center gap-1.5">
              <span>&#128161;</span> {verificationHint}
            </p>
          )}

          {/* Photo */}
          {(verificationType === 1 || verificationType === 5) && (
            <PhotoUpload onPhotoSelect={setPhotoBase64} hint={verificationHint} />
          )}

          {/* Reflection */}
          {(verificationType === 2 || verificationType === 5) && (
            <ReflectionEditor
              minWords={minReflectionWords || 10}
              onTextChange={setReflectionText}
            />
          )}

          {/* Timer */}
          {verificationType === 3 && (
            <TaskTimer
              minimumMinutes={minTimerMinutes || 5}
              onTimerComplete={(secs) => setTimerSeconds(secs)}
              taskTitle={title}
            />
          )}

          {/* Submit butonu */}
          <button
            onClick={handleComplete}
            disabled={!canSubmit() || isSubmitting}
            className={`w-full mt-4 py-3 rounded-xl text-xs font-black uppercase tracking-wider transition-all ${
              canSubmit() && !isSubmitting
                ? "bg-emerald-600 hover:bg-emerald-500 text-white shadow-lg shadow-emerald-500/20 active:scale-[0.98]"
                : "bg-slate-800 text-slate-500 cursor-not-allowed"
            }`}
          >
            {isSubmitting ? "Dogrulanıyor..." : "Dogrula ve Tamamla"}
          </button>
        </div>
      )}
    </div>
  );
};

export default TaskItem;