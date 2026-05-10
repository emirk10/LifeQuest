import React, { useState, useEffect, useCallback } from 'react';
import MainLayout from '../layouts/MainLayout';
import socialService from '../services/socialService';
import ChallengeCard from '../components/ChallengeCard';
import { challengeService } from '../services/challengeService';
import categoryService from '../services/categoryService';
import useSignalR from '../hooks/useSignalR';

const tabs = ['Arkadaşlarım', 'İstekler', 'Mücadeleler', 'Kullanıcı Ara', 'Engellenenler'];
const CHALLENGE_STATUS_TABS = ['active', 'invited', 'completed'];

const SocialPage = () => {
  const [activeTab, setActiveTab] = useState(0);
  const [challengeStatus, setChallengeStatus] = useState('active');
  const [friends, setFriends] = useState([]);
  const [requests, setRequests] = useState([]);
  const [challenges, setChallenges] = useState([]);
  const [categories, setCategories] = useState([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState([]);
  const [blockedUsers, setBlockedUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [toast, setToast] = useState(null);
  const [searching, setSearching] = useState(false);
  const [showChallengeModal, setShowChallengeModal] = useState(false);
  const [challengeTarget, setChallengeTarget] = useState(null);
  const [challengeForm, setChallengeForm] = useState({
    title: '',
    challengeType: 'TaskCount',
    targetValue: 10,
    durationDays: 7,
    challengerMessage: '',
    targetCategoryId: '',
  });

  // Real-time challenge updates
  useSignalR({
    onNotification: (notif) => {
      if (notif?.type === 'ChallengeProgressUpdated' || notif?.type === 'ChallengeCompleted') {
        fetchChallenges();
      }
    }
  });


  const showToast = (message, type = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 3500);
  };

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      // Ayrıştırılmış hata yönetimi: Bir servis çökerse diğerleri çalışmaya devam etsin
      const safeFetch = async (fn, setter, defaultValue = []) => {
        try {
          const res = await fn();
          setter(res);
        } catch (err) {
          console.error(`Fetch error:`, err);
          setter(defaultValue);
        }
      };

      await Promise.all([
        safeFetch(socialService.getFriends, setFriends),
        safeFetch(socialService.getFriendRequests, setRequests),
        safeFetch(categoryService.getAllCategories, setCategories),
        safeFetch(socialService.getBlockedUsers, setBlockedUsers)
      ]);
    } catch (err) {
      console.error("General fetch error", err);
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchChallenges = useCallback(async () => {
    try {
      const result = await challengeService.getActiveChallenges(challengeStatus);
      setChallenges(result?.challenges || result || []);
    } catch (err) {
      console.error(err);
    }
  }, [challengeStatus]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  useEffect(() => {
    if (activeTab === 2) fetchChallenges();
  }, [fetchChallenges, activeTab, challengeStatus]);

  const handleSearch = async () => {
    if (searchQuery.length < 2) return;
    setSearching(true);
    try {
      const results = await socialService.searchUsers(searchQuery);
      setSearchResults(results);
    } catch (err) {
      showToast('Arama sırasında hata oluştu.', 'error');
    } finally {
      setSearching(false);
    }
  };

  const handleSendRequest = async (userId) => {
    try {
      await socialService.sendFriendRequest(userId);
      showToast('Arkadaşlık isteği gönderildi! 🤝');
      setSearchResults(prev => prev.filter(u => u.id !== userId));
    } catch (err) {
      showToast(err.response?.data?.message || 'İstek gönderilemedi.', 'error');
    }
  };

  const handleAccept = async (requestId) => {
    try {
      await socialService.acceptFriendRequest(requestId);
      showToast('Arkadaşlık isteği kabul edildi! 🎉');
      fetchData();
    } catch (err) {
      showToast('İşlem başarısız.', 'error');
    }
  };

  const handleReject = async (requestId) => {
    try {
      await socialService.rejectFriendRequest(requestId);
      showToast('İstek reddedildi.');
      setRequests(prev => prev.filter(r => r.requestId !== requestId));
    } catch (err) {
      showToast('İşlem başarısız.', 'error');
    }
  };

  const handleRemoveFriend = async (friendshipId) => {
    if (!confirm('Arkadaşını listeden çıkarmak istediğine emin misin?')) return;
    try {
      await socialService.removeFriend(friendshipId);
      showToast('Arkadaşlıktan çıkarıldı.');
      fetchData();
    } catch (err) {
      showToast('İşlem başarısız.', 'error');
    }
  };

  const handleBlock = async (userId) => {
    if (!confirm('Bu kullanıcıyı engellemek istediğine emin misin? Arkadaşlıktan çıkarılacak ve sana ulaşamayacak.')) return;
    try {
      await socialService.blockUser(userId);
      showToast('Kullanıcı engellendi.');
      fetchData();
    } catch (err) {
      showToast('İşlem başarısız.', 'error');
    }
  };

  const handleUnblock = async (userId) => {
    try {
      await socialService.unblockUser(userId);
      showToast('Engel kaldırıldı.');
      fetchData();
    } catch (err) {
      showToast('İşlem başarısız.', 'error');
    }
  };

  const openChallengeModal = (friend) => {
    setChallengeTarget(friend);
    setShowChallengeModal(true);
  };

  const handleCreateChallenge = async (e) => {
    e.preventDefault();
    try {
      await challengeService.sendInvite({
        opponentId: challengeTarget.friendId,
        title: challengeForm.title,
        challengeType: challengeForm.challengeType,
        targetValue: challengeForm.targetValue,
        durationDays: challengeForm.durationDays,
        challengerMessage: challengeForm.challengerMessage,
        targetCategoryId: challengeForm.challengeType === 'CategoryFocus' ? challengeForm.targetCategoryId : null,
      });
      showToast(`${challengeTarget.name}'e mücadele daveti gönderildi! ⚔️`);
      setShowChallengeModal(false);
      fetchChallenges();
    } catch (err) {
      console.error("Challenge error details:", err.response?.data);
      const errorMessage = err.response?.data?.message || err.response?.data?.Message || 'Mücadele oluşturulamadı.';
      showToast(errorMessage, 'error');
    }
  };

  return (
    <MainLayout>
      {/* Toast */}
      {toast && (
        <div className={`fixed top-6 right-6 z-50 px-6 py-4 rounded-2xl font-bold text-sm shadow-2xl ${
          toast.type === 'success' ? 'bg-emerald-600 text-white' : 'bg-rose-600 text-white'
        }`}>
          {toast.message}
        </div>
      )}

      <div className="space-y-8">
        {/* Header */}
        <section className="relative">
          <div className="absolute -left-10 -top-10 w-40 h-40 bg-violet-600/10 blur-[80px] rounded-full"></div>
          <h2 className="text-4xl font-black text-white italic tracking-tighter mb-2 relative z-10">SOSYAL</h2>
          <div className="h-1.5 w-24 bg-violet-600 rounded-full shadow-[0_0_20px_rgba(124,58,237,0.6)]"></div>
          <div className="flex gap-4 mt-4 flex-wrap">
            <div className="bg-slate-900/60 px-4 py-2 rounded-xl border border-slate-800 text-center">
              <p className="text-2xl font-black text-white">{friends.length}</p>
              <p className="text-[10px] text-slate-500 uppercase tracking-wider font-bold">Arkadaş</p>
            </div>
            <div className="bg-slate-900/60 px-4 py-2 rounded-xl border border-amber-500/20 text-center">
              <p className="text-2xl font-black text-amber-400">{requests.length}</p>
              <p className="text-[10px] text-slate-500 uppercase tracking-wider font-bold">Bekleyen İstek</p>
            </div>
            <div className="bg-slate-900/60 px-4 py-2 rounded-xl border border-rose-500/20 text-center">
              <p className="text-2xl font-black text-rose-400">{challenges.filter(c => c.status === 'Active').length}</p>
              <p className="text-[10px] text-slate-500 uppercase tracking-wider font-bold">Aktif Mücadele</p>
            </div>
          </div>
        </section>

        {/* Tabs */}
        <div className="flex gap-2 overflow-x-auto pb-2">
          {tabs.map((tab, i) => (
            <button
              key={tab}
              onClick={() => setActiveTab(i)}
              className={`px-5 py-2.5 rounded-xl text-xs font-black uppercase tracking-wider whitespace-nowrap transition-all ${
                activeTab === i
                  ? 'bg-violet-600 text-white shadow-lg shadow-violet-500/30'
                  : 'bg-slate-900/60 text-slate-400 hover:text-white border border-slate-800'
              }`}
            >
              {tab}
              {i === 1 && requests.length > 0 && (
                <span className="ml-1.5 bg-amber-500 text-black text-[9px] px-1.5 py-0.5 rounded-full font-black">
                  {requests.length}
                </span>
              )}
            </button>
          ))}
        </div>

        {/* Tab Content */}
        {loading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {[1, 2, 3].map(i => (
              <div key={i} className="h-24 bg-slate-900/40 rounded-2xl animate-pulse border border-slate-800"></div>
            ))}
          </div>
        ) : (
          <>
            {/* ARKADAŞLARIM */}
            {activeTab === 0 && (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {friends.length === 0 ? (
                  <div className="col-span-2 bg-slate-900/40 rounded-2xl border border-dashed border-slate-700 p-12 text-center">
                    <p className="text-4xl mb-4">🤝</p>
                    <p className="text-slate-400 font-bold">Henüz arkadaşın yok.</p>
                    <p className="text-slate-600 text-sm mt-1">Kullanıcı Ara sekmesinden arkadaş ekleyebilirsin!</p>
                  </div>
                ) : (
                  friends.map((f) => (
                    <div key={f.friendshipId} className="bg-slate-900/60 p-5 rounded-2xl border border-slate-800 hover:border-violet-500/40 transition-colors group flex items-center justify-between gap-4">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-gradient-to-br from-violet-600 to-indigo-600 rounded-full flex items-center justify-center font-black text-white text-lg shadow-lg">
                          {f.name.charAt(0)}{f.surname.charAt(0)}
                        </div>
                        <div>
                          <p className="text-white font-bold">{f.name} {f.surname}</p>
                          <p className="text-slate-500 text-xs">@{f.userName}</p>
                          <div className="flex gap-2 mt-1">
                            <span className="text-[10px] text-indigo-400 font-bold">Lv. {f.level}</span>
                            <span className="text-[10px] text-slate-600">•</span>
                            <span className="text-[10px] text-amber-400 font-bold">{f.totalXP?.toLocaleString()} XP</span>
                          </div>
                        </div>
                      </div>
                      <div className="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                        <button
                          onClick={() => openChallengeModal(f)}
                          className="px-3 py-1.5 text-[10px] font-black rounded-lg bg-rose-500/20 text-rose-400 hover:bg-rose-500 hover:text-white transition-all uppercase"
                        >
                          ⚔️ Mücadele
                        </button>
                        <button
                          onClick={() => handleRemoveFriend(f.friendshipId)}
                          className="px-3 py-1.5 text-[10px] font-black rounded-lg bg-slate-800 text-slate-500 hover:bg-slate-700 transition-all uppercase"
                        >
                          Çıkar
                        </button>
                        <button
                          onClick={() => handleBlock(f.friendId)}
                          className="px-3 py-1.5 text-[10px] font-black rounded-lg bg-slate-800/50 text-rose-500/50 hover:bg-rose-500 hover:text-white transition-all uppercase"
                          title="Engelle"
                        >
                          🚫
                        </button>
                      </div>
                    </div>
                  ))
                )}
              </div>
            )}

            {/* BEKLEYEN İSTEKLER */}
            {activeTab === 1 && (
              <div className="space-y-4">
                {requests.length === 0 ? (
                  <div className="bg-slate-900/40 rounded-2xl border border-dashed border-slate-700 p-12 text-center">
                    <p className="text-4xl mb-4">📬</p>
                    <p className="text-slate-400 font-bold">Bekleyen istek yok.</p>
                  </div>
                ) : (
                  requests.map((r) => (
                    <div key={r.requestId} className="bg-slate-900/60 p-5 rounded-2xl border border-amber-500/20 flex items-center justify-between gap-4">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-gradient-to-br from-amber-500 to-orange-600 rounded-full flex items-center justify-center font-black text-white text-lg">
                          {r.requesterName.charAt(0)}{r.requesterSurname.charAt(0)}
                        </div>
                        <div>
                          <p className="text-white font-bold">{r.requesterName} {r.requesterSurname}</p>
                          <p className="text-slate-500 text-xs">@{r.requesterUserName} • Lv. {r.requesterLevel}</p>
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleAccept(r.requestId)}
                          className="px-4 py-2 text-xs font-black rounded-xl bg-emerald-600 text-white hover:bg-emerald-500 transition-all uppercase shadow-lg shadow-emerald-500/20"
                        >
                          Kabul
                        </button>
                        <button
                          onClick={() => handleReject(r.requestId)}
                          className="px-4 py-2 text-xs font-black rounded-xl bg-slate-800 text-slate-400 hover:bg-slate-700 transition-all uppercase"
                        >
                          Reddet
                        </button>
                      </div>
                    </div>
                  ))
                )}
              </div>
            )}

            {/* SP-05: MÜCADELELER */}
            {activeTab === 2 && (
              <div className="space-y-4">
                {/* Status filter */}
                <div className="flex gap-2">
                  {[['active','Aktif'],['invited','Davetler'],['completed','Tamamlanan']].map(([s, l]) => (
                    <button key={s}
                      onClick={() => { setChallengeStatus(s); }}
                      className={`px-3 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-wider transition-all ${
                        challengeStatus === s
                          ? 'bg-rose-600 text-white shadow-lg'
                          : 'bg-slate-900/60 text-slate-400 border border-slate-800 hover:text-white'
                      }`}>
                      {l}
                    </button>
                  ))}
                </div>

                {challenges.length === 0 ? (
                  <div className="bg-slate-900/40 rounded-2xl border border-dashed border-slate-700 p-12 text-center">
                    <p className="text-4xl mb-4">⚔️</p>
                    <p className="text-slate-400 font-bold">Henüz {challengeStatus === 'invited' ? 'bekleyen davet' : challengeStatus === 'completed' ? 'tamamlanan mücadele' : 'aktif mücadele'} yok.</p>
                    {challengeStatus === 'active' && <p className="text-slate-600 text-sm mt-1">Arkadaşlarına mücadele daveti gönder!</p>}
                  </div>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {challenges.map(c => (
                      <ChallengeCard key={c.id} challenge={c} onUpdate={fetchChallenges} />
                    ))}
                  </div>
                )}
              </div>
            )}

            {/* KULLANICI ARA */}
            {activeTab === 3 && (
              <div className="space-y-6">
                <div className="flex gap-3">
                  <input
                    type="text"
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                    placeholder="İsim veya kullanıcı adı ara..."
                    className="flex-1 bg-slate-900/60 border border-slate-800 rounded-xl px-4 py-3 text-slate-200 text-sm focus:outline-none focus:border-violet-500 transition-colors placeholder:text-slate-600"
                  />
                  <button
                    onClick={handleSearch}
                    disabled={searching || searchQuery.length < 2}
                    className="px-6 py-3 rounded-xl bg-violet-600 hover:bg-violet-500 text-white font-bold text-sm transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {searching ? '...' : '🔍 Ara'}
                  </button>
                </div>

                <div className="space-y-3">
                  {searchResults.map((user) => (
                    <div key={user.id} className="bg-slate-900/60 p-4 rounded-2xl border border-slate-800 hover:border-violet-500/30 transition-colors flex items-center justify-between gap-4">
                      <div className="flex items-center gap-4">
                        <div className="w-10 h-10 bg-gradient-to-br from-violet-600 to-indigo-600 rounded-full flex items-center justify-center font-black text-white">
                          {user.name.charAt(0)}{user.surname.charAt(0)}
                        </div>
                        <div>
                          <p className="text-white font-bold text-sm">{user.name} {user.surname}</p>
                          <p className="text-slate-500 text-xs">@{user.userName} • Lv. {user.level}</p>
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleSendRequest(user.id)}
                          className="px-4 py-2 rounded-xl bg-violet-600/20 text-violet-400 hover:bg-violet-600 hover:text-white text-xs font-black transition-all uppercase"
                        >
                          + Arkadaş Ekle
                        </button>
                        <button
                          onClick={() => handleBlock(user.id)}
                          className="p-2 rounded-xl bg-slate-800 text-slate-500 hover:bg-rose-600 hover:text-white transition-all"
                          title="Engelle"
                        >
                          🚫
                        </button>
                      </div>
                    </div>
                  ))}

                  {searchResults.length === 0 && searchQuery.length >= 2 && !searching && (
                    <p className="text-slate-500 text-center py-8">Kullanıcı bulunamadı.</p>
                  )}
                </div>
              </div>
            )}

            {/* ENGELLENENLER */}
            {activeTab === 4 && (
              <div className="space-y-4">
                {blockedUsers.length === 0 ? (
                  <div className="bg-slate-900/40 rounded-2xl border border-dashed border-slate-700 p-12 text-center">
                    <p className="text-4xl mb-4">🛡️</p>
                    <p className="text-slate-400 font-bold">Engellenmiş kullanıcı yok.</p>
                  </div>
                ) : (
                  blockedUsers.map((u) => (
                    <div key={u.id} className="bg-slate-900/60 p-5 rounded-2xl border border-rose-500/10 flex items-center justify-between gap-4">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 bg-slate-800 rounded-full flex items-center justify-center font-black text-slate-500">
                          {u.name.charAt(0)}{u.surname.charAt(0)}
                        </div>
                        <div>
                          <p className="text-slate-200 font-bold">{u.name} {u.surname}</p>
                          <p className="text-slate-500 text-xs">@{u.userName}</p>
                        </div>
                      </div>
                      <button
                        onClick={() => handleUnblock(u.id)}
                        className="px-4 py-2 text-xs font-black rounded-xl bg-slate-800 text-slate-400 hover:bg-white hover:text-black transition-all uppercase border border-slate-700"
                      >
                        Engeli Kaldır
                      </button>
                    </div>
                  ))
                )}
              </div>
            )}
          </>
        )}
      </div>

      {/* SP-05: Mücadele Modal */}
      {showChallengeModal && challengeTarget && (
        <div className="fixed inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-3xl p-8 w-full max-w-md shadow-2xl">
            <h3 className="text-2xl font-bold text-white mb-1">⚔️ Mücadele Başlat</h3>
            <p className="text-slate-400 text-sm mb-6">
              <span className="text-violet-400 font-bold">{challengeTarget.name} {challengeTarget.surname}</span>'e mücadele daveti
            </p>
            <form onSubmit={handleCreateChallenge} className="space-y-4">
              <div>
                <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Mücadele Adı *</label>
                <input required type="text" className="w-full bg-slate-800 rounded-xl text-white p-3 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
                  value={challengeForm.title} onChange={e => setChallengeForm({ ...challengeForm, title: e.target.value })}
                  placeholder="Örn: Haftalık Görev Mücadelesi" />
              </div>
              <div>
                <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Mücadele Tipi</label>
                <select className="w-full bg-slate-800 rounded-xl text-white p-3 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
                  value={challengeForm.challengeType}
                  onChange={e => setChallengeForm({ ...challengeForm, challengeType: e.target.value })}>
                  <option value="TaskCount">📋 Görev Sayısı</option>
                  <option value="XPRace">⭐ XP Yarışı</option>
                  <option value="StreakBattle">🔥 Seri Mücadelesi</option>
                  <option value="CategoryFocus">🎯 Kategori Odağı</option>
                </select>
              </div>
              {challengeForm.challengeType === 'CategoryFocus' && (
                <div>
                  <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Hedef Kategori *</label>
                  <select required className="w-full bg-slate-800 rounded-xl text-white p-3 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
                    value={challengeForm.targetCategoryId}
                    onChange={e => setChallengeForm({ ...challengeForm, targetCategoryId: e.target.value })}>
                    <option value="">Kategori Seçin...</option>
                    {categories.map(cat => (
                      <option key={cat.id} value={cat.id}>{cat.name}</option>
                    ))}
                  </select>
                </div>
              )}
              <div className="flex gap-4">
                <div className="flex-1">
                  <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Hedef Sayı</label>
                  <input type="number" min="1" max="500" className="w-full bg-slate-800 rounded-xl text-white p-3 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
                    value={challengeForm.targetValue}
                    onChange={e => setChallengeForm({ ...challengeForm, targetValue: parseInt(e.target.value) })} />
                </div>
                <div className="flex-1">
                  <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Süre (Gün)</label>
                  <input type="number" min="1" max="90" className="w-full bg-slate-800 rounded-xl text-white p-3 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
                    value={challengeForm.durationDays}
                    onChange={e => setChallengeForm({ ...challengeForm, durationDays: parseInt(e.target.value) })} />
                </div>
              </div>
              <div>
                <label className="text-xs text-slate-400 font-bold uppercase tracking-wider mb-1 block">Mesaj (opsiyonel)</label>
                <input type="text" className="w-full bg-slate-800 rounded-xl text-white p-3 text-sm focus:outline-none focus:ring-2 focus:ring-violet-500"
                  placeholder="Rakibe mesaj..."
                  value={challengeForm.challengerMessage}
                  onChange={e => setChallengeForm({ ...challengeForm, challengerMessage: e.target.value })} />
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <button type="button" onClick={() => setShowChallengeModal(false)} className="px-5 py-2.5 rounded-xl font-bold text-slate-400 hover:text-white transition-colors text-sm">İptal</button>
                <button type="submit" className="px-6 py-2.5 rounded-xl font-bold bg-rose-600 text-white hover:bg-rose-500 shadow-lg shadow-rose-900/50 text-sm transition-all">⚔️ Mücadele Başlat</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </MainLayout>
  );
};

export default SocialPage;
