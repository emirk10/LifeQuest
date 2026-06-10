import React, { useEffect, useState } from 'react';

const AVATARS = ['🦁','🐯','🦊','🐺','🦅','🦋','🐉','🦄','🐬','🦈','🐸','🐼','🐻','🦝','🐮','🐧','🦩','🐙','🦕','🤖'];

const FOCUS_LABELS = {
  health: '💪 Sağlık & Spor',
  career: '🚀 Kariyer & İş',
  education: '🎓 Öğrenme',
  social: '🤝 İlişkiler',
  mindfulness: '🧘 Zihinsel Sağlık',
  finance: '💰 Finans',
};

const StepSummary = ({ answers, result, onFinish }) => {
  const [confetti, setConfetti] = useState([]);

  useEffect(() => {
    
    const items = Array.from({ length: 20 }, (_, i) => ({
      id: i,
      left: `${Math.random() * 100}%`,
      delay: `${Math.random() * 2}s`,
      duration: `${1.5 + Math.random() * 2}s`,
      color: ['#7c3aed','#06b6d4','#10b981','#f59e0b','#ef4444'][i % 5]
    }));
    setConfetti(items);
  }, []);

  const avatar = AVATARS[(answers.avatarId || 1) - 1];
  const dailyLimit = result?.dailyTaskLimit || Math.max(1, Math.floor((answers.preferredTasksPerWeek || 5) / 7));

  return (
    <div style={{ textAlign: 'center', position: 'relative', overflow: 'hidden' }}>
      {}
      <div style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%', pointerEvents: 'none' }}>
        {confetti.map(c => (
          <div
            key={c.id}
            style={{
              position: 'absolute',
              left: c.left,
              top: '-10px',
              width: '8px', height: '8px',
              borderRadius: '50%',
              background: c.color,
              animation: `fall ${c.duration} ${c.delay} ease-in forwards`
            }}
          />
        ))}
      </div>

      <style>{`
        @keyframes fall {
          to { transform: translateY(400px) rotate(720deg); opacity: 0; }
        }
      `}</style>

      <div style={{ fontSize: '4rem', marginBottom: '0.75rem' }}>{avatar}</div>
      <div className="step-title" style={{ textAlign: 'center' }}>Hazırsın! 🎉</div>
      <div className="step-subtitle" style={{ textAlign: 'center' }}>
        Merhaba, <strong style={{ color: '#fff' }}>{answers.displayName || 'Kahraman'}</strong>! Profiin oluşturuldu.
      </div>

      {}
      <div style={{ textAlign: 'left', marginBottom: '1.5rem' }}>
        <div className="summary-card">
          <div className="summary-icon">🎯</div>
          <div className="summary-info">
            <h4>Odak Alanın</h4>
            <p>{FOCUS_LABELS[answers.primaryFocusArea] || answers.primaryFocusArea || 'Genel'}</p>
          </div>
        </div>
        <div className="summary-card">
          <div className="summary-icon">📋</div>
          <div className="summary-info">
            <h4>Günlük Görev Limitin</h4>
            <p>Günde {dailyLimit} görev, haftada {answers.preferredTasksPerWeek || 5} görev hedefi</p>
          </div>
        </div>
        <div className="summary-card">
          <div className="summary-icon">🔥</div>
          <div className="summary-info">
            <h4>Streak Başladı!</h4>
            <p>1. Gün • Devam et!</p>
          </div>
        </div>
        {result?.firstTaskTitle && (
          <div className="summary-card">
            <div className="summary-icon">✅</div>
            <div className="summary-info">
              <h4>İlk Görev Hazır</h4>
              <p>{result.firstTaskTitle}</p>
            </div>
          </div>
        )}
      </div>

      <button className="btn-primary" style={{ width: '100%', fontSize: '1rem', padding: '1rem' }} onClick={onFinish}>
        🚀 Hadi Başlayalım!
      </button>
    </div>
  );
};

export default StepSummary;
