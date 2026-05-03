import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { onboardingService } from '../services/onboardingService';
import StepWelcome from '../components/onboarding/StepWelcome';
import StepLifestyle from '../components/onboarding/StepLifestyle';
import StepGoals from '../components/onboarding/StepGoals';
import StepCapacity from '../components/onboarding/StepCapacity';
import StepMotivation from '../components/onboarding/StepMotivation';
import StepDifficulty from '../components/onboarding/StepDifficulty';
import StepFirstTask from '../components/onboarding/StepFirstTask';
import StepSummary from '../components/onboarding/StepSummary';
import './OnboardingPage.css';

const TOTAL_STEPS = 8;

const OnboardingPage = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [answers, setAnswers] = useState({
    displayName: '',
    avatarId: 1,
    lifestyleType: '',
    wakeUpTime: '07:00',
    workDays: [1, 2, 3, 4, 5],
    primaryFocusArea: '',
    secondaryGoals: [],
    dailyAvailableMinutes: 30,
    preferredTaskDuration: 'short_15',
    preferredTasksPerWeek: 5,
    motivationFactor: '',
    motivationKillers: [],
    difficultyPreference: 'medium',
    ageRange: '25-34'
  });

  const updateAnswers = (updates) => setAnswers(prev => ({ ...prev, ...updates }));

  const next = () => setStep(s => Math.min(s + 1, TOTAL_STEPS));
  const prev = () => setStep(s => Math.max(s - 1, 1));

  const handleComplete = async () => {
    setLoading(true);
    try {
      const response = await onboardingService.completeOnboarding(answers);
      setResult(response);
      setStep(8);
    } catch (err) {
      console.error('Onboarding tamamlanırken hata:', err);
    } finally {
      setLoading(false);
    }
  };

  const goToDashboard = () => navigate('/dashboard');

  const progressPct = Math.round((step / TOTAL_STEPS) * 100);

  return (
    <div className="onboarding-page">
      <div className="onboarding-container">
        {/* Progress Bar */}
        <div className="onboarding-progress">
          <div className="progress-header">
            <span className="progress-step">Adım {step} / {TOTAL_STEPS}</span>
            <span className="progress-pct">{progressPct}%</span>
          </div>
          <div className="progress-track">
            <div className="progress-fill" style={{ width: `${progressPct}%` }} />
          </div>
          <div className="step-dots">
            {Array.from({ length: TOTAL_STEPS }, (_, i) => (
              <div
                key={i}
                className={`step-dot ${i + 1 < step ? 'done' : ''} ${i + 1 === step ? 'active' : ''}`}
              />
            ))}
          </div>
        </div>

        {/* Adım İçeriği */}
        <div className="step-content">
          {step === 1 && (
            <StepWelcome answers={answers} onChange={updateAnswers} onNext={next} />
          )}
          {step === 2 && (
            <StepLifestyle answers={answers} onChange={updateAnswers} onNext={next} onBack={prev} />
          )}
          {step === 3 && (
            <StepGoals answers={answers} onChange={updateAnswers} onNext={next} onBack={prev} />
          )}
          {step === 4 && (
            <StepCapacity answers={answers} onChange={updateAnswers} onNext={next} onBack={prev} />
          )}
          {step === 5 && (
            <StepMotivation answers={answers} onChange={updateAnswers} onNext={next} onBack={prev} />
          )}
          {step === 6 && (
            <StepDifficulty
              answers={answers}
              onChange={updateAnswers}
              onNext={handleComplete}
              onBack={prev}
              loading={loading}
            />
          )}
          {step === 7 && (
            <StepFirstTask result={result} onNext={next} onSkip={goToDashboard} />
          )}
          {step === 8 && (
            <StepSummary answers={answers} result={result} onFinish={goToDashboard} />
          )}
        </div>
      </div>
    </div>
  );
};

export default OnboardingPage;
