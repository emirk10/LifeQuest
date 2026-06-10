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
  onFocus,
  onCancel,
  onReassign,
  tags = [],
}) => {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showDesc, setShowDesc] = useState(false);
  const [showPostpone, setShowPostpone] = useState(false);
  const [postponeReason, setPostponeReason] = useState("");
  const [touchStart, setTouchStart] = useState(null);
  const [touchEnd, setTouchEnd] = useState(null);
  const [swipeOffset, setSwipeOffset] = useState(0);

  
  const minSwipeDistance = 50;

  const safeDifficulty = DIFFICULTY_COLORS[difficulty] ? difficulty : "Medium";

  const [showReflection, setShowReflection] = useState(false);
  const [reflectionText, setReflectionText] = useState("");
  const [customTags, setCustomTags] = useState("");

  const handleShare = async () => {
    try {
      const res = await axiosClient.post(`/user-tasks/${id}/share`);
      if (res.data.success) {
        const fullUrl = `${window.location.origin}${res.data.shareUrl}`;
        const action = window.confirm(`Paylaşım linki oluşturuldu!\n\nLink: ${fullUrl}\n\nTamam'a basarak linki kopyalayabilir, İptal'e basarak e-posta ile paylaşmaya devam edebilirsiniz.`);
        
        if (action) {
          await navigator.clipboard.writeText(fullUrl);
          alert("Bağlantı panoya kopyalandı! 📋");
          return;
        }
      }
    } catch (err) {
      console.error("Link oluşturma hatası:", err);
    }

    const email = window.prompt("Görevi e-posta ile paylaşmak istediğiniz adresi giriniz:");
    if (!email || !email.includes("@")) return;

    try {
      const res = await axiosClient.post("/social/share-task", { recipientEmail: email, taskTitle: title, taskId: id });
      alert(res.data.message || (res.data.success ? "Paylaşıldı!" : "Hata oluştu."));
    } catch (err) {
      alert(err.response?.data?.message || "Paylaşım hatası.");
    }
  };

  const handleComplete = () => {
    if (isCompleted || isSubmitting) return;
    setShowReflection(true);
  };

  const handleFinalComplete = async () => {
    if (isSubmitting || isCompleted) return;
    setIsSubmitting(true);
    try {
      const tagsArray = customTags ? customTags.split(',').map(t => t.trim()).filter(t => t) : [];
      await onComplete(id, reflectionText, tagsArray);
      setShowReflection(false);
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

  const handleCancel = async () => {
    if (isSubmitting || isCompleted) return;
    const confirmCancel = window.confirm("Bu görevi iptal etmek istediğinize emin misiniz? (Ceza uygulanmaz)");
    if (!confirmCancel) return;
    setIsSubmitting(true);
    try {
      await onCancel?.(id);
    } catch (err) {
      console.error("Görev iptal hatası:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleReassign = async () => {
    if (isSubmitting) return;
    setIsSubmitting(true);
    try {
      await onReassign?.(id);
    } catch (err) {
      console.error("Yeniden atama hatası:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  
  const onTouchStart = (e) => {
    setTouchEnd(null);
    setTouchStart(e.targetTouches[0].clientX);
  };

  const onTouchMove = (e) => {
    const currentTouch = e.targetTouches[0].clientX;
    const offset = currentTouch - touchStart;
    
    
    if (Math.abs(offset) < 150) {
      setSwipeOffset(offset);
    }
    setTouchEnd(currentTouch);
  };

  const onTouchEnd = () => {
    if (!touchStart || !touchEnd) return;
    const distance = touchStart - touchEnd;
    const isLeftSwipe = distance > minSwipeDistance;
    const isRightSwipe = distance < -minSwipeDistance;

    if (isRightSwipe && !isCompleted) {
      handleComplete();
    } else if (isLeftSwipe && !isCompleted && !isPostponed) {
      setShowPostpone(true);
    }
    
    setSwipeOffset(0);
  };

  const isPostponed = status === 'Postponed';

  return (
    <div
      onTouchStart={onTouchStart}
      onTouchMove={onTouchMove}
      onTouchEnd={onTouchEnd}
      style={{ transform: `translateX(${swipeOffset}px)`, transition: swipeOffset === 0 ? 'transform 0.3s' : 'none' }}
      className={`rounded-2xl border transition-all duration-300 relative overflow-hidden ${
        isCompleted || status === 'Cancelled' || status === 'Abandoned'
          ? "opacity-50 grayscale border-slate-800 bg-slate-900/20"
          : "border-slate-800 hover:border-indigo-500/40 bg-slate-900/40 hover:bg-slate-900/60 hover:shadow-lg hover:shadow-indigo-500/5"
      }`}
    >
      {}
      {swipeOffset > 20 && !isCompleted && status !== 'Cancelled' && status !== 'Abandoned' && (
        <div className="absolute inset-y-0 left-0 w-16 bg-emerald-500/20 flex items-center justify-center text-emerald-500 font-black text-xs animate-pulse">
          BİTİR
        </div>
      )}
      {swipeOffset < -20 && !isCompleted && !isPostponed && status !== 'Cancelled' && status !== 'Abandoned' && (
        <div className="absolute inset-y-0 right-0 w-16 bg-amber-500/20 flex items-center justify-center text-amber-500 font-black text-xs animate-pulse">
          ERTELE
        </div>
      )}
      <div className="p-4 flex items-center justify-between gap-4">
        {}
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
              {status === 'Cancelled' && (
                <span className="text-[10px] font-bold text-rose-500 bg-rose-500/10 px-2 py-0.5 rounded border border-rose-500/20 whitespace-nowrap">
                  İPTAL EDİLDİ
                </span>
              )}
              {status === 'Abandoned' && (
                <span className="text-[10px] font-bold text-red-500 bg-red-500/10 px-2 py-0.5 rounded border border-red-500/20 whitespace-nowrap">
                  VAZGEÇİLDİ
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
            {}
            {(tags.length > 0 || customTags) && (
              <div className="flex flex-wrap gap-1 mt-1.5">
                {tags.map((tag, i) => (
                  <span key={i} className="text-[9px] bg-indigo-500/10 text-indigo-400 px-1.5 py-0.5 rounded border border-indigo-500/20">#{tag}</span>
                ))}
              </div>
            )}
          </div>
        </div>

        {}
        <div className="flex items-center gap-2 shrink-0 border-l border-slate-800/50 pl-4 py-2">
          {status === 'Cancelled' ? (
            <button
              onClick={handleReassign}
              disabled={isSubmitting}
              className={`px-4 py-2.5 rounded-xl text-xs font-black uppercase tracking-wider transition-all shrink-0 bg-indigo-600 hover:bg-indigo-500 text-white shadow-lg active:scale-95 ${
                isSubmitting ? "bg-indigo-800 text-indigo-400 cursor-wait" : ""
              }`}
            >
              {isSubmitting ? "..." : "🔄 Yeniden Ata"}
            </button>
          ) : isPostponed ? (
            <button
              onClick={handleCancelPostpone}
              disabled={isSubmitting}
              className={`px-4 py-2 rounded-xl text-xs font-bold transition-all shrink-0 ${
                isSubmitting
                  ? "bg-slate-800 text-slate-500 cursor-wait"
                  : "bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white"
              }`}
            >
              {isSubmitting ? "..." : "Ertelemeyi Kaldır"}
            </button>
          ) : status === 'Abandoned' ? (
            <span className="text-xs text-red-500 font-bold uppercase tracking-widest px-2">Vazgeçildi</span>
          ) : (
            <>
              {!isCompleted && (
                <button
                  onClick={handleCancel}
                  disabled={isSubmitting}
                  title="Görevi İptal Et"
                  className="w-10 h-10 rounded-xl bg-slate-800/50 hover:bg-rose-500/10 text-slate-500 hover:text-rose-500 flex items-center justify-center transition-colors border border-transparent hover:border-rose-500/20 shrink-0"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              )}

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

              {!isCompleted && onFocus && (
                <button
                  onClick={() => onFocus({ id, title, xp, difficulty })}
                  title="Odaklanma Modu"
                  className="w-10 h-10 rounded-xl bg-indigo-600/10 hover:bg-indigo-600/20 text-indigo-400 flex items-center justify-center transition-colors border border-indigo-500/20 shrink-0"
                >
                  <span className="text-lg">⏱️</span>
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

      {}
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

      {/* Reflection Panel — FEAT-002 */}
      {showReflection && !isCompleted && (
        <div className="px-4 pb-4 border-t border-slate-800/50 bg-indigo-500/5 rounded-b-2xl animate-in slide-in-from-top-2 duration-300">
          <div className="pt-4 flex flex-col gap-3">
            <div className="flex justify-between items-center">
              <p className="text-indigo-400 text-xs font-black uppercase tracking-widest">Görevi Tamamla & Yorumla</p>
              <span className="text-[10px] text-indigo-500/60 font-bold">+5 XP Bonus Kazan! ✍️</span>
            </div>
            
            <textarea
              placeholder="Bu görev sana ne hissettirdi? (Mikro-günlük)..."
              value={reflectionText}
              onChange={(e) => setReflectionText(e.target.value)}
              className="w-full bg-slate-900/80 border border-slate-800 rounded-xl px-4 py-2.5 text-sm text-slate-200 outline-none focus:border-indigo-500/50 transition-colors resize-none"
              rows={2}
            />

            <div className="flex gap-2">
              <div className="flex-1 relative">
                <input
                  type="text"
                  placeholder="Etiketler (virgülle ayır)..."
                  value={customTags}
                  onChange={(e) => setCustomTags(e.target.value)}
                  className="w-full bg-slate-900/80 border border-slate-800 rounded-xl px-4 py-2 text-xs text-slate-400 outline-none focus:border-indigo-500/50"
                />
              </div>
              <button
                onClick={handleFinalComplete}
                disabled={isSubmitting}
                className="bg-indigo-600 hover:bg-indigo-500 text-white px-6 py-2 rounded-xl text-xs font-black uppercase tracking-wider transition-all shadow-lg shadow-indigo-500/20 active:scale-95"
              >
                {isSubmitting ? "..." : "Kaydet & Bitir"}
              </button>
              <button
                onClick={() => setShowReflection(false)}
                className="bg-slate-800 hover:bg-slate-700 text-slate-400 px-4 py-2 rounded-xl text-xs font-bold transition-all"
              >
                İptal
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default TaskItem;