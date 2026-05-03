import React from 'react';

const DIFFICULTIES = [
  { value: 'easy',   label: 'Kolay Başla',  emoji: '🌱', sub: 'Alışkanlık kurmak için ideal, zamanla zorlaşır' },
  { value: 'medium', label: 'Dengeli',      emoji: '⚖️', sub: 'Hem kolay hem zor karışımı' },
  { value: 'hard',   label: 'Zorla Beni',   emoji: '🔥', sub: 'Konfor alanının dışına çık' },
  { value: 'mixed',  label: 'Sürpriz yap',  emoji: '🎲', sub: 'Sistem karar versin' },
];

const AGE_RANGES = ['18-24','25-34','35-44','45-54','55+'];

const StepDifficulty = ({ answers, onChange, onNext, onBack, loading }) => {
  return (
    <div>
      <div className="step-title">Zorluk tercihin nedir? 💪</div>
      <div className="step-subtitle">Başlangıç zorluğunu ayarlar. İleride otomatik kalibre edilir.</div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.75rem' }}>
        Görevler nasıl olsun?
      </label>
      <div className="option-cards" style={{ gridTemplateColumns: 'repeat(2, 1fr)' }}>
        {DIFFICULTIES.map(d => (
          <div
            key={d.value}
            className={`option-card ${answers.difficultyPreference === d.value ? 'selected' : ''}`}
            onClick={() => onChange({ difficultyPreference: d.value })}
          >
            <span className="card-emoji">{d.emoji}</span>
            <div className="card-label">{d.label}</div>
            <div className="card-sub">{d.sub}</div>
          </div>
        ))}
      </div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.5rem' }}>
        Yaş aralığın?
      </label>
      <div className="chips-grid">
        {AGE_RANGES.map(a => (
          <div
            key={a}
            className={`chip ${answers.ageRange === a ? 'active' : ''}`}
            onClick={() => onChange({ ageRange: a })}
          >
            {a}
          </div>
        ))}
      </div>

      <div className="step-actions">
        <button className="btn-secondary" onClick={onBack} disabled={loading}>← Geri</button>
        <button
          className="btn-primary"
          onClick={onNext}
          disabled={loading || !answers.difficultyPreference}
        >
          {loading ? '⏳ Profil oluşturuluyor...' : 'Tamamla 🎉'}
        </button>
      </div>
    </div>
  );
};

export default StepDifficulty;
