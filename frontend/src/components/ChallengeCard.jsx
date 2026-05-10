import React, { useEffect, useState } from 'react';
import { challengeService } from '../services/challengeService';

const TYPE_ICONS = {
  TaskCount: '📋',
  XPRace: '⭐',
  StreakBattle: '🔥',
  CategoryFocus: '🎯',
  SpecificTask: '✅'
};

const ChallengeCard = ({ challenge, onUpdate }) => {
  const [timeLeft, setTimeLeft] = useState('');
  const [responding, setResponding] = useState(false);
  const [showDetails, setShowDetails] = useState(false);
  const [details, setDetails] = useState(null);
  const [loadingDetails, setLoadingDetails] = useState(false);

  useEffect(() => {
    const calc = () => {
      const end = new Date(challenge.endDate);
      const diff = end - Date.now();
      if (diff <= 0) { setTimeLeft('Sona erdi'); return; }
      const d = Math.floor(diff / 86400000);
      const h = Math.floor((diff % 86400000) / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      setTimeLeft(d > 0 ? `${d}g ${h}s` : `${h}s ${m}dk`);
    };
    calc();
    const id = setInterval(calc, 60000);
    return () => clearInterval(id);
  }, [challenge.endDate]);

  const handleRespond = async (accept) => {
    setResponding(true);
    try {
      await challengeService.respondToChallenge(challenge.id, accept);
      onUpdate?.();
    } catch (err) {
      console.error(err);
    } finally {
      setResponding(false);
    }
  };

  const fetchDetails = async () => {
    setShowDetails(true);
    setLoadingDetails(true);
    try {
      const data = await challengeService.getChallengeDetails(challenge.id);
      setDetails(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingDetails(false);
    }
  };

  const myPct = challenge.targetValue
    ? Math.min(100, Math.round((challenge.myProgress / challenge.targetValue) * 100))
    : 0;
  const opPct = challenge.targetValue
    ? Math.min(100, Math.round((challenge.opponentProgress / challenge.targetValue) * 100))
    : 0;

  const isWinner = challenge.winnerId && (
    challenge.iAmChallenger 
      ? challenge.winnerId === challenge.challengerId 
      : challenge.winnerId === challenge.opponentId
  );

  return (
    <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-5 hover:border-rose-500/30 transition-all group shadow-xl">
      <div className="flex justify-between items-start mb-4">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <span className="text-xl">{TYPE_ICONS[challenge.challengeType] || '⚔️'}</span>
            <h4 className="font-black text-white text-sm uppercase italic tracking-tight">{challenge.title}</h4>
          </div>
          <p className="text-[10px] text-slate-500 font-bold uppercase tracking-widest">
            vs {challenge.opponentName} • {challenge.status === 'Active' ? `⏱️ ${timeLeft}` : challenge.status}
          </p>
        </div>
        <div className={`px-2 py-1 rounded-lg text-[9px] font-black uppercase tracking-tighter border ${
          challenge.status === 'Active' ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400' : 'bg-slate-800 border-slate-700 text-slate-500'
        }`}>
          {challenge.status === 'Invited' ? '⏳ Davet' : challenge.status === 'Active' ? 'Aktif' : 'Bitti'}
        </div>
      </div>

      {challenge.status === 'Active' && (
        <div className="space-y-3 mb-4">
          <div className="space-y-1">
            <div className={`flex justify-between text-[10px] font-black uppercase tracking-widest ${challenge.challengeType === 'XPRace' ? 'text-amber-400' : 'text-indigo-400'}`}>
              <span>Sen</span>
              <span>{challenge.myProgress.toLocaleString()} / {challenge.targetValue.toLocaleString()} {challenge.challengeType === 'XPRace' ? 'XP' : 'birim'}</span>
            </div>
            <div className="h-2 bg-slate-950 rounded-full overflow-hidden border border-slate-800">
              <div 
                className={`h-full transition-all duration-1000 ${
                  challenge.challengeType === 'XPRace' ? 'bg-gradient-to-r from-amber-600 to-amber-400' : 
                  challenge.challengeType === 'StreakBattle' ? 'bg-gradient-to-r from-orange-600 to-orange-400' : 
                  'bg-gradient-to-r from-indigo-600 to-indigo-400'
                }`} 
                style={{ width: `${myPct}%` }} 
              />
            </div>
          </div>
          <div className="space-y-1">
            <div className={`flex justify-between text-[10px] font-black uppercase tracking-widest ${challenge.challengeType === 'XPRace' ? 'text-amber-500/80' : 'text-rose-400'}`}>
              <span>Rakip</span>
              <span>{challenge.opponentProgress.toLocaleString()} / {challenge.targetValue.toLocaleString()} {challenge.challengeType === 'XPRace' ? 'XP' : 'birim'}</span>
            </div>
            <div className="h-2 bg-slate-950 rounded-full overflow-hidden border border-slate-800">
              <div 
                className={`h-full transition-all duration-1000 ${
                  challenge.challengeType === 'XPRace' ? 'bg-gradient-to-r from-amber-700/50 to-amber-500/50' : 
                  'bg-gradient-to-r from-rose-600 to-rose-400'
                }`} 
                style={{ width: `${opPct}%` }} 
              />
            </div>
          </div>
          
          <button 
            onClick={fetchDetails}
            className="w-full mt-2 py-2 bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white text-[10px] font-black uppercase tracking-widest rounded-xl transition-all border border-slate-700/50"
          >
            Sıralamayı Gör
          </button>
        </div>
      )}

      {challenge.status === 'Invited' && !challenge.iAmChallenger && (
        <div className="flex gap-2 mb-4">
          <button
            onClick={() => handleRespond(true)}
            disabled={responding}
            className="flex-1 py-2 bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-black rounded-xl transition-all uppercase shadow-lg shadow-emerald-900/20"
          >
            {responding ? '...' : '✅ Kabul'}
          </button>
          <button
            onClick={() => handleRespond(false)}
            disabled={responding}
            className="flex-1 py-2 bg-slate-800 hover:bg-slate-700 text-slate-400 text-xs font-black rounded-xl transition-all uppercase"
          >
            {responding ? '...' : '❌ Red'}
          </button>
        </div>
      )}

      {challenge.status === 'Completed' && (
        <div className={`text-center p-3 rounded-xl mb-4 font-black text-xs uppercase tracking-widest border ${
          isWinner ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400' : 'bg-slate-800 border-slate-700 text-slate-500'
        }`}>
          {isWinner ? '🏆 KAZANDIN!' : '🏁 TAMAMLANDI'}
        </div>
      )}

      {challenge.challengerMessage && (
        <p className="text-[10px] text-slate-500 italic border-t border-slate-800/50 pt-3">
          "{challenge.challengerMessage}"
        </p>
      )}

      {/* Details Modal */}
      {showDetails && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/80 backdrop-blur-sm p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-3xl w-full max-w-md overflow-hidden shadow-2xl">
            <div className="p-6 border-b border-slate-800 flex justify-between items-center">
              <h3 className="text-lg font-black text-white italic uppercase tracking-tighter">Mücadele Sıralaması</h3>
              <button onClick={() => setShowDetails(false)} className="text-slate-500 hover:text-white">✕</button>
            </div>
            <div className="p-6 space-y-4 max-h-[400px] overflow-y-auto">
              {loadingDetails ? (
                <div className="py-8 text-center animate-pulse text-slate-500">Yükleniyor...</div>
              ) : (
                details?.participants?.sort((a, b) => b.score - a.score).map((p, idx) => (
                  <div key={p.userId} className={`flex items-center gap-4 p-3 rounded-2xl border ${p.isMe ? 'bg-indigo-600/10 border-indigo-500/30' : 'bg-slate-800/50 border-slate-700/50'}`}>
                    <div className="w-6 text-center font-black text-slate-500 text-xs">{idx + 1}.</div>
                    <div className="flex-1">
                      <p className="text-white font-bold text-sm">{p.userName} {p.isMe && '(Sen)'}</p>
                      <div className="h-1.5 bg-slate-950 rounded-full mt-1.5 overflow-hidden">
                        <div className="h-full bg-indigo-500" style={{ width: `${(p.progress / challenge.targetValue) * 100}%` }} />
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="text-indigo-400 font-black text-xs">{p.score} Puan</p>
                      <p className="text-[10px] text-slate-500 font-bold">{p.progress} / {challenge.targetValue}</p>
                    </div>
                  </div>
                ))
              )}
            </div>
            <div className="p-6 bg-slate-900/50">
              <button 
                onClick={() => setShowDetails(false)}
                className="w-full py-3 bg-slate-800 hover:bg-slate-700 text-white font-bold rounded-2xl transition-all"
              >
                Kapat
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ChallengeCard;
