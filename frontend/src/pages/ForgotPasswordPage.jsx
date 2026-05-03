import React, { useState } from "react";
import { Link } from "react-router-dom";
import authService from "../services/authService";

const ForgotPasswordPage = () => {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setMessage("");
    setError("");

    try {
      const res = await authService.forgotPassword(email);
      if (res.isSuccess || res.success) {
        setMessage("🎉 Şifre sıfırlama bağlantısı e-posta adresinize gönderildi! Lütfen gelen kutunuzu kontrol edin.");
      } else {
        setError(res.message || "Şifre sıfırlama işlemi başarısız.");
      }
    } catch (err) {
      console.error(err);
      setError(err.response?.data?.message || "Bir hata oluştu. Lütfen tekrar deneyin.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden bg-[radial-gradient(circle_at_center,_var(--tw-gradient-stops))] from-indigo-900/20 via-slate-950 to-slate-950">
      <div className="absolute top-0 left-0 w-96 h-96 bg-indigo-600/20 blur-[120px] rounded-full"></div>
      <div className="absolute bottom-0 right-0 w-96 h-96 bg-rose-600/10 blur-[120px] rounded-full"></div>

      <div className="w-full max-w-md bg-slate-900/40 backdrop-blur-xl border border-slate-800 p-8 rounded-[2rem] shadow-2xl relative z-10">
        <div className="text-center mb-8">
          <div className="w-16 h-16 bg-indigo-600 rounded-2xl mx-auto flex items-center justify-center text-3xl shadow-[0_0_30px_rgba(79,70,229,0.5)] mb-4 text-white">
            🔑
          </div>
          <h1 className="text-3xl font-black text-white tracking-tighter uppercase italic">
            ŞİFREMİ UNUTTUM
          </h1>
          <p className="text-slate-500 text-sm mt-2 font-medium">
            Şifreni yenilemek için e-postanı gir.
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {error && (
            <div className="p-3 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 text-xs font-bold text-center">
              {error}
            </div>
          )}

          {message && (
            <div className="p-3 bg-emerald-500/10 border border-emerald-500/20 rounded-xl text-emerald-500 text-xs font-bold text-center">
              {message}
            </div>
          )}

          <div>
            <label className="block text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2 ml-1">
              E-Posta Adresi
            </label>
            <input
              type="email"
              placeholder="kullanici@lifequest.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full bg-slate-800/50 border border-slate-700 rounded-2xl p-4 text-white placeholder:text-slate-600 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all"
              required
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-indigo-600 hover:bg-indigo-500 text-white font-black py-4 rounded-2xl shadow-[0_0_20px_rgba(79,70,229,0.3)] transition-all active:scale-[0.98] uppercase tracking-widest"
          >
            {loading ? "GÖNDERİLİYOR..." : "SIFIRLAMA LİNKİ GÖNDER"}
          </button>
        </form>

        <div className="mt-8 text-center">
          <p className="text-slate-500 text-sm">
            Şifreni hatırladın mı?{" "}
            <Link
              to="/login"
              className="text-indigo-400 font-bold hover:text-indigo-300 underline decoration-2 underline-offset-4"
            >
              Giriş Yap
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default ForgotPasswordPage;
