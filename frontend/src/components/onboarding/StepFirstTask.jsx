import React from 'react';

const StepFirstTask = ({ result, onNext, onSkip }) => {
  if (!result?.firstTaskTitle) {
    return (
      <div>
        <div className="step-title">İlk görevin hazır! 🚀</div>
        <div className="step-subtitle">Profilin oluşturuldu. Dashboard'dan görev havuzuna erişebilirsin.</div>
        <div className="step-actions">
          <button className="btn-primary" onClick={onNext}>Devam →</button>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="step-title">İlk görevini birlikte yapalım! 🌟</div>
      <div className="step-subtitle">Profiline özel bir görev seçtik. Şimdi tamamlayarak ilk XP'ni kazan!</div>

      <div style={{
        background: 'rgba(124,58,237,0.15)',
        border: '1px solid rgba(124,58,237,0.3)',
        borderRadius: '16px',
        padding: '1.5rem',
        marginBottom: '1.5rem',
        textAlign: 'center'
      }}>
        <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>🎯</div>
        <div style={{ fontSize: '1.1rem', fontWeight: '700', color: '#fff', marginBottom: '0.3rem' }}>
          {result.firstTaskTitle}
        </div>
        <div style={{ fontSize: '0.85rem', color: 'rgba(255,255,255,0.5)' }}>
          Profil önerin • +50 XP
        </div>
      </div>

      <div style={{ display: 'flex', gap: '0.75rem', flexDirection: 'column' }}>
        <button className="btn-primary" style={{ width: '100%', textAlign: 'center' }} onClick={onNext}>
          ✅ Bu görevi kabul ediyorum!
        </button>
        <button
          style={{ background: 'none', border: 'none', color: 'rgba(255,255,255,0.4)', cursor: 'pointer', fontSize: '0.85rem' }}
          onClick={onSkip}
        >
          Şimdi değil, Dashboard'a git →
        </button>
      </div>
    </div>
  );
};

export default StepFirstTask;
