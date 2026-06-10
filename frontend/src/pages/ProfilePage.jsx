import React, { useState, useEffect } from "react";
import MainLayout from "../layouts/MainLayout";
import userService from "../services/userService";
import axiosClient from "../api/axiosClient";
import { getUserIdFromToken } from "../utils/authUtils";
import BehaviorInsightCard from "../components/BehaviorInsightCard";
import VacationModeToggle from "../components/VacationModeToggle";
import { Radar, RadarChart, PolarGrid, PolarAngleAxis, PolarRadiusAxis, ResponsiveContainer, Tooltip } from 'recharts';

const PERSONALITY_OPTIONS = [
  { value: "friendly", label: "Arkadaşça 😊", desc: "Sıcak, destekleyici ve neşeli" },
  { value: "motivator", label: "Motivasyoncu 🔥", desc: "Enerjik, heyecanlı ve teşvik edici" },
  { value: "coach", label: "Koç 🎯", desc: "Analitik, stratejik ve verimlilik odaklı" },
];

const ProfilePage = () => {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [chatPersonality, setChatPersonality] = useState("friendly");
  const [nudgeEnabled, setNudgeEnabled] = useState(true);
  const [personalitySaving, setPersonalitySaving] = useState(false);
  const [behaviorSummary, setBehaviorSummary] = useState(null);
  const [isOnVacation, setIsOnVacation] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editData, setEditData] = useState({ bio: "", location: "", interests: [] });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await userService.getProfile();
        setProfile(data);
        setChatPersonality(data?.chatPersonality || "friendly");
        setNudgeEnabled(data?.nudgeEnabled ?? true);
        setIsOnVacation(data?.isOnVacation ?? false);
        setEditData({
          bio: data?.bio || "",
          location: data?.location || "",
          interests: data?.interests || []
        });

        
        const userId = getUserIdFromToken();
        if (userId) {
          try {
            const bRes = await axiosClient.get(`/users/${userId}/behavior-summary`);
            setBehaviorSummary(bRes.data);
          } catch {  }
        }
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, []);

  const handlePersonalityChange = async (value) => {
    setChatPersonality(value);
    setPersonalitySaving(true);
    try {
      const userId = getUserIdFromToken();
      await axiosClient.patch(`/users/${userId}/chat-personality`, { chatPersonality: value });
    } catch (err) {
      console.error("Kişilik kaydedilemedi:", err);
    } finally {
      setPersonalitySaving(false);
    }
  };

  const handleNudgeToggle = async () => {
    const newVal = !nudgeEnabled;
    setNudgeEnabled(newVal);
    try {
      const userId = getUserIdFromToken();
      await axiosClient.patch(`/users/${userId}/nudge-settings`, { nudgeEnabled: newVal });
    } catch (err) {
      console.error("Nudge kaydedilemedi:", err);
      setNudgeEnabled(!newVal); 
    }
  };

  const handleUpdateProfile = async () => {
    setSaving(true);
    try {
      await userService.updateProfile(editData);
      setProfile(prev => ({ ...prev, ...editData }));
      setShowEditModal(false);
    } catch (err) {
      console.error("Profil güncellenemedi:", err);
    } finally {
      setSaving(false);
    }
  };


  const getActiveDays = (dateString) => {
    if (!dateString) return 0;
    const start = new Date(dateString);
    const now = new Date();
    const diffTime = Math.abs(now - start);
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  };

  const colors = [
    "bg-blue-500",
    "bg-emerald-500",
    "bg-amber-500",
    "bg-rose-500",
    "bg-purple-500",
  ];

  if (loading) {
    return (
      <MainLayout>
        <div className="max-w-5xl mx-auto flex items-center justify-center h-96">
          <p className="text-slate-500 animate-pulse">
            Profil Bilgileri Getiriliyor...
          </p>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="max-w-5xl mx-auto space-y-10">
        <section className="bg-slate-900/50 border border-slate-800 rounded-[2.5rem] p-10 flex flex-col md:flex-row items-center gap-10 relative overflow-hidden shadow-2xl">
          <div className="absolute top-0 right-0 w-64 h-64 bg-indigo-600/5 blur-[120px] rounded-full"></div>

          <div className="relative">
            <div className="w-32 h-32 bg-gradient-to-tr from-indigo-600 to-cyan-400 rounded-3xl rotate-3 flex items-center justify-center text-5xl shadow-2xl transform hover:rotate-6 transition-transform">
              👤
            </div>
            <div className="absolute -bottom-2 -right-2 bg-slate-950 px-3 py-1 rounded-full border border-slate-800 font-black text-indigo-400 text-sm italic shadow-lg">
              LVL {Math.floor(profile?.generalLevel || 1)}
            </div>
          </div>

          <div className="flex-1 text-center md:text-left z-10">
            <h1 className="text-4xl font-black text-white tracking-tighter uppercase italic mb-1">
              {profile?.name} {profile?.surname}
            </h1>
            <p className="text-slate-500 font-medium">
              @{profile?.userName || "kullanici"} •{" "}
              <span className="text-indigo-400">
                {getActiveDays(profile?.createdAt)} Gündür Aktif
              </span>
            </p>

            <div className="mt-6 flex flex-wrap gap-3 justify-center md:justify-start">
              <div className="px-4 py-2 bg-slate-800/80 rounded-xl border border-slate-700/50 flex flex-col items-center min-w-[100px]">
                <span className="text-[10px] text-slate-500 font-black uppercase tracking-widest">
                  TOPLAM XP
                </span>
                <span className="text-sm font-bold text-white">
                  {profile?.totalXP}
                </span>
              </div>
              <div className="px-4 py-2 bg-slate-800/80 rounded-xl border border-slate-700/50 flex flex-col items-center min-w-[100px]">
                <span className="text-[10px] text-slate-500 font-black uppercase tracking-widest">
                  GÖREVLER
                </span>
                <span className="text-sm font-bold text-emerald-400">
                  {profile?.completedTaskCount} Bitti
                </span>
              </div>
              <div className="px-4 py-2 bg-slate-800/80 rounded-xl border border-slate-700/50 flex flex-col items-center min-w-[100px]">
                <span className="text-[10px] text-slate-500 font-black uppercase tracking-widest">
                  ROZETLER
                </span>
                <span className="text-sm font-bold text-amber-400">
                  {profile?.earnedBadges?.length || 0} Kazanıldı
                </span>
              </div>
            </div>

            <div className="mt-4 flex flex-col gap-2">
              <p className="text-sm text-slate-300 italic">"{profile?.bio || "Henüz bir biyografi eklenmemiş."}"</p>
              <div className="flex items-center gap-4 text-xs text-slate-500">
                {profile?.location && <span>📍 {profile.location}</span>}
                {profile?.interests?.length > 0 && (
                  <div className="flex flex-wrap gap-1">
                    {profile.interests.map(interest => (
                      <span key={interest} className="bg-indigo-500/10 text-indigo-400 px-2 py-0.5 rounded-full border border-indigo-500/20">#{interest}</span>
                    ))}
                  </div>
                )}
              </div>
            </div>

            <button 
              onClick={() => setShowEditModal(true)}
              className="mt-6 px-6 py-2 bg-slate-800 hover:bg-slate-700 text-white text-xs font-bold rounded-xl transition-all border border-slate-700"
            >
              Profili Düzenle
            </button>
          </div>
        </section>

        <section className="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div className="bg-slate-900/40 p-8 rounded-[2rem] border border-slate-800 h-full flex flex-col">
            <h3 className="text-lg font-black text-white mb-6 tracking-widest uppercase italic flex items-center gap-2">
              <span className="text-xl">📊</span> Yetenek İlerlemesi
            </h3>

            <div className="flex-1 flex items-center justify-center min-h-[300px]">
              {profile?.categoryStats && profile.categoryStats.length > 0 ? (
                <div className="w-full h-full -ml-4">
                  <ResponsiveContainer width="100%" height={300}>
                    <RadarChart cx="50%" cy="50%" outerRadius="70%" data={profile.categoryStats.map(stat => ({
                      subject: stat.categoryName,
                      A: stat.level,
                      fullMark: Math.max(...profile.categoryStats.map(s => s.level)) + 5
                    }))}>
                      <PolarGrid stroke="#334155" />
                      <PolarAngleAxis dataKey="subject" tick={{ fill: '#94a3b8', fontSize: 10, fontWeight: 'bold' }} />
                      <PolarRadiusAxis angle={30} domain={[0, 'auto']} tick={false} axisLine={false} />
                      <Radar
                        name="Level"
                        dataKey="A"
                        stroke="#818cf8"
                        strokeWidth={2}
                        fill="#6366f1"
                        fillOpacity={0.5}
                      />
                      <Tooltip 
                        contentStyle={{ backgroundColor: '#0f172a', border: '1px solid #1e293b', borderRadius: '0.75rem' }}
                        itemStyle={{ color: '#818cf8', fontWeight: 'bold' }}
                      />
                    </RadarChart>
                  </ResponsiveContainer>
                </div>
              ) : (
                <div className="text-center opacity-50">
                  <p>Henüz kategori istatistiği oluşmadı.</p>
                  <p className="text-xs mt-2">Görev tamamladıkça burası dolacak.</p>
                </div>
              )}
            </div>
          </div>

          <div className="bg-slate-900/40 p-8 rounded-[2rem] border border-slate-800 flex flex-col h-full">
            <h3 className="text-lg font-black text-white mb-6 tracking-widest uppercase italic flex items-center gap-2">
              <span className="text-xl">🏆</span> Rozet Koleksiyonu
            </h3>

            {profile?.earnedBadges && profile.earnedBadges.length > 0 ? (
              <div className="grid grid-cols-3 gap-4">
                {profile.earnedBadges.slice(0, 6).map((badge, i) => {
                  const showBadgeIcon =
                    badge.iconPath && badge.iconPath.length < 5;

                  return (
                    <div
                      key={i}
                      className="aspect-square bg-slate-950 border border-slate-800 rounded-2xl flex flex-col items-center justify-center p-2 text-center group hover:border-indigo-500/50 transition-all relative overflow-hidden"
                    >
                      <div className="mb-1 group-hover:scale-110 transition-transform z-10">
                        {showBadgeIcon ? (
                          <span className="text-3xl">{badge.iconPath}</span>
                        ) : (
                          <span className="text-2xl font-black text-amber-500 font-mono">
                            {badge.badgeName?.charAt(0).toUpperCase()}
                          </span>
                        )}
                      </div>
                      <p className="text-[9px] font-bold text-slate-400 uppercase leading-tight group-hover:text-indigo-300 z-10">
                        {badge.badgeName}
                      </p>
                    </div>
                  );
                })}
                {profile.earnedBadges.length > 6 && (
                  <div className="aspect-square bg-slate-800/30 border border-slate-800 rounded-2xl flex items-center justify-center text-xs font-bold text-slate-500">
                    +{profile.earnedBadges.length - 6}
                  </div>
                )}
              </div>
            ) : (
              <div className="flex-1 flex flex-col justify-center items-center text-center opacity-40">
                <div className="text-5xl mb-4 grayscale">🛡️</div>
                <p className="text-sm font-medium">Henüz rozet kazanılmadı.</p>
                <p className="text-xs mt-1">
                  Görevleri tamamla, rozetleri kap!
                </p>
              </div>
            )}
          </div>
        </section>

        {}
        <section className="bg-slate-900/40 p-8 rounded-[2rem] border border-slate-800">
          <h3 className="text-lg font-black text-white mb-6 tracking-widest uppercase italic flex items-center gap-2">
            <span className="text-xl">🤖</span> AI Asistan Kişiliği
            {personalitySaving && <span className="text-xs text-indigo-400 font-normal animate-pulse">Kaydediliyor...</span>}
          </h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            {PERSONALITY_OPTIONS.map((opt) => (
              <button
                key={opt.value}
                onClick={() => handlePersonalityChange(opt.value)}
                className={`p-4 rounded-2xl border text-left transition-all ${
                  chatPersonality === opt.value
                    ? "bg-indigo-600/20 border-indigo-500 shadow-lg shadow-indigo-500/10"
                    : "bg-slate-900/60 border-slate-700/50 hover:border-slate-600"
                }`}
              >
                <p className="text-sm font-bold text-white mb-1">{opt.label}</p>
                <p className="text-[11px] text-slate-400">{opt.desc}</p>
              </button>
            ))}
          </div>
          <div className="flex items-center justify-between bg-slate-900/60 rounded-2xl p-4 border border-slate-700/50">
            <div>
              <p className="text-sm font-bold text-white">Proaktif Hatırlatmalar</p>
              <p className="text-[11px] text-slate-400 mt-0.5">Asistan beklenmedik anlarda seni motive etsin</p>
            </div>
            <button
              onClick={handleNudgeToggle}
              className={`relative w-12 h-6 rounded-full transition-all ${
                nudgeEnabled ? "bg-indigo-600" : "bg-slate-700"
              }`}
            >
              <span className={`absolute top-1 w-4 h-4 bg-white rounded-full transition-all ${
                nudgeEnabled ? "left-7" : "left-1"
              }`} />
            </button>
          </div>
        </section>

        {}
        {behaviorSummary && (
          <section>
            <BehaviorInsightCard summary={behaviorSummary} />
          </section>
        )}

        {}
        <section>
          <VacationModeToggle
            isOnVacation={isOnVacation}
            onVacationChange={(val) => setIsOnVacation(val)}
          />
        </section>
        {}
        {showEditModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4">
            <div className="bg-slate-900 border border-slate-700 rounded-3xl w-full max-w-lg overflow-hidden shadow-2xl relative">
              <div className="p-8 space-y-6">
                <h2 className="text-2xl font-black text-white italic tracking-tighter uppercase">Profili Düzenle</h2>
                
                <div className="space-y-4">
                  <div className="space-y-1">
                    <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Biyografi</label>
                    <textarea 
                      value={editData.bio}
                      onChange={(e) => setEditData({...editData, bio: e.target.value})}
                      className="w-full bg-slate-800 border border-slate-700 rounded-xl p-3 text-sm text-white outline-none focus:border-indigo-500 transition-colors min-h-[100px]"
                      placeholder="Kendinden bahset..."
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Konum</label>
                    <input 
                      type="text"
                      value={editData.location}
                      onChange={(e) => setEditData({...editData, location: e.target.value})}
                      className="w-full bg-slate-800 border border-slate-700 rounded-xl p-3 text-sm text-white outline-none focus:border-indigo-500 transition-colors"
                      placeholder="Örn: İstanbul, Türkiye"
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-[10px] font-black text-slate-500 uppercase tracking-widest">İlgi Alanları (Virgülle ayır)</label>
                    <input 
                      type="text"
                      value={editData.interests.join(", ")}
                      onChange={(e) => setEditData({...editData, interests: e.target.value.split(",").map(s => s.trim()).filter(s => s !== "")})}
                      className="w-full bg-slate-800 border border-slate-700 rounded-xl p-3 text-sm text-white outline-none focus:border-indigo-500 transition-colors"
                      placeholder="Örn: Yazılım, Fitness, Kitap"
                    />
                  </div>
                </div>

                <div className="flex gap-4 pt-4">
                  <button 
                    onClick={() => setShowEditModal(false)}
                    className="flex-1 px-6 py-3 bg-slate-800 hover:bg-slate-700 text-slate-300 font-bold rounded-2xl transition-all"
                  >
                    İptal
                  </button>
                  <button 
                    onClick={handleUpdateProfile}
                    disabled={saving}
                    className="flex-1 px-6 py-3 bg-indigo-600 hover:bg-indigo-500 text-white font-bold rounded-2xl transition-all shadow-lg shadow-indigo-500/20 disabled:opacity-50"
                  >
                    {saving ? "Kaydediliyor..." : "Kaydet"}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export default ProfilePage;
