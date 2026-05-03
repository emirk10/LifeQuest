import React, { useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import authService from "../services/authService";
import { useAuth } from "../context/authContext";
import { getUserNameFromToken } from "../utils/authUtils";
import axiosClient from "../api/axiosClient";

const LoginPage = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [searchParams] = useSearchParams();

  const { login } = useAuth();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");

    try {
      const response = await authService.login({ email, password });

      if (response && response.token) {
        sessionStorage.setItem("token", response.token);

        const derivedName = getUserNameFromToken();
        const userData = {
          name: derivedName,
          ...response.user,
        };

        const acceptTask = searchParams.get("acceptTask");
        if (acceptTask) {
          sessionStorage.setItem("acceptTask", acceptTask);
        }

        login(userData, response.token);
      } else {
        setError("Beklenmedik bir sunucu yanıtı alındı.");
      }
    } catch (err) {
      console.error(err);
      setError("Giriş başarısız! E-posta veya şifre hatalı olabilir.");
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center p-6 bg-[radial-gradient(circle_at_center,_var(--tw-gradient-stops))] from-indigo-900/20 via-slate-950 to-slate-950">
      <div className="w-full max-w-md">
        <div className="text-center mb-10">
          <div className="inline-flex w-16 h-16 bg-indigo-600 rounded-2xl items-center justify-center font-black text-3xl shadow-[0_0_30px_rgba(79,70,229,0.3)] mb-4 text-white">
            LQ
          </div>
          <h1 className="text-3xl font-black text-white tracking-tighter">
            LIFEQUEST
          </h1>
          <p className="text-slate-500 font-medium mt-2 italic">
            Maceraya devam etmek için giriş yap.
          </p>
        </div>

        <div className="bg-slate-900/50 border border-slate-800 p-8 rounded-[2.5rem] backdrop-blur-sm shadow-2xl">
          <form className="space-y-6" onSubmit={handleLogin}>
            {error && (
              <div className="p-3 bg-rose-500/10 border border-rose-500/20 rounded-xl text-rose-500 text-xs font-bold text-center">
                {error}
              </div>
            )}

            <div>
              <label className="block text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2 ml-1">
                E-Posta Adresi
              </label>
              <input
                type="email"
                placeholder="admin@lifequest.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full bg-slate-800/50 border border-slate-700 rounded-2xl p-4 text-white placeholder:text-slate-600 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all"
                required
              />
            </div>

            <div>
              <label className="block text-[10px] font-black text-slate-500 uppercase tracking-widest mb-2 ml-1">
                Şifre
              </label>
              <input
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full bg-slate-800/50 border border-slate-700 rounded-2xl p-4 text-white placeholder:text-slate-600 focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-500 transition-all"
                required
              />
            </div>

            <button
              type="submit"
              className="w-full bg-indigo-600 hover:bg-indigo-500 text-white font-black py-4 rounded-2xl shadow-[0_0_20px_rgba(79,70,229,0.3)] transition-all active:scale-[0.98] uppercase tracking-widest"
            >
              Giriş Yap
            </button>
          </form>

          <div className="mt-8 text-center space-y-2">
            <p className="text-slate-500 text-sm">
              Hesabın yok mu?{" "}
              <Link
                to="/register"
                className="text-indigo-400 font-bold hover:text-indigo-300 underline decoration-2 underline-offset-4"
              >
                Hemen Kaydol
              </Link>
            </p>
            <p className="text-slate-500 text-sm">
              <Link
                to="/forgot-password"
                className="text-slate-400 font-semibold hover:text-indigo-300 underline decoration-2 underline-offset-4"
              >
                Şifremi Unuttum
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
