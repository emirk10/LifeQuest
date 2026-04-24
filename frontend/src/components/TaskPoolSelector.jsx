import { useState } from 'react';
import EnhancedTaskCard from './EnhancedTaskCard';
import '../styles/TaskPoolSelector.css';

const MOOD_LABELS = {
  0: { label: 'Enerjik', emoji: '⚡', color: '#f59e0b' },
  1: { label: 'Normal', emoji: '😊', color: '#10b981' },
  2: { label: 'Yorgun', emoji: '😴', color: '#6366f1' },
  3: { label: 'Bunalmış', emoji: '😰', color: '#ef4444' },
  4: { label: 'Vaktim Kısıtlı', emoji: '⏰', color: '#3b82f6' },
};

const SLOT_LABELS = {
  0: 'Sabah',
  1: 'Öğle',
  2: 'Öğleden Sonra',
  3: 'Akşam',
  4: 'Gece',
};

export default function TaskPoolSelector({ poolData, onTaskSelect, onRefresh }) {
  const [selectedIds, setSelectedIds] = useState(new Set());
  const [filter, setFilter] = useState('all'); // all | quick | deep

  if (!poolData) return null;

  const mood = MOOD_LABELS[poolData.todayMood] || MOOD_LABELS[1];
  const slot = SLOT_LABELS[poolData.timeSlot] || 'Gün';

  const filteredTasks = poolData.tasks?.filter((t) => {
    if (filter === 'quick') return t.estimatedMinutes && t.estimatedMinutes <= 20;
    if (filter === 'deep') return t.estimatedMinutes && t.estimatedMinutes > 20;
    return true;
  }) || [];

  const handleSelect = async (task) => {
    if (selectedIds.has(task.userTaskId)) return;
    setSelectedIds((prev) => new Set([...prev, task.userTaskId]));
    await onTaskSelect?.(task);
  };

  return (
    <div className="task-pool-container">
      {/* Header */}
      <div className="task-pool-header">
        <div className="pool-context">
          <div className="pool-mood-badge" style={{ '--mood-color': mood.color }}>
            {mood.emoji} {mood.label}
          </div>
          <div className="pool-meta">
            <span>🌅 {slot}</span>
            <span>⏱️ {poolData.availableMinutes} dk müsait</span>
            <span>⚡ Enerji {poolData.energyLevel}/5</span>
          </div>
        </div>

        <div className="pool-filters">
          {['all', 'quick', 'deep'].map((f) => (
            <button
              key={f}
              className={`filter-chip ${filter === f ? 'active' : ''}`}
              onClick={() => setFilter(f)}
            >
              {f === 'all' ? 'Tümü' : f === 'quick' ? '⚡ Hızlı (≤20dk)' : '🧠 Odak (>20dk)'}
            </button>
          ))}
        </div>

        <button className="pool-refresh-btn" onClick={onRefresh}>
          🔄 Yenile
        </button>
      </div>

      {/* Task Grid */}
      {filteredTasks.length === 0 ? (
        <div className="pool-empty">
          <span>🎉</span>
          <p>Bu filtre için görev bulunamadı</p>
        </div>
      ) : (
        <div className="task-pool-grid">
          {filteredTasks.map((task) => (
            <EnhancedTaskCard
              key={task.userTaskId}
              task={task}
              isSelected={selectedIds.has(task.userTaskId)}
              onSelect={() => handleSelect(task)}
            />
          ))}
        </div>
      )}

      {selectedIds.size > 0 && (
        <div className="selection-summary">
          ✅ {selectedIds.size} görev seçildi — Harika! Başarılar!
        </div>
      )}
    </div>
  );
}
