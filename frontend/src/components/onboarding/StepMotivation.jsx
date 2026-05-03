import React from 'react';

const MOTIVATIONS = [
  { value: 'leveling',  label: 'Seviye atlamak',    emoji: '⬆️' },
  { value: 'badges',    label: 'Rozetler kazanmak', emoji: '🏅' },
  { value: 'streaks',   label: 'Seri tutturmak',    emoji: '🔥' },
  { value: 'social',    label: 'Arkadaşları geçmek',emoji: '🏆' },
  { value: 'progress',  label: 'İlerleme görmek',   emoji: '📈' },
  { value: 'variety',   label: 'Çeşitlilik',        emoji: '🎲' },
];

const KILLERS = [
  { value: 'boring_tasks',        label: 'Sıkıcı görevler' },
  { value: 'too_long',            label: 'Çok uzun sürenler' },
  { value: 'no_variety',          label: 'Tekrar eden şeyler' },
  { value: 'too_hard',            label: 'Çok zor gelince' },
  { value: 'no_progress_visible', label: 'İlerleme göremeyince' },
  { value: 'busy_day',            label: 'Yoğun günler' },
  { value: 'forgot',              label: 'Unutunca' },
];

const StepMotivation = ({ answers, onChange, onNext, onBack }) => {
  const toggleKiller = (v) => {
    const cur = answers.motivationKillers || [];
    onChange({
      motivationKillers: cur.includes(v) ? cur.filter(x => x !== v) : [...cur, v]
    });
  };

  return (
    <div>
      <div className="step-title">Seni ne motive eder? ⚡</div>
      <div className="step-subtitle">Bu cevaplar chatbot tonunu ve görev seçimini kişiselleştirir.</div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.75rem' }}>
        En çok neyi seversin?
      </label>
      <div className="option-cards">
        {MOTIVATIONS.map(m => (
          <div
            key={m.value}
            className={`option-card ${answers.motivationFactor === m.value ? 'selected' : ''}`}
            onClick={() => onChange({ motivationFactor: m.value })}
          >
            <span className="card-emoji">{m.emoji}</span>
            <div className="card-label">{m.label}</div>
          </div>
        ))}
      </div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.5rem' }}>
        Hangisi seni kolayca durdurur? <span style={{ opacity: 0.5 }}>(Hepsini seç)</span>
      </label>
      <div className="chips-grid">
        {KILLERS.map(k => (
          <div
            key={k.value}
            className={`chip ${answers.motivationKillers?.includes(k.value) ? 'active' : ''}`}
            onClick={() => toggleKiller(k.value)}
          >
            {k.label}
          </div>
        ))}
      </div>

      <div className="step-actions">
        <button className="btn-secondary" onClick={onBack}>← Geri</button>
        <button className="btn-primary" onClick={onNext} disabled={!answers.motivationFactor}>
          Devam →
        </button>
      </div>
    </div>
  );
};

export default StepMotivation;
