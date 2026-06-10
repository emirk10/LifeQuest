import React, { useState } from 'react';
import GoalProgressRing from './GoalProgressRing';
import goalService from '../services/goalService';

const GoalCard = ({ goal, onUpdate }) => {
  const [loading, setLoading] = useState(false);

  const handleProgress = async (inc) => {
    setLoading(true);
    try {
      await goalService.updateGoalProgress(goal.id, inc);
      onUpdate?.();
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleArchive = async () => {
    if (!window.confirm('Bu hedefi arşivlemek istiyor musun?')) return;
    try {
      await goalService.archiveGoal(goal.id);
      onUpdate?.();
    } catch (err) {
      console.error(err);
    }
  };

  const color = goal.color || '#7c3aed';

  return (
    <div style={{
      background: 'rgba(255,255,255,0.04)',
      border: `1px solid ${color}30`,
      borderLeft: `4px solid ${color}`,
      borderRadius: '16px',
      padding: '1.25rem',
      display: 'flex',
      gap: '1rem',
      alignItems: 'flex-start',
      transition: 'all 0.2s'
    }}>
      {}
      <div style={{ flexShrink: 0 }}>
        <GoalProgressRing
          percent={goal.progressPercent}
          color={color}
          size={72}
          stroke={6}
          icon={goal.icon}
        />
      </div>

      {}
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ fontWeight: 700, color: '#fff', fontSize: '0.95rem', marginBottom: '0.2rem' }}>
          {goal.title}
        </div>
        <div style={{ fontSize: '0.8rem', color: 'rgba(255,255,255,0.45)', marginBottom: '0.6rem' }}>
          {goal.currentValue} / {goal.targetValue} {goal.progressUnit} •{' '}
          {goal.daysRemaining}g kaldı
          {goal.isOnLag && <span style={{ color: '#f59e0b', marginLeft: '0.4rem' }}>⚠️ Geride</span>}
        </div>

        {}
        {goal.milestoneCount > 0 && (
          <div style={{ fontSize: '0.75rem', color: 'rgba(255,255,255,0.35)', marginBottom: '0.5rem' }}>
            🎯 {goal.completedMilestoneCount}/{goal.milestoneCount} milestone
          </div>
        )}

        {}
        {!goal.isCompleted && (
          <div style={{ display: 'flex', gap: '0.4rem', flexWrap: 'wrap' }}>
            {[1, 5, 10].map(inc => (
              <button
                key={inc}
                onClick={() => handleProgress(inc)}
                disabled={loading}
                style={{
                  padding: '0.25rem 0.6rem',
                  fontSize: '0.75rem',
                  background: `${color}20`,
                  border: `1px solid ${color}50`,
                  borderRadius: '8px',
                  color: '#fff',
                  cursor: 'pointer'
                }}
              >
                +{inc}
              </button>
            ))}
            <button
              onClick={handleArchive}
              style={{
                padding: '0.25rem 0.6rem',
                fontSize: '0.75rem',
                background: 'rgba(255,255,255,0.05)',
                border: '1px solid rgba(255,255,255,0.1)',
                borderRadius: '8px',
                color: 'rgba(255,255,255,0.4)',
                cursor: 'pointer',
                marginLeft: 'auto'
              }}
            >
              Arşivle
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default GoalCard;
