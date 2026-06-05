import { useState } from 'react';
import { moodApi } from '../api/moodApi';
import '../styles/DailyMoodSelector.css';

const MOOD_OPTIONS = [
  {
    key: 0,
    label: 'Enerjik',
    emoji: '⚡',
    description: 'Harika hissediyorum, büyük hedefler koyabilirim!',
    color: '#f59e0b',
    gradient: 'from-amber-400 to-orange-500',
  },
  {
    key: 1,
    label: 'Normal',
    emoji: '😊',
    description: 'Dengeli bir gün, her şey yolunda.',
    color: '#10b981',
    gradient: 'from-emerald-400 to-teal-500',
  },
  {
    key: 2,
    label: 'Yorgun',
    emoji: '😴',
    description: 'Biraz yorgunum, hafif görevler tercih ederim.',
    color: '#6366f1',
    gradient: 'from-indigo-400 to-purple-500',
  },
  {
    key: 3,
    label: 'Bunalmış',
    emoji: '😰',
    description: 'Çok şey var, sadece bir şeye odaklanabilirim.',
    color: '#ef4444',
    gradient: 'from-red-400 to-rose-500',
  },
  {
    key: 4,
    label: 'Vaktim Kısıtlı',
    emoji: '⏰',
    description: 'Az vaktim var, hızlı görevler istiyorum.',
    color: '#3b82f6',
    gradient: 'from-blue-400 to-cyan-500',
  },
];

export default function DailyMoodSelector({ onMoodRecorded }) {
  const [selectedMood, setSelectedMood] = useState(null);
  const [availableMinutes, setAvailableMinutes] = useState(30);
  const [energyLevel, setEnergyLevel] = useState(3);
  const [journalEntry, setJournalEntry] = useState('');
  const [sleepQuality, setSleepQuality] = useState(3);
  const [loading, setLoading] = useState(false);
  const [step, setStep] = useState(1); // 1: mood seç, 2: detaylar

  let maxEnergy = 5;
  let maxMinutes = 240;
  let limitMessage = '';

  if (selectedMood) {
    if (selectedMood.key === 3) { // Bunalmış
      maxEnergy = 2;
      maxMinutes = 15;
      limitMessage = 'Bunalmış hissettiğiniz için sadece en kolay ve kısa görevler (maks 15 dk, 2 enerji) seçilebilir.';
    } else if (selectedMood.key === 2) { // Yorgun
      maxEnergy = 3;
      limitMessage = 'Yorgun hissettiğiniz için yüksek enerjili görevler (maks 3 enerji) sınırlandırılmıştır.';
    } else if (selectedMood.key === 4) { // Vaktim Kısıtlı
      maxMinutes = 20;
      limitMessage = 'Zamanınız kısıtlı olduğu için hızlı görevler (maks 20 dk) sınırlandırılmıştır.';
    }
  }

  const handleMoodSelect = (mood) => {
    setSelectedMood(mood);
    setStep(2);
    // Mood'a göre otomatik enerji ve süre öneri
    if (mood.key === 0) { setEnergyLevel(5); setAvailableMinutes(60); }
    else if (mood.key === 1) { setEnergyLevel(3); setAvailableMinutes(30); }
    else if (mood.key === 2) { setEnergyLevel(2); setAvailableMinutes(20); }
    else if (mood.key === 3) { setEnergyLevel(1); setAvailableMinutes(15); }
    else if (mood.key === 4) { setEnergyLevel(3); setAvailableMinutes(10); }
  };

  const handleSubmit = async () => {
    if (!selectedMood) return;
    setLoading(true);
    const finalMinutes = Math.min(availableMinutes, maxMinutes);
    const finalEnergy = Math.min(energyLevel, maxEnergy);
    try {
      await moodApi.recordDailyMood({
        moodMode: selectedMood.key,
        availableMinutes: finalMinutes,
        energyLevel: finalEnergy,
        journalEntry,
        sleepQuality,
      });
      onMoodRecorded?.({ mood: selectedMood, availableMinutes: finalMinutes, energyLevel: finalEnergy, journalEntry, sleepQuality });
    } catch (err) {
      console.error('Mood kaydedilemedi:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="mood-selector-container">
      {step === 1 && (
        <>
          <div className="mood-selector-header">
            <h2>🌅 Bugün nasılsın?</h2>
            <p>Ruh haline göre en uygun görevleri seçeceğiz</p>
          </div>
          <div className="mood-grid">
            {MOOD_OPTIONS.map((mood) => (
              <button
                key={mood.key}
                className="mood-card"
                style={{ '--mood-color': mood.color }}
                onClick={() => handleMoodSelect(mood)}
              >
                <span className="mood-emoji">{mood.emoji}</span>
                <span className="mood-label">{mood.label}</span>
                <span className="mood-description">{mood.description}</span>
              </button>
            ))}
          </div>
        </>
      )}

      {step === 2 && selectedMood && (
        <div className="mood-details">
          <div className="selected-mood-badge" style={{ '--mood-color': selectedMood.color }}>
            <span>{selectedMood.emoji}</span>
            <span>{selectedMood.label}</span>
          </div>

          {limitMessage && (
            <div className="mood-limit-info-badge">
              ℹ️ {limitMessage}
            </div>
          )}

          <div className="mood-slider-group">
            <label>
              ⚡ Enerji Seviyesi: <strong>{Math.min(energyLevel, maxEnergy)}/5</strong>
            </label>
            <input
              type="range"
              min="1"
              max={maxEnergy}
              value={Math.min(energyLevel, maxEnergy)}
              onChange={(e) => setEnergyLevel(Number(e.target.value))}
              className="mood-slider"
            />
            <div className="slider-labels">
              <span>Düşük</span><span>Orta</span><span>Yüksek</span>
            </div>
          </div>

          <div className="mood-slider-group">
            <label>
              ⏱️ Bugün müsait olduğun süre: <strong>{Math.min(availableMinutes, maxMinutes)} dk</strong>
            </label>
            <input
              type="range"
              min="5"
              max={maxMinutes}
              step="5"
              value={Math.min(availableMinutes, maxMinutes)}
              onChange={(e) => setAvailableMinutes(Number(e.target.value))}
              className="mood-slider"
            />
            <div className="slider-labels">
              <span>5 dk</span><span>{maxMinutes >= 60 ? '1 saat' : `${maxMinutes} dk`}</span><span>{maxMinutes >= 120 ? '2 saat' : `${maxMinutes} dk`}</span>
            </div>
          </div>

          <div className="mood-slider-group">
            <label>
              🌙 Uyku Kalitesi: <strong>{sleepQuality}/5</strong>
            </label>
            <input
              type="range"
              min="1"
              max="5"
              value={sleepQuality}
              onChange={(e) => setSleepQuality(Number(e.target.value))}
              className="mood-slider"
            />
          </div>

          <div className="mood-input-group">
            <label>📝 Günlük Notu / Niyetin (Opsiyonel)</label>
            <textarea
              value={journalEntry}
              onChange={(e) => setJournalEntry(e.target.value)}
              placeholder="Bugün nasıl hissediyorsun? Kendine ne söylemek istersin?"
              className="mood-textarea"
            />
          </div>

          <div className="mood-actions">
            <button className="mood-back-btn" onClick={() => setStep(1)}>← Değiştir</button>
            <button
              className="mood-submit-btn"
              onClick={handleSubmit}
              disabled={loading}
            >
              {loading ? '⌛ Kaydediliyor...' : '🎯 Görevlerimi Getir'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
