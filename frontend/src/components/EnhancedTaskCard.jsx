import { useState } from 'react';
import { taskPoolApi } from '../api/moodApi';
import '../styles/EnhancedTaskCard.css';

const DIFFICULTY_CONFIG = {
  Easy: { label: 'Kolay', color: '#10b981', stars: 1 },
  Medium: { label: 'Orta', color: '#f59e0b', stars: 2 },
  Hard: { label: 'Zor', color: '#ef4444', stars: 3 },
};

export default function EnhancedTaskCard({ task, isSelected, onSelect }) {
  const [actionLoading, setActionLoading] = useState(null);
  const [status, setStatus] = useState('suggested'); // suggested | selected | inprogress | paused | postponed
  const [showPostpone, setShowPostpone] = useState(false);
  const [postponeReason, setPostponeReason] = useState('');

  const diff = DIFFICULTY_CONFIG[task.difficulty] || DIFFICULTY_CONFIG.Easy;
  const matchPercent = Math.round((task.matchScore || 0) * 100);

  const handleSelect = async () => {
    setActionLoading('select');
    try {
      await taskPoolApi.selectTask(task.userTaskId);
      setStatus('selected');
      onSelect?.();
    } finally {
      setActionLoading(null);
    }
  };

  const handlePause = async () => {
    setActionLoading('pause');
    try {
      await taskPoolApi.pauseTask(task.userTaskId);
      setStatus('paused');
    } finally {
      setActionLoading(null);
    }
  };

  const handleResume = async () => {
    setActionLoading('resume');
    try {
      await taskPoolApi.resumeTask(task.userTaskId);
      setStatus('inprogress');
    } finally {
      setActionLoading(null);
    }
  };

  const handlePostpone = async () => {
    setActionLoading('postpone');
    try {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      const dateStr = tomorrow.toISOString().split('T')[0];
      await taskPoolApi.postponeTask(task.userTaskId, dateStr, postponeReason);
      setStatus('postponed');
      setShowPostpone(false);
    } finally {
      setActionLoading(null);
    }
  };

  const handleAbandon = async () => {
    if (!window.confirm('Bu görevden vazgeçmek istediğinden emin misin?')) return;
    setActionLoading('abandon');
    try {
      await taskPoolApi.abandonTask(task.userTaskId, 'Kullanıcı vazgeçti');
      setStatus('abandoned');
    } finally {
      setActionLoading(null);
    }
  };

  if (status === 'abandoned') {
    return (
      <div className="task-card task-card--abandoned">
        <span>🚫 Bu görevden vazgeçildi</span>
      </div>
    );
  }

  if (status === 'postponed') {
    return (
      <div className="task-card task-card--postponed">
        <span>📅 Yarına ertelendi — XP %70 olacak</span>
      </div>
    );
  }

  return (
    <div className={`task-card ${isSelected ? 'task-card--selected' : ''} task-card--${status}`}>
      {/* Match Score Badge */}
      <div className="task-match-badge">
        <div
          className="match-ring"
          style={{ '--match': matchPercent, '--match-color': matchPercent > 70 ? '#10b981' : matchPercent > 40 ? '#f59e0b' : '#6366f1' }}
        >
          <span>{matchPercent}%</span>
        </div>
        <span className="match-label">uyum</span>
      </div>

      {/* Card Header */}
      <div className="task-card-header">
        <span className="task-category">{task.categoryName}</span>
        <span className="task-difficulty" style={{ color: diff.color }}>
          {'★'.repeat(diff.stars)}{'☆'.repeat(3 - diff.stars)} {diff.label}
        </span>
      </div>

      {/* Title & Description */}
      <h3 className="task-title">{task.title}</h3>
      <p className="task-description">{task.description}</p>

      {/* Why This Task */}
      {task.whyThisTask && (
        <div className="task-why">
          <span>💡</span>
          <span>{task.whyThisTask}</span>
        </div>
      )}

      {/* Recommendation Reason */}
      {task.recommendationReason && (
        <div className="task-reason">
          🎯 {task.recommendationReason}
        </div>
      )}

      {/* Meta */}
      <div className="task-meta">
        {task.estimatedMinutes && <span>⏱️ {task.estimatedMinutes} dk</span>}
        <span>✨ {task.baseXP} XP</span>
      </div>

      {/* Badge Hint */}
      {task.badgeHint && (
        <div className="task-badge-hint">
          🏅 {task.badgeHint}
        </div>
      )}

      {/* Actions */}
      <div className="task-actions">
        {status === 'suggested' && (
          <button
            className="task-btn task-btn--primary"
            onClick={handleSelect}
            disabled={!!actionLoading}
          >
            {actionLoading === 'select' ? '⌛' : '✅ Seç'}
          </button>
        )}

        {status === 'selected' && (
          <button
            className="task-btn task-btn--start"
            onClick={() => setStatus('inprogress')}
          >
            ▶️ Başla
          </button>
        )}

        {status === 'inprogress' && (
          <button
            className="task-btn task-btn--pause"
            onClick={handlePause}
            disabled={!!actionLoading}
          >
            {actionLoading === 'pause' ? '⌛' : '⏸️ Duraklat'}
          </button>
        )}

        {status === 'paused' && (
          <button
            className="task-btn task-btn--resume"
            onClick={handleResume}
            disabled={!!actionLoading}
          >
            {actionLoading === 'resume' ? '⌛' : '▶️ Devam Et'}
          </button>
        )}

        {/* Erteleme butonu her durumda görünür */}
        {['suggested', 'selected', 'paused'].includes(status) && (
          <button
            className="task-btn task-btn--postpone"
            onClick={() => setShowPostpone(!showPostpone)}
          >
            📅 Ertele
          </button>
        )}

        {['selected', 'inprogress', 'paused'].includes(status) && (
          <button
            className="task-btn task-btn--abandon"
            onClick={handleAbandon}
            disabled={!!actionLoading}
          >
            {actionLoading === 'abandon' ? '⌛' : '🚫'}
          </button>
        )}
      </div>

      {/* Postpone Panel */}
      {showPostpone && (
        <div className="postpone-panel">
          <p>Neden erteliyorsun? (isteğe bağlı)</p>
          <input
            type="text"
            placeholder="Sebebi yaz..."
            value={postponeReason}
            onChange={(e) => setPostponeReason(e.target.value)}
            className="postpone-input"
          />
          <div className="postpone-actions">
            <button
              className="task-btn task-btn--primary"
              onClick={handlePostpone}
              disabled={actionLoading === 'postpone'}
            >
              {actionLoading === 'postpone' ? '⌛' : 'Yarına Ertele (%70 XP)'}
            </button>
            <button
              className="task-btn"
              onClick={() => setShowPostpone(false)}
            >
              İptal
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
