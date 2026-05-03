import React from 'react';

const LIFESTYLES = [
  { value: 'student',      label: 'Öğrenci',    emoji: '📚' },
  { value: 'professional', label: 'Çalışan',    emoji: '💼' },
  { value: 'freelancer',   label: 'Freelancer', emoji: '💻' },
  { value: 'homemaker',    label: 'Ev/Aile',    emoji: '🏠' },
  { value: 'other',        label: 'Diğer',      emoji: '✨' },
];

const WAKE_TIMES = ['05:00','06:00','07:00','08:00','09:00','10:00','11:00+'];
const DAYS = ['Paz','Pzt','Sal','Çar','Per','Cum','Cmt'];

const StepLifestyle = ({ answers, onChange, onNext, onBack }) => {
  const toggleDay = (d) => {
    const current = answers.workDays || [];
    const updated = current.includes(d) ? current.filter(x => x !== d) : [...current, d];
    onChange({ workDays: updated });
  };

  return (
    <div>
      <div className="step-title">Hayatın nasıl bir ritmi var? 🌅</div>
      <div className="step-subtitle">Bu cevaplar görev zamanlamanı ve haftalık planını şekillendirir.</div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.75rem' }}>
        Kendinizi nasıl tanımlarsınız?
      </label>
      <div className="option-cards" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
        {LIFESTYLES.map(opt => (
          <div
            key={opt.value}
            className={`option-card ${answers.lifestyleType === opt.value ? 'selected' : ''}`}
            onClick={() => onChange({ lifestyleType: opt.value })}
          >
            <span className="card-emoji">{opt.emoji}</span>
            <div className="card-label">{opt.label}</div>
          </div>
        ))}
      </div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.5rem' }}>
        Genellikle saat kaçta uyanırsınız?
      </label>
      <div className="chips-grid" style={{ marginBottom: '1.5rem' }}>
        {WAKE_TIMES.map(t => (
          <div
            key={t}
            className={`chip ${answers.wakeUpTime === t ? 'active' : ''}`}
            onClick={() => onChange({ wakeUpTime: t.replace('+', '') })}
          >
            {t}
          </div>
        ))}
      </div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.5rem' }}>
        Hangi günler daha yoğun geçiyor?
      </label>
      <div className="chips-grid">
        {DAYS.map((d, i) => (
          <div
            key={i}
            className={`chip ${answers.workDays?.includes(i) ? 'active' : ''}`}
            onClick={() => toggleDay(i)}
          >
            {d}
          </div>
        ))}
      </div>

      <div className="step-actions">
        <button className="btn-secondary" onClick={onBack}>← Geri</button>
        <button className="btn-primary" onClick={onNext} disabled={!answers.lifestyleType}>
          Devam →
        </button>
      </div>
    </div>
  );
};

export default StepLifestyle;
