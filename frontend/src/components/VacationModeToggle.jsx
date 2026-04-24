import React, { useState } from "react";
import axiosClient from "../api/axiosClient";
import { getUserIdFromToken } from "../utils/authUtils";

const VACATION_REASONS = [
  { value: "Tatil", label: "🏖️ Tatil" },
  { value: "Hastalık", label: "🤒 Hastalık" },
  { value: "Çok yoğunum", label: "💼 Çok yoğunum" },
];

const VacationModeToggle = ({ isOnVacation: initialVacation = false, onVacationChange }) => {
  const [isOnVacation, setIsOnVacation] = useState(initialVacation);
  const [showConfirm, setShowConfirm] = useState(false);
  const [reason, setReason] = useState("Tatil");
  const [endDate, setEndDate] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState(null);

  const todayStr = new Date().toISOString().split("T")[0];

  const handleStartVacation = async () => {
    setLoading(true);
    try {
      const userId = getUserIdFromToken();
      const body = {
        reason,
        endDate: endDate || null,
      };
      const res = await axiosClient.post(`/users/${userId}/vacation/start`, body);
      setIsOnVacation(true);
      setMessage({ type: "success", text: res.data?.message || "Tatil modu başlatıldı! 🏖️" });
      setShowConfirm(false);
      onVacationChange?.(true);
    } catch (err) {
      setMessage({ type: "error", text: "Bir hata oluştu." });
    } finally {
      setLoading(false);
    }
  };

  const handleEndVacation = async () => {
    setLoading(true);
    try {
      const userId = getUserIdFromToken();
      const res = await axiosClient.post(`/users/${userId}/vacation/end`);
      setIsOnVacation(false);
      setMessage({ type: "success", text: res.data?.message || "Tatil modu kapatıldı! 🔥" });
      onVacationChange?.(false);
    } catch (err) {
      setMessage({ type: "error", text: "Bir hata oluştu." });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-gradient-to-br from-slate-900/80 to-slate-800/60 border border-slate-700/50 rounded-[2rem] p-6 space-y-5">
      {/* Başlık */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-base font-black text-white tracking-wider uppercase italic flex items-center gap-2">
            <span className="text-xl">🏖️</span> Tatil / Mola Modu
          </h3>
          <p className="text-xs text-slate-400 mt-1">
            Bu sürede streak'in korunur, görev gelmez.
          </p>
        </div>

        {/* Toggle */}
        <button
          onClick={() => isOnVacation ? handleEndVacation() : setShowConfirm(true)}
          disabled={loading}
          className={`relative w-14 h-7 rounded-full transition-all duration-300 ${
            isOnVacation
              ? "bg-emerald-500 shadow-lg shadow-emerald-500/30"
              : "bg-slate-700 hover:bg-slate-600"
          } disabled:opacity-50`}
          aria-label="Tatil modunu aç/kapat"
        >
          <span
            className={`absolute top-1 w-5 h-5 bg-white rounded-full shadow transition-all duration-300 ${
              isOnVacation ? "left-8" : "left-1"
            }`}
          />
        </button>
      </div>

      {/* Durum rozeti */}
      {isOnVacation && (
        <div className="flex items-center gap-2 bg-emerald-500/10 border border-emerald-500/20 rounded-2xl px-4 py-2">
          <span className="text-emerald-400 text-sm">✅</span>
          <p className="text-emerald-300 text-sm font-medium">
            Tatil modundasın — streak'in güvende! 🔥
          </p>
        </div>
      )}

      {/* Onay formu */}
      {showConfirm && !isOnVacation && (
        <div className="space-y-4 border border-slate-700/50 rounded-2xl p-4 bg-slate-900/40">
          <p className="text-sm text-slate-300 font-semibold">Tatil nedeni nedir?</p>

          {/* Neden seçimi */}
          <div className="grid grid-cols-3 gap-2">
            {VACATION_REASONS.map((r) => (
              <button
                key={r.value}
                onClick={() => setReason(r.value)}
                className={`py-2 px-3 rounded-xl text-xs font-bold transition-all ${
                  reason === r.value
                    ? "bg-indigo-600/30 border border-indigo-500 text-white"
                    : "bg-slate-800 border border-slate-700 text-slate-400 hover:border-slate-500"
                }`}
              >
                {r.label}
              </button>
            ))}
          </div>

          {/* Bitiş tarihi */}
          <div>
            <label className="text-xs text-slate-400 block mb-1">Bitiş tarihi (opsiyonel)</label>
            <input
              type="date"
              min={todayStr}
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="w-full bg-slate-800 border border-slate-700 rounded-xl px-3 py-2 text-sm text-white focus:outline-none focus:border-indigo-500 transition-colors"
            />
          </div>

          {/* Açıklama */}
          <div className="bg-amber-500/10 border border-amber-500/20 rounded-xl p-3">
            <p className="text-amber-300 text-xs">
              ⚠️ Tatil süresince sana görev atanmaz. Dilediğin zaman kapatabilirsin.
            </p>
          </div>

          {/* Butonlar */}
          <div className="flex gap-2">
            <button
              onClick={handleStartVacation}
              disabled={loading}
              className="flex-1 bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-bold py-2 rounded-xl transition-all disabled:opacity-50"
            >
              {loading ? "Başlatılıyor..." : "Başlat 🏖️"}
            </button>
            <button
              onClick={() => setShowConfirm(false)}
              className="flex-1 bg-slate-800 hover:bg-slate-700 text-slate-300 text-sm font-bold py-2 rounded-xl transition-all"
            >
              İptal
            </button>
          </div>
        </div>
      )}

      {/* Mesaj */}
      {message && (
        <div
          className={`rounded-xl px-4 py-2 text-sm ${
            message.type === "success"
              ? "bg-emerald-500/10 text-emerald-300 border border-emerald-500/20"
              : "bg-red-500/10 text-red-300 border border-red-500/20"
          }`}
        >
          {message.text}
        </div>
      )}
    </div>
  );
};

export default VacationModeToggle;
