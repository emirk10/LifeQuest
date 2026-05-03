import React from 'react';

const AVATARS = ['🦁','🐯','🦊','🐺','🦅','🦋','🐉','🦄','🐬','🦈','🐸','🐼','🐻','🦝','🐮','🐧','🦩','🐙','🦕','🤖'];

const StepWelcome = ({ answers, onChange, onNext }) => {
  return (
    <div>
      <div className="step-title">Hoş geldin! 👋</div>
      <div className="step-subtitle">Seni tanımak istiyoruz. Bu birkaç adım, deneyimini tamamen kişiselleştirmemizi sağlayacak.</div>

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.5rem' }}>
        Seni nasıl çağıralım?
      </label>
      <input
        className="onboarding-input"
        placeholder="Takma isim veya ad"
        value={answers.displayName}
        onChange={e => onChange({ displayName: e.target.value })}
      />

      <label style={{ color: 'rgba(255,255,255,0.65)', fontSize: '0.85rem', display: 'block', marginBottom: '0.75rem' }}>
        Bir avatar seç
      </label>
      <div className="avatar-grid">
        {AVATARS.map((emoji, i) => (
          <div
            key={i}
            className={`avatar-item ${answers.avatarId === i + 1 ? 'active' : ''}`}
            onClick={() => onChange({ avatarId: i + 1 })}
          >
            {emoji}
          </div>
        ))}
      </div>

      <div className="step-actions">
        <button
          className="btn-primary"
          onClick={onNext}
          disabled={!answers.displayName.trim()}
        >
          Devam →
        </button>
      </div>
    </div>
  );
};

export default StepWelcome;
