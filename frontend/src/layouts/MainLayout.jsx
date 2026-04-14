import React, { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../context/authContext";
import userService from "../services/userService";
import { getUserRoleFromToken } from "../utils/authUtils";

const MainLayout = ({ children }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, user } = useAuth();

  const [headerUser, setHeaderUser] = useState(user);

  const userRole = getUserRoleFromToken();
  const isAdmin = userRole === "Admin";

  useEffect(() => {
    const fetchLatestProfile = async () => {
      try {
        if (user) {
          const freshData = await userService.getProfile();
          setHeaderUser((prev) => ({
            ...prev,
            ...freshData,
            xp: freshData.totalXP || freshData.TotalXP || 0,
            level: freshData.generalLevel || freshData.GeneralLevel || 1,
          }));
        }
      } catch (error) {
        console.error(error);
      }
    };

    fetchLatestProfile();

    window.addEventListener("userUpdated", fetchLatestProfile);

    return () => {
      window.removeEventListener("userUpdated", fetchLatestProfile);
    };
  }, [location.pathname, user]);

  const menuItems = [
    { name: "Dashboard", path: "/dashboard", icon: "🏠" },
    { name: "Görevler", path: "/tasks", icon: "⚔️" },
    { name: "Rozetler", path: "/badges", icon: "🏆" },
    { name: "Profil", path: "/profile", icon: "👤" },
    { name: "AI Test", path: "/ai-test", icon: "🤖" },
    ...(isAdmin ? [{ name: "Admin", path: "/admin", icon: "⚙️" }] : []),
  ];

  const totalXp = headerUser?.xp || 0;
  const levelBase = 1000;
  const currentLevelXp = totalXp % levelBase;
  const progressPercentage = (currentLevelXp / levelBase) * 100;

  return (
    <div className="flex h-screen w-full bg-slate-950 text-slate-100 overflow-hidden font-sans">
      <aside className="w-72 bg-slate-900 border-r border-slate-800 flex flex-col shrink-0 shadow-2xl z-20">
        <div className="p-8">
          <div
            className="flex items-center gap-3 group cursor-pointer"
            onClick={() => navigate("/dashboard")}
          >
            <div className="w-10 h-10 bg-indigo-600 rounded-xl flex items-center justify-center font-black text-xl shadow-[0_0_20px_rgba(79,70,229,0.4)] group-hover:scale-110 transition-transform">
              LQ
            </div>
            <h1 className="text-xl font-black tracking-tighter uppercase italic">
              LifeQuest
            </h1>
          </div>
        </div>

        <nav className="flex-1 px-4 space-y-2">
          {menuItems.map((item) => (
            <button
              key={item.path}
              onClick={() => navigate(item.path)}
              className={`w-full flex items-center gap-4 p-4 rounded-2xl transition-all duration-300 ${
                location.pathname === item.path
                  ? "bg-indigo-600/20 text-indigo-400 border border-indigo-600/30 shadow-[inset_0_0_15px_rgba(79,70,229,0.1)]"
                  : "hover:bg-slate-800/50 text-slate-500 hover:text-slate-300"
              }`}
            >
              <span className="text-xl">{item.icon}</span>
              <span className="font-bold tracking-wide uppercase text-xs">
                {item.name}
              </span>
            </button>
          ))}
        </nav>

        <div className="p-6 mt-auto border-t border-slate-800 space-y-4 bg-slate-900/50">
          <div className="p-4 bg-slate-800/30 rounded-2xl border border-slate-700/50 text-center relative overflow-hidden group">
            <div className="absolute inset-0 bg-indigo-500/5 opacity-0 group-hover:opacity-100 transition-opacity"></div>
            <p className="text-[10px] text-slate-500 font-black uppercase tracking-widest mb-1 relative z-10">
              Karakter Durumu
            </p>
            <p className="text-sm font-black text-white italic tracking-tight relative z-10">
              {headerUser?.name?.toUpperCase() ||
                headerUser?.Name?.toUpperCase() ||
                "SAVAŞÇI"}
            </p>
          </div>

          <button
            onClick={logout}
            className="w-full flex items-center justify-center gap-2 p-4 rounded-2xl bg-rose-500/10 text-rose-500 border border-rose-500/20 hover:bg-rose-500 hover:text-white transition-all duration-300 font-black text-[10px] uppercase tracking-[0.2em] shadow-lg shadow-rose-900/10"
          >
            <span>🚪</span> Güvenli Çıkış
          </button>
        </div>
      </aside>

      <div className="flex-1 flex flex-col relative">
        <header className="h-24 bg-slate-950/80 backdrop-blur-2xl border-b border-slate-800/50 flex items-center justify-between px-10 z-10 shadow-lg">
          <div className="flex flex-col gap-3 w-80">
            <div className="flex justify-between text-[10px] font-black tracking-widest text-slate-500 uppercase">
              <span className="text-indigo-500 font-black">Experience Bar</span>
              <span>
                {currentLevelXp} / {levelBase} XP
              </span>
            </div>
            <div className="h-3 w-full bg-slate-900 rounded-full overflow-hidden p-[2px] border border-slate-800 shadow-inner">
              <div
                className="h-full bg-gradient-to-r from-indigo-600 via-indigo-400 to-cyan-400 rounded-full transition-all duration-1000 shadow-[0_0_15px_rgba(79,70,229,0.5)]"
                style={{ width: `${progressPercentage}%` }}
              ></div>
            </div>
          </div>

          <div className="flex items-center gap-10">
            <div className="text-right">
              <p className="text-[10px] text-slate-500 font-black uppercase tracking-[0.2em] mb-1">
                Current Level
              </p>
              <p className="text-4xl font-black text-indigo-500 italic tracking-tighter drop-shadow-[0_0_10px_rgba(79,70,229,0.3)]">
                {Math.floor(headerUser?.level || 1)}
              </p>
            </div>
          </div>
        </header>

        <main className="flex-1 overflow-y-auto p-12 bg-[radial-gradient(circle_at_top_right,_var(--tw-gradient-stops))] from-slate-900/30 via-slate-950 to-slate-950">
          <div className="max-w-7xl mx-auto">{children}</div>
        </main>
      </div>
    </div>
  );
};

export default MainLayout;