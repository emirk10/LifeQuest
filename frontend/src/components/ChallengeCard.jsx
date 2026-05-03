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

  // Canlı sayaç
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

  const myPct = challenge.targetValue
    ? Math.min(100, Math.round((challenge.myProgress / challenge.targetValue) * 100))
    : 0;
  const opPct = challenge.targetValue
    ? Math.min(100, Math.round((challenge.opponentProgress / challenge.targetValue) * 100))
    : 0;

  const isWinner = challenge.winnerId && challenge.iAmChallenger
    ? challenge.winnerId === challenge.opponentId ? false : true
    : challenge.winnerId && !challenge.iAmChallenger
    ? challenge.winnerId !== challenge.opponentId : false;

  return (
    <div style={{
      background: 'rgba(255,255,255,0.04)',
      border: '1px solid rgba(255,255,255,0.08)',
      borderRadius: '16px',
      padding: '1.25rem',
      marginBottom: '0.75rem',
      transition: 'all 0.2s'
    }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '1rem' }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.25rem' }}>
            <span>{TYPE_ICONS[challenge.challengeType] || '⚔️'}</span>
            <span style={{ fontWeight: 700, color: '#fff', fontSize: '0.95rem' }}>{challenge.title}</span>
          </div>
          <div style={{ fontSize: '0.78rem', color: 'rgba(255,255,255,0.4)' }}>
            vs {challenge.opponentName} • {challenge.status === 'Active' ? `⏱️ ${timeLeft}` : challenge.status}
          </div>
        </div>
        <div style={{
          padding: '0.25rem 0.6rem',
          borderRadius: '8px',
          fontSize: '0.72rem',
          fontWeight: 600,
          background: challenge.status === 'Active' ? 'rgba(16,185,129,0.15)' : 'rgba(255,255,255,0.07)',
          color: challenge.status === 'Active' ? '#10b981' : 'rgba(255,255,255,0.4)',
          border: `1px solid ${challenge.status === 'Active' ? 'rgba(16,185,129,0.3)' : 'rgba(255,255,255,0.1)'}`
        }}>
          {challenge.status === 'Invited' ? '⏳ Davet' : challenge.status === 'Active' ? 'Aktif' : challenge.status}
        </div>
      </div>

      {/* Progress Bars */}
      {challenge.status === 'Active' && (
        <div style={{ marginBottom: '1rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', marginBottom: '0.3rem' }}>
            <span style={{ color: '#a78bfa', fontWeight: 600 }}>Sen: {challenge.myProgress}{challenge.targetValue ? `/${challenge.targetValue}` : ''}</span>
            <span style={{ color: 'rgba(255,255,255,0.5)', fontWeight: 600 }}>Rakip: {challenge.opponentProgress}{challenge.targetValue ? `/${challenge.targetValue}` : ''}</span>
          </div>
          <div style={{ height: '6px', background: 'rgba(255,255,255,0.08)', borderRadius: '99px', overflow: 'hidden', marginBottom: '0.3rem' }}>
            <div style={{ height: '100%', width: `${myPct}%`, background: 'linear-gradient(90deg, #7c3aed, #4f46e5)', borderRadius: '99px', transition: 'width 0.5s' }} />
          </div>
          <div style={{ height: '6px', background: 'rgba(255,255,255,0.08)', borderRadius: '99px', overflow: 'hidden' }}>
            <div style={{ height: '100%', width: `${opPct}%`, background: 'linear-gradient(90deg, #ef4444, #dc2626)', borderRadius: '99px', transition: 'width 0.5s' }} />
          </div>
        </div>
      )}

      {/* Completed */}
      {challenge.status === 'Completed' && (
        <div style={{
          textAlign: 'center', padding: '0.75rem',
          background: isWinner ? 'rgba(16,185,129,0.1)' : 'rgba(255,255,255,0.04)',
          borderRadius: '10px', marginBottom: '1rem',
          color: isWinner ? '#10b981' : 'rgba(255,255,255,0.5)',
          fontWeight: 600, fontSize: '0.9rem'
        }}>
          {isWinner ? '🏆 Kazandın! ' : '💪 Tamamlandı'}
          {isWinner && `+${challenge.winnerXPBonus} XP`}
        </div>
      )}

      {/* Invite Actions */}
      {challenge.status === 'Invited' && !challenge.iAmChallenger && (
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button
            onClick={() => handleRespond(true)}
            disabled={responding}
            style={{
              flex: 1, padding: '0.5rem', background: 'rgba(16,185,129,0.2)',
              border: '1px solid rgba(16,185,129,0.4)', borderRadius: '10px',
              color: '#10b981', fontWeight: 600, cursor: 'pointer', fontSize: '0.85rem'
            }}
          >
            ✅ Kabul Et
          </button>
          <button
            onClick={() => handleRespond(false)}
            disabled={responding}
            style={{
              flex: 1, padding: '0.5rem', background: 'rgba(239,68,68,0.1)',
              border: '1px solid rgba(239,68,68,0.3)', borderRadius: '10px',
              color: '#ef4444', fontWeight: 600, cursor: 'pointer', fontSize: '0.85rem'
            }}
          >
            ❌ Reddet
          </button>
        </div>
      )}

      {challenge.challengerMessage && (
        <div style={{ fontSize: '0.78rem', color: 'rgba(255,255,255,0.35)', marginTop: '0.5rem', fontStyle: 'italic' }}>
          "{challenge.challengerMessage}"
        </div>
      )}
    </div>
  );
};

export default ChallengeCard;
