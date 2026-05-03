import React from 'react';

const DURATIONS = [
  { value: 'micro_5',  label: 'Mini (5-10 dk)',   sub: 'Hızlı, odaklanmış' },
  { value: 'short_15', label: 'Kısa (15 dk)',     sub: 'Kahve molası kadar' },
  { value: 'medium_30',label: 'Orta (30 dk)',     sub: 'Bir oturum' },
  { value: 'long_60',  label: 'Uzun (60+ dk)',    sub: 'Derinlemesine' },
  { value: 'any',      label: 'Farketmez',        sub: 'Sisteme bırak' },
];

const WEEKLY_OPTIONS = [
  { value: 3,  label: '3 görev',  sub: 'Yavaş başla' },
  { value: 5,  label: '5 görev',  sub: 'Dengeli' },
  { value: 7,  label: '7 görev',  sub: 'Her gün bir şey' },
  { value: 10, label: '10 görev', sub: 'Aktif' },
  { value: 14, label: '14+ görev',sub: 'Maksimum' },
];

const StepCapacity = ({ answers, onChange, onNext, onBack }) => {
  return (
    <div>
      <div className="step-title">Ne kadar süre ayırabilirsin? ⏱️</div>
      <div className="step-subtitle">Bu cevaplar günlük görev limitini ve önerileri belirler.</div>

      <div className="slider-row">
        <div className="slider-label">
          <span>Günde kendinize ne kadar zaman?</span>
          <strong>{answers.dailyAvailableMinutes} dk</strong>
        </div>
        <input
          type="range" min={5} max={120} step={5}
          value={answers.dailyAvailableMinutes}
          onChange={e => onChange({ dailyAvailableMinutes: Number(e.target.value) })}
        />
        <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.75rem', color: 'rgba(255,255,255,0.3)', marginTop: '0.3rem' }}>
          <span>5 dk</span><span>30 dk</span><span>60 dk</span><span>2 saat</span>
        </div>
      </div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.75rem' }}>
        Tek seferde ne kadar süren görevler yaparsın?
      </label>
      <div className="option-cards" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
        {DURATIONS.map(d => (
          <div
            key={d.value}
            className={`option-card ${answers.preferredTaskDuration === d.value ? 'selected' : ''}`}
            onClick={() => onChange({ preferredTaskDuration: d.value })}
          >
            <div className="card-label">{d.label}</div>
            <div className="card-sub">{d.sub}</div>
          </div>
        ))}
      </div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.75rem' }}>
        Haftada kaç görev hedefliyorsun?
      </label>
      <div className="chips-grid">
        {WEEKLY_OPTIONS.map(o => (
          <div
            key={o.value}
            className={`chip ${answers.preferredTasksPerWeek === o.value ? 'active' : ''}`}
            onClick={() => onChange({ preferredTasksPerWeek: o.value })}
          >
            {o.label}
          </div>
        ))}
      </div>

      <div className="step-actions">
        <button className="btn-secondary" onClick={onBack}>← Geri</button>
        <button className="btn-primary" onClick={onNext}>Devam →</button>
      </div>
    </div>
  );
};

export default StepCapacity;
