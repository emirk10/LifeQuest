import React, { useState } from "react";
import axiosClient from "../api/axiosClient";
import { taskPoolApi } from "../api/moodApi";

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
  status,
  postponedTo,
  onComplete,
  onPostpone,
  onCancelPostpone,
}) => {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showDesc, setShowDesc] = useState(false);
  const [showPostpone, setShowPostpone] = useState(false);
  const [postponeReason, setPostponeReason] = useState("");

  const safeDifficulty = DIFFICULTY_COLORS[difficulty] ? difficulty : "Medium";

  const handleShare = async () => {
    const email = window.prompt("Görevi e-posta ile paylaşmak istediğiniz adresi giriniz:");
    if (!email || !email.includes("@")) {
      alert("Lütfen geçerli bir e-posta adresi giriniz.");
      return;
    }

    try {
      const res = await axiosClient.post("/social/share-task", { recipientEmail: email, taskTitle: title, taskId: id });
      if (res.data.success) {
        alert(res.data.message);
      } else {
        alert(res.data.message || "Paylaşma başarısız.");
      }
    } catch (err) {
      console.error(err);
      alert(err.response?.data?.message || "Görev paylaşılırken bir hata oluştu.");
    }
  };

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

  const handlePostpone = async () => {
    if (isSubmitting || isCompleted) return;
    setIsSubmitting(true);
    try {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      const dateStr = tomorrow.toISOString().split('T')[0];
      
      await taskPoolApi.postponeTask(id, dateStr, postponeReason);
      setShowPostpone(false);
      onPostpone?.(id);
    } catch (err) {
      console.error("Erteleme hatası:", err);
      alert("Görev ertelenirken bir hata oluştu.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelPostpone = async () => {
    if (isSubmitting) return;
    setIsSubmitting(true);
    try {
      await taskPoolApi.cancelPostponeTask(id);
      onCancelPostpone?.(id);
    } catch (err) {
      console.error("Erteleme iptal hatası:", err);
      alert("Erteleme iptal edilirken bir hata oluştu.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const isPostponed = status === 'Postponed';

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

          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <h4
                className={`font-bold text-sm truncate transition-colors ${
                  isCompleted ? "text-slate-500 line-through" : "text-white group-hover:text-indigo-400"
                }`}
              >
                {title}
              </h4>
              {isPostponed && (
                <span className="text-[10px] font-bold text-amber-500 bg-amber-500/10 px-2 py-0.5 rounded border border-amber-500/20 whitespace-nowrap">
                  ERTELENDİ
                </span>
              )}
            </div>
            <div className="flex items-center gap-3">
              <span className={`text-[10px] font-bold px-2 py-0.5 rounded-md border uppercase tracking-wider ${DIFFICULTY_COLORS[safeDifficulty]}`}>
                {safeDifficulty}
              </span>
              <span className="text-xs font-bold text-indigo-400">+{xp} XP</span>
              {isPostponed && postponedTo && (
                <span className="text-xs text-amber-500/80 italic hidden sm:inline">
                  (📅 {postponedTo} tarihinde aktif olacak)
                </span>
              )}
              {description && (
                <span className="text-[10px] text-slate-600 hover:text-slate-400 transition-colors">
                  {showDesc ? "▲ Gizle" : "▼ Detay"}
                </span>
              )}
            </div>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex items-center gap-2 shrink-0 border-l border-slate-800/50 pl-4 py-2">
          {isPostponed ? (
            <button
              onClick={handleCancelPostpone}
              disabled={isSubmitting}
              className={`px-4 py-2 rounded-xl text-xs font-bold transition-all shrink-0 ${
                isSubmitting
                  ? "bg-slate-800 text-slate-500 cursor-wait"
                  : "bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white"
              }`}
            >
              {isSubmitting ? "..." : "İptal Et"}
            </button>
          ) : (
            <>
              {!isCompleted && (
                <button
                  onClick={() => setShowPostpone(!showPostpone)}
                  title="Görevi Yarına Ertele"
                  className="w-10 h-10 rounded-xl bg-slate-800/50 hover:bg-amber-500/10 text-slate-500 hover:text-amber-500 flex items-center justify-center transition-colors border border-transparent hover:border-amber-500/20 shrink-0"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </button>
              )}

              <button
                onClick={handleShare}
                className="px-4 py-2.5 rounded-xl text-xs font-black uppercase tracking-wider transition-all flex-shrink-0 bg-slate-800 hover:bg-slate-700 text-slate-300 border border-slate-700 active:scale-95 shadow-lg"
              >
                ✉️ Paylaş
              </button>

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
            </>
          )}
        </div>
      </div>

      {/* Postpone Panel */}
      {showPostpone && (
        <div className="px-4 pb-4 border-t border-slate-800/50 bg-amber-500/5 rounded-b-2xl">
          <div className="pt-4 flex flex-col gap-3">
            <div className="flex justify-between items-center">
              <p className="text-amber-400 text-xs font-bold uppercase tracking-tight">Görevi Yarına Ertele</p>
              <span className="text-[10px] text-amber-500/60 italic">* XP kazancı %70'e düşecektir.</span>
            </div>
            <div className="flex gap-2">
              <input
                type="text"
                placeholder="Erteleme sebebi (isteğe bağlı)..."
                value={postponeReason}
                onChange={(e) => setPostponeReason(e.target.value)}
                className="flex-1 bg-slate-900/80 border border-slate-800 rounded-xl px-4 py-2 text-sm text-slate-200 outline-none focus:border-amber-500/50 transition-colors"
              />
              <button
                onClick={handlePostpone}
                disabled={isSubmitting}
                className="bg-amber-600 hover:bg-amber-500 text-white px-4 py-2 rounded-xl text-xs font-bold transition-all whitespace-nowrap"
              >
                Onayla
              </button>
              <button
                onClick={() => setShowPostpone(false)}
                className="bg-slate-800 hover:bg-slate-700 text-slate-400 px-4 py-2 rounded-xl text-xs font-bold transition-all"
              >
                İptal
              </button>
            </div>
          </div>
        </div>
      )}

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