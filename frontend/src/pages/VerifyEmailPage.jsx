import React, { useEffect, useState } from "react";
import { useSearchParams, useNavigate, Link } from "react-router-dom";
import authService from "../services/authService";

const VerifyEmailPage = () => {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");
  const navigate = useNavigate();

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    if (!token) {
      setError("Geçersiz veya eksik doğrulama bağlantısı.");
      return;
    }

    const verify = async () => {
      setLoading(true);
      setError("");
      setSuccess("");

      try {
        const response = await authService.verifyEmail(token);
        if (response.isSuccess) {
          setSuccess("🎉 E-posta adresiniz başarıyla doğrulandı! Giriş sayfasına yönlendiriliyorsunuz.");
          setTimeout(() => navigate("/login"), 3000);
        } else {
          setError(response.message || "E-posta doğrulama başarısız oldu.");
        }
      } catch (err) {
        console.error(err);
        setError(err.response?.data?.message || "Doğrulama işlemi sırasında bir hata oluştu.");
      } finally {
        setLoading(false);
      }
    };

    verify();
  }, [token, navigate]);

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-4 relative overflow-hidden bg-[radial-gradient(circle_at_center,_var(--tw-gradient-stops))] from-indigo-900/20 via-slate-950 to-slate-950">
      <div className="absolute top-0 left-0 w-96 h-96 bg-indigo-600/20 blur-[120px] rounded-full"></div>
      <div className="absolute bottom-0 right-0 w-96 h-96 bg-rose-600/10 blur-[120px] rounded-full"></div>

      <div className="w-full max-w-md bg-slate-900/40 backdrop-blur-xl border border-slate-800 p-8 rounded-[2rem] shadow-2xl relative z-10 text-center">
        <div className="text-center mb-6">
          <div className="w-16 h-16 bg-indigo-600 rounded-2xl mx-auto flex items-center justify-center text-3xl shadow-[0_0_30px_rgba(79,70,229,0.5)] mb-4 text-white">
            ✉️
          </div>
          <h1 className="text-3xl font-black text-white tracking-tighter italic uppercase">
            E-POSTA DOĞRULAMA
          </h1>
          <p className="text-slate-500 text-sm mt-2 font-medium">
            LifeQuest Hesabını Aktifleştir
          </p>
        </div>

        <div className="space-y-4">
          {loading && (
            <div className="p-4 bg-indigo-500/10 border border-indigo-500/20 rounded-xl text-indigo-400 font-medium text-center text-sm animate-pulse">
              E-posta adresiniz doğrulanıyor, lütfen bekleyin...
            </div>
          )}

          {error && (
            <div className="p-4 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 font-bold text-center text-sm">
              {error}
            </div>
          )}

          {success && (
            <div className="p-4 bg-emerald-500/10 border border-emerald-500/20 rounded-xl text-emerald-500 font-bold text-center text-sm">
              {success}
            </div>
          )}

          <div className="mt-8 text-center">
            <Link
              to="/login"
              className="text-indigo-400 font-bold hover:text-indigo-300 underline decoration-2 underline-offset-4"
            >
              Giriş Sayfasına Dön
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default VerifyEmailPage;
