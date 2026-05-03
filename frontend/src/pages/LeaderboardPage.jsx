import React, { useState, useEffect } from 'react';
import MainLayout from '../layouts/MainLayout';
import leaderboardService from '../services/leaderboardService';
import { getUserIdFromToken } from '../utils/authUtils';

const RANK_COLORS = [
  'from-amber-400 to-yellow-500',   // 1st
  'from-slate-300 to-slate-400',    // 2nd
  'from-amber-600 to-orange-700',   // 3rd
];

const RANK_ICONS = ['👑', '🥈', '🥉'];
const RANK_BORDER = ['border-amber-400/50 shadow-amber-500/20', 'border-slate-400/50 shadow-slate-400/10', 'border-amber-600/50 shadow-orange-700/10'];

const LeaderboardPage = () => {
  const [leaderboard, setLeaderboard] = useState([]);
  const [loading, setLoading] = useState(true);
  const [myRank, setMyRank] = useState(null);
  const currentUserId = getUserIdFromToken?.() || null;

  useEffect(() => {
    const fetchLeaderboard = async () => {
      try {
        const data = await leaderboardService.getLeaderboard();
        const list = Array.isArray(data) ? data : (data.leaderboard || []);
        setLeaderboard(list);
        // Find current user's rank
        const myEntry = list.findIndex(u => u.userName === JSON.parse(sessionStorage.getItem('user_data'))?.userName);
        if (myEntry >= 0) setMyRank(myEntry + 1);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchLeaderboard();
  }, []);

  const top3 = leaderboard.slice(0, 3);
  const rest = leaderboard.slice(3);

  return (
    <MainLayout>
      <div className="space-y-8 max-w-3xl mx-auto">
        {/* Header */}
        <section className="relative">
          <div className="absolute -left-10 -top-10 w-40 h-40 bg-amber-600/10 blur-[80px] rounded-full"></div>
          <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2 relative z-10">SIRALAMALAR</h2>
          <div className="h-1.5 w-24 bg-amber-500 rounded-full shadow-[0_0_20px_rgba(245,158,11,0.6)]"></div>
          <p className="text-slate-500 text-sm mt-2">En yüksek XP'ye sahip oyuncular</p>
        </section>

        {loading ? (
          <div className="space-y-4">
            {[1,2,3,4,5].map(i => (
              <div key={i} className="h-16 bg-slate-900/40 rounded-2xl animate-pulse border border-slate-800"></div>
            ))}
          </div>
        ) : (
          <>
            {/* Top 3 Podium */}
            {top3.length > 0 && (
              <div className="grid grid-cols-3 gap-4 mb-4">
                {/* 2nd place */}
                <div className={`order-1 mt-8 bg-slate-900/60 rounded-2xl border p-4 text-center ${RANK_BORDER[1]} shadow-lg`}>
                  <div className="text-3xl mb-2">{RANK_ICONS[1]}</div>
                  <div className={`w-14 h-14 mx-auto rounded-full bg-gradient-to-br ${RANK_COLORS[1]} flex items-center justify-center font-black text-white text-lg shadow-lg mb-3`}>
                    {top3[1]?.name?.charAt(0)}{top3[1]?.surname?.charAt(0)}
                  </div>
                  <p className="text-white font-bold text-sm truncate">{top3[1]?.name} {top3[1]?.surname}</p>
                  <p className="text-slate-500 text-xs">@{top3[1]?.userName}</p>
                  <p className="text-amber-400 font-black text-sm mt-2">{top3[1]?.totalXP?.toLocaleString()} XP</p>
                  <p className="text-slate-600 text-xs">Lv. {top3[1]?.generalLevel}</p>
                </div>

                {/* 1st place */}
                <div className={`order-2 bg-slate-900/60 rounded-2xl border p-4 text-center ${RANK_BORDER[0]} shadow-2xl relative`}>
                  <div className="absolute -top-4 left-1/2 -translate-x-1/2 bg-amber-500 text-black text-xs font-black px-3 py-1 rounded-full shadow-lg">1. SIRALAMASINDA</div>
                  <div className="text-3xl mb-2 mt-4">{RANK_ICONS[0]}</div>
                  <div className={`w-16 h-16 mx-auto rounded-full bg-gradient-to-br ${RANK_COLORS[0]} flex items-center justify-center font-black text-black text-xl shadow-lg shadow-amber-500/30 mb-3`}>
                    {top3[0]?.name?.charAt(0)}{top3[0]?.surname?.charAt(0)}
                  </div>
                  <p className="text-white font-bold truncate">{top3[0]?.name} {top3[0]?.surname}</p>
                  <p className="text-slate-500 text-xs">@{top3[0]?.userName}</p>
                  <p className="text-amber-400 font-black mt-2">{top3[0]?.totalXP?.toLocaleString()} XP</p>
                  <p className="text-slate-600 text-xs">Lv. {top3[0]?.generalLevel}</p>
                </div>

                {/* 3rd place */}
                <div className={`order-3 mt-12 bg-slate-900/60 rounded-2xl border p-4 text-center ${RANK_BORDER[2]} shadow-lg`}>
                  <div className="text-3xl mb-2">{RANK_ICONS[2]}</div>
                  <div className={`w-14 h-14 mx-auto rounded-full bg-gradient-to-br ${RANK_COLORS[2]} flex items-center justify-center font-black text-white text-lg shadow-lg mb-3`}>
                    {top3[2]?.name?.charAt(0)}{top3[2]?.surname?.charAt(0)}
                  </div>
                  <p className="text-white font-bold text-sm truncate">{top3[2]?.name} {top3[2]?.surname}</p>
                  <p className="text-slate-500 text-xs">@{top3[2]?.userName}</p>
                  <p className="text-amber-400 font-black text-sm mt-2">{top3[2]?.totalXP?.toLocaleString()} XP</p>
                  <p className="text-slate-600 text-xs">Lv. {top3[2]?.generalLevel}</p>
                </div>
              </div>
            )}

            {/* Rest of leaderboard */}
            <div className="space-y-2">
              {rest.map((user, idx) => {
                const rank = idx + 4;
                return (
                  <div key={user.userName} className={`flex items-center gap-4 p-4 rounded-2xl border transition-all ${
                    rank === myRank
                      ? 'border-indigo-500/50 bg-indigo-500/10 shadow-lg shadow-indigo-500/10'
                      : 'border-slate-800 bg-slate-900/40 hover:border-slate-700'
                  }`}>
                    <div className="w-8 text-center">
                      <span className="text-slate-500 font-black text-sm">{rank}</span>
                    </div>
                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-indigo-600 to-violet-600 flex items-center justify-center font-black text-white text-sm">
                      {user.name?.charAt(0)}{user.surname?.charAt(0)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-white font-bold text-sm truncate">
                        {user.name} {user.surname}
                        {rank === myRank && <span className="ml-2 text-[10px] text-indigo-400 font-black">(Sen)</span>}
                      </p>
                      <p className="text-slate-500 text-xs">@{user.userName}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-amber-400 font-black text-sm">{user.totalXP?.toLocaleString()} XP</p>
                      <p className="text-slate-600 text-xs">Lv. {user.generalLevel}</p>
                    </div>
                  </div>
                );
              })}
            </div>

            {leaderboard.length === 0 && (
              <div className="text-center py-16 text-slate-500">
                <p className="text-4xl mb-4">🏆</p>
                <p className="font-bold">Sıralama verisi yok.</p>
              </div>
            )}
          </>
        )}
      </div>
    </MainLayout>
  );
};

export default LeaderboardPage;
