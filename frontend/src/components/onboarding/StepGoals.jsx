import React from 'react';

const FOCUS_AREAS = [
  { value: 'health',       label: 'Sağlık & Spor',    emoji: '💪', sub: 'Egzersiz, uyku, beslenme' },
  { value: 'career',       label: 'Kariyer & İş',     emoji: '🚀', sub: 'Beceri, networking, verimlilik' },
  { value: 'education',    label: 'Öğrenme',          emoji: '🎓', sub: 'Okuma, kurslar, yeni beceriler' },
  { value: 'social',       label: 'İlişkiler',        emoji: '🤝', sub: 'Aile, arkadaşlar, sosyal hayat' },
  { value: 'mindfulness',  label: 'Zihinsel Sağlık',  emoji: '🧘', sub: 'Meditasyon, stres yönetimi' },
  { value: 'finance',      label: 'Finans',           emoji: '💰', sub: 'Tasarruf, bütçe, yatırım' },
];

const StepGoals = ({ answers, onChange, onNext, onBack }) => {
  const toggleSecondary = (v) => {
    if (v === answers.primaryFocusArea) return;
    const cur = answers.secondaryGoals || [];
    const next = cur.includes(v)
      ? cur.filter(x => x !== v)
      : cur.length < 2 ? [...cur, v] : cur;
    onChange({ secondaryGoals: next });
  };

  return (
    <div>
      <div className="step-title">Ne üzerinde çalışmak istiyorsun? 🎯</div>
      <div className="step-subtitle">Ana odak alanın öncelikli görevleri belirler.</div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.75rem' }}>
        En çok neyi geliştirmek istiyorsun?
      </label>
      <div className="option-cards">
        {FOCUS_AREAS.map(opt => (
          <div
            key={opt.value}
            className={`option-card ${answers.primaryFocusArea === opt.value ? 'selected' : ''}`}
            onClick={() => onChange({ primaryFocusArea: opt.value })}
          >
            <span className="card-emoji">{opt.emoji}</span>
            <div className="card-label">{opt.label}</div>
            <div className="card-sub">{opt.sub}</div>
          </div>
        ))}
      </div>

      {answers.primaryFocusArea && (
        <>
          <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.5rem' }}>
            Bunlar dışında neler var? (Opsiyonel, max 2)
          </label>
          <div className="chips-grid">
            {FOCUS_AREAS
              .filter(f => f.value !== answers.primaryFocusArea)
              .map(f => (
                <div
                  key={f.value}
                  className={`chip ${answers.secondaryGoals?.includes(f.value) ? 'active' : ''}`}
                  onClick={() => toggleSecondary(f.value)}
                >
                  {f.emoji} {f.label}
                </div>
              ))}
          </div>
        </>
      )}

      <div className="step-actions">
        <button className="btn-secondary" onClick={onBack}>← Geri</button>
        <button className="btn-primary" onClick={onNext} disabled={!answers.primaryFocusArea}>
          Devam →
        </button>
      </div>
    </div>
  );
};

export default StepGoals;
